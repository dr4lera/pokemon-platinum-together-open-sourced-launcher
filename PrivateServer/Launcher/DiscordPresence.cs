using System.Buffers.Binary;
using System.IO.Pipes;
using System.Text.Json;
using System.Collections.Concurrent;

namespace PlatinumPrivateLauncher;

// Local Discord IPC only. No Discord account tokens or OAuth credentials are used.
sealed class DiscordPresence : IAsyncDisposable
{
    public const string ApplicationId = "1561916972508385380";
    public const string ProjectUrl = "https://github.com/dr4lera/pokemon-platinum-together-open-sourced-launcher";
    readonly CancellationTokenSource stop = new();
    readonly Func<string> state;
    readonly Action<string> status;
    readonly string asset;
    readonly Func<DiscordParty?> party;
    readonly Action<string>? join;
    readonly long started = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    readonly Task worker;

    public DiscordPresence(Func<string> state, Action<string> status, string asset = "logo", Func<DiscordParty?>? party = null, Action<string>? join = null)
    {
        this.state = state; this.status = status; this.asset = asset;
        this.party = party ?? (() => null); this.join = join;
        worker = Run();
    }

    internal static object Activity(string state, string asset, long started, DiscordParty? party = null) => new
    {
        type = 0,
        details = "Plat Together Open Sourced",
        state,
        timestamps = new { start = started },
        assets = new { large_image = asset, large_text = "Plat Together Open Sourced" },
        party = party is null ? null : new { id = party.Id, size = new[] { party.Size, party.Max } },
        secrets = party is null ? null : new { join = party.Secret },
        instance = party is not null,
        buttons = new[] {
            new { label = "GitHub & setup", url = ProjectUrl },
            new { label = "Host a server", url = "https://github.com/dr4lera/pokemon-platinum-together-open-sourced-server" }
        }
    };

    internal static async Task Write(Stream stream, int opcode, byte[] bytes, CancellationToken token)
    {
        if (bytes.Length > 1024 * 1024) throw new IOException("Discord frame exceeds limit.");
        var frame = new byte[8 + bytes.Length];
        BinaryPrimitives.WriteInt32LittleEndian(frame, opcode);
        BinaryPrimitives.WriteInt32LittleEndian(frame.AsSpan(4), bytes.Length);
        bytes.CopyTo(frame.AsSpan(8));
        await stream.WriteAsync(frame, token);
        await stream.FlushAsync(token);
    }

    internal static string? JoinSecret(JsonElement root)
    {
        if (!root.TryGetProperty("cmd", out var cmd) || cmd.GetString() != "DISPATCH" ||
            !root.TryGetProperty("evt", out var evt) || evt.GetString() != "ACTIVITY_JOIN" ||
            !root.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Object ||
            !data.TryGetProperty("secret", out var secret) || secret.ValueKind != JsonValueKind.String) return null;
        var value = secret.GetString();
        return DiscordJoinAddress.Valid(value) ? value : null;
    }

