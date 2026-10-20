using System.IO.Pipes;
using System.Text;
using System.Security.Cryptography;

namespace PlatinumPrivateLauncher;
static class DiscordLaunch
{
    internal static string PipeName => "PlatTogetherInvite-" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(AppContext.BaseDirectory)))[..20];
    internal static string? Secret(string[] args)
    {
        var index = Array.IndexOf(args, "--join");
        if (index >= 0 && index + 1 < args.Length && DiscordJoinAddress.Valid(args[index + 1])) return args[index + 1];
        index = Array.IndexOf(args, "--discord-launch");
        if (index >= 0 && index + 1 < args.Length && Uri.TryCreate(args[index + 1], UriKind.Absolute, out var uri) && uri.Scheme == "discord-" + DiscordPresence.ApplicationId)
            foreach (var part in uri.Query.TrimStart('?').Split('&')) {
                if (!part.StartsWith("secret=", StringComparison.Ordinal)) continue;
                var value = Uri.UnescapeDataString(part[7..]);if (DiscordJoinAddress.Valid(value)) return value;
            }
        return null;
    }
    internal static async Task Forward(string? secret)
    {
        using var pipe = new NamedPipeClientStream(".", PipeName, PipeDirection.Out, PipeOptions.Asynchronous);
        using var timeout = new CancellationTokenSource(3000);await pipe.ConnectAsync(timeout.Token);
        await pipe.WriteAsync(Encoding.UTF8.GetBytes(secret ?? "focus"), timeout.Token);
    }
    internal static async Task Listen(Launcher launcher, CancellationToken token)
    {
        while (!token.IsCancellationRequested) {
            try {
                using var pipe = new NamedPipeServerStream(PipeName, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);
                await pipe.WaitForConnectionAsync(token);using var deadline = CancellationTokenSource.CreateLinkedTokenSource(token);deadline.CancelAfter(2000);
                var bytes = new byte[1025];int count = 0, n;
                while (count < bytes.Length && (n = await pipe.ReadAsync(bytes.AsMemory(count), deadline.Token)) > 0) count += n;
                if (count >= bytes.Length) continue;
                var value = Encoding.UTF8.GetString(bytes, 0, count);
                if (!launcher.IsDisposed && launcher.IsHandleCreated) launcher.BeginInvoke(() => {launcher.Show();launcher.WindowState=FormWindowState.Normal;launcher.Activate();if (DiscordJoinAddress.Valid(value)) launcher.ReceiveDiscordInvite(value);});
            } catch (OperationCanceledException) when (token.IsCancellationRequested) { break; }
            catch (Exception e) when (e is IOException or OperationCanceledException or InvalidOperationException) { }
        }
    }
}