    internal sealed class Connection(Stream pipe, Action<string>? join)
    {
        readonly ConcurrentDictionary<string, TaskCompletionSource> pending = new();
        readonly SemaphoreSlim writer = new(1, 1);
        async Task Send(int opcode, byte[] body, CancellationToken token)
        {
            await writer.WaitAsync(token);
            try { await Write(pipe, opcode, body, token); } finally { writer.Release(); }
        }
        public async Task Receive(CancellationToken token)
        {
            try {
                while (true) {
                    var frame = await Read(pipe, token);
                    if (frame.Opcode == 3) { await Send(4, frame.Body, token); continue; }
                    if (frame.Opcode != 1) throw new IOException("Discord disconnected.");
                    using var doc = JsonDocument.Parse(frame.Body);var root = doc.RootElement;
                    if (JoinSecret(root) is string secret) join?.Invoke(secret);
                    if (root.TryGetProperty("nonce", out var n) && n.ValueKind == JsonValueKind.String && pending.TryGetValue(n.GetString()!, out var response)) {
                        if (root.TryGetProperty("evt", out var evt) && evt.GetString() == "ERROR") {
                            var code = root.TryGetProperty("data", out var error) && error.TryGetProperty("code", out var number) ? number.ToString() : "unknown";
                            response.TrySetException(new IOException("Discord rejected " + (root.TryGetProperty("cmd", out var command) ? command.GetString() : "command") + " (code " + code + ")."));
                        }
                        else response.TrySetResult();
                    }
                }
            } finally { foreach(var response in pending.Values) response.TrySetException(new IOException("Discord disconnected.")); }
        }
        public async Task Command(string cmd, object? args, string? evt, CancellationToken token)
        {
            var nonce = Guid.NewGuid().ToString("N");var response = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);pending[nonce] = response;
            try {
                await Send(1, JsonSerializer.SerializeToUtf8Bytes(new {cmd,args,evt,nonce}, new JsonSerializerOptions {DefaultIgnoreCondition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull}), token);
                await response.Task.WaitAsync(TimeSpan.FromSeconds(5), token);
            } finally { pending.TryRemove(nonce, out _); }
        }
    }

    internal static async Task<(int Opcode, byte[] Body)> Read(Stream stream, CancellationToken token)
    {
        var header = new byte[8];
        await stream.ReadExactlyAsync(header, token);
        var length = BinaryPrimitives.ReadInt32LittleEndian(header.AsSpan(4));
        if (length < 0 || length > 1024 * 1024) throw new IOException("Invalid Discord frame length.");
        var body = new byte[length];
        await stream.ReadExactlyAsync(body, token);
        return (BinaryPrimitives.ReadInt32LittleEndian(header), body);
    }

    static async Task<JsonElement> Response(Stream pipe, string? nonce, CancellationToken token)
    {
        while (true)
        {
            var frame = await Read(pipe, token);
            if (frame.Opcode == 3) { await Write(pipe, 4, frame.Body, token); continue; }
            if (frame.Opcode == 2) throw new IOException("Discord closed the presence connection.");
            if (frame.Opcode != 1) throw new IOException("Unexpected Discord response.");
            using var doc = JsonDocument.Parse(frame.Body);
            var root = doc.RootElement;
            if (root.TryGetProperty("evt", out var evt) && evt.GetString() == "ERROR")
                throw new IOException("Discord rejected Rich Presence (check application ID and assets).");
            if (nonce is null ? evt.ValueKind == JsonValueKind.String && evt.GetString() == "READY"
                : root.TryGetProperty("nonce", out var n) && n.GetString() == nonce)
                return root.Clone();
        }
    }

    async Task Run()
    {
        while (!stop.IsCancellationRequested)
        {
            NamedPipeClientStream? pipe = null;
            try
            {
                for (int i = 0; i < 10; i++)
                {
                    var candidate = new NamedPipeClientStream(".", "discord-ipc-" + i, PipeDirection.InOut, PipeOptions.Asynchronous);
                    try { await candidate.ConnectAsync(150, stop.Token); pipe = candidate; break; }
                    catch (Exception e) when (e is TimeoutException or IOException) { candidate.Dispose(); }
                    catch { candidate.Dispose(); throw; }
                }
                if (pipe is null) throw new IOException("Discord is not open; waiting to reconnect.");
                using (var timeout = CancellationTokenSource.CreateLinkedTokenSource(stop.Token))
                {
                    timeout.CancelAfter(TimeSpan.FromSeconds(5));
                    await Write(pipe, 0, JsonSerializer.SerializeToUtf8Bytes(new { v = 1, client_id = ApplicationId }), timeout.Token);
                    await Response(pipe, null, timeout.Token);
                }
                using var sessionStop = CancellationTokenSource.CreateLinkedTokenSource(stop.Token);
                var connection = new Connection(pipe, join);var reader = connection.Receive(sessionStop.Token);
                try {
                    await connection.Command("SUBSCRIBE", null, "ACTIVITY_JOIN", stop.Token);
                    while (!stop.IsCancellationRequested) {
                        var current = party();
                        await connection.Command("SET_ACTIVITY", new {pid=Environment.ProcessId,activity=Activity(state(), asset, started, current)}, null, stop.Token);
                        status(current is null ? "Discord: Rich Presence accepted" : "Discord: party invites ready");
                        await Task.WhenAny(Task.Delay(TimeSpan.FromSeconds(15), stop.Token), reader);
                        if (reader.IsCompleted) await reader;
                    }
                } finally {
                    if (pipe.IsConnected && !reader.IsCompleted) try { using var clear = new CancellationTokenSource(600);await connection.Command("SET_ACTIVITY", new {pid=Environment.ProcessId,activity=(object?)null}, null, clear.Token); } catch { }
                    sessionStop.Cancel();try { await reader; } catch { }
                }
            }
            catch (OperationCanceledException) when (stop.IsCancellationRequested) { }
            catch (Exception e) when (e is IOException or TimeoutException or OperationCanceledException or JsonException)
            { status(e is IOException ? e.Message : "Discord unavailable; retrying shortly."); }
            finally
            {
                if (pipe is not null)
                {
                    pipe.Dispose();
                }
            }
            try { await Task.Delay(TimeSpan.FromSeconds(15), stop.Token); }
            catch (OperationCanceledException) { }
        }
    }

    public async ValueTask DisposeAsync() { stop.Cancel(); await worker; stop.Dispose(); }
}

internal sealed record DiscordParty(string Id, int Size, int Max, string Secret);
internal static class DiscordJoinAddress
{
    internal static bool Valid(string? value) => value is not null && System.Text.Encoding.UTF8.GetByteCount(value) <= 128 &&
        Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme == "https" && uri.UserInfo == "" && uri.Query == "" && uri.Fragment == "" &&
        System.Text.RegularExpressions.Regex.IsMatch(uri.AbsolutePath, "^/private/j/[A-Za-z0-9_-]{22}$") &&
        uri.HostNameType == UriHostNameType.Dns && uri.Host != "localhost" && !uri.Host.EndsWith(".localhost", StringComparison.OrdinalIgnoreCase);
}

