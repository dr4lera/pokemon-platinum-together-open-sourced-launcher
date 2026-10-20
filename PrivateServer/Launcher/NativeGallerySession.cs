using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace PlatinumPrivateLauncher;

// Owns only the launcher-created loopback gallery helper. The helper also watches
// the attached game PID so that closing the launcher cannot leave a proxy behind.
internal sealed class NativeGallerySession : IDisposable
{
    readonly Process helper;
    readonly string readyFile;
    readonly object gate = new();
    Process? game;
    int disposed;

    internal string Url { get; }

    NativeGallerySession(Process helper, string readyFile, string url)
    {
        this.helper = helper;
        this.readyFile = readyFile;
        Url = url;
    }

    internal static async Task<NativeGallerySession> Start(ProcessStartInfo start, string readyFile)
    {
        readyFile = ReadyPath(readyFile);
        CheckOwnedPaths(readyFile);
        if (File.Exists(readyFile) || File.Exists(readyFile + ".tmp"))
            throw new IOException("The gallery helper launch files already exist. Try launching again.");
        if (!Directory.Exists(Path.GetDirectoryName(readyFile)))
            throw new IOException("The gallery helper launch directory is missing.");

        start.UseShellExecute = false;
        start.CreateNoWindow = true;
        start.RedirectStandardInput = true;
        start.RedirectStandardOutput = true;
        start.RedirectStandardError = true;
        var process = new Process { StartInfo = start };
        NativeGallerySession? session = null;
        try
        {
            if (!process.Start()) throw new IOException("The gallery helper could not start.");
            // Never retain or print helper output: command diagnostics may contain
            // private profile information, and an unread pipe can block startup.
            _ = Drain(process.StandardOutput.BaseStream);
            _ = Drain(process.StandardError.BaseStream);
            var timer = Stopwatch.StartNew();
            while (timer.Elapsed < TimeSpan.FromSeconds(12))
            {
                if (process.HasExited) throw new IOException("The gallery helper stopped before it was ready.");
                CheckOwnedPaths(readyFile);
                if (File.Exists(readyFile))
                {
                    if (new FileInfo(readyFile).Length > 8192) throw new IOException("The gallery helper returned an invalid launch response.");
                    try
                    {
                        var url = ValidateReady(await File.ReadAllBytesAsync(readyFile).ConfigureAwait(false), process.Id);
                        if (process.HasExited) throw new IOException("The gallery helper stopped before it was ready.");
                        session = new NativeGallerySession(process, readyFile, url);
                        return session;
                    }
                    catch (JsonException)
                    {
                        // The helper normally renames an atomic stage. Tolerate a
                        // transient partial read without accepting unverified data.
                    }
                }
                await Task.Delay(75).ConfigureAwait(false);
            }
            throw new IOException("The gallery helper did not become ready within 12 seconds.");
        }
        catch
        {
            if (session is null)
            {
                StopOwnedHelper(process);
                Cleanup(readyFile);
            }
            throw;
        }
    }

    internal void AttachGame(Process process)
    {
        ArgumentNullException.ThrowIfNull(process);
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(disposed != 0, this);
            if (game is not null) throw new InvalidOperationException("The gallery helper already has a game attached.");
            if (process.HasExited) throw new IOException("The private game stopped before its gallery helper was attached.");
            if (helper.HasExited) throw new IOException("The gallery helper stopped before the private game was attached.");
            try
            {
                helper.StandardInput.WriteLine(JsonSerializer.Serialize(new { game_pid = process.Id }));
                helper.StandardInput.Flush();
                game = process;
                game.Exited += GameExited;
                game.EnableRaisingEvents = true;
                if (game.HasExited) Dispose();
            }
            catch
            {
                Dispose();
                throw;
            }
        }
    }

    void GameExited(object? sender, EventArgs args) => Dispose();

    public void Dispose()
    {
        lock (gate)
        {
            if (Interlocked.Exchange(ref disposed, 1) != 0) return;
            if (game is not null) game.Exited -= GameExited;
            game = null;
            StopOwnedHelper(helper);
            Cleanup(readyFile);
        }
    }

    static async Task Drain(Stream stream)
    {
        try { await stream.CopyToAsync(Stream.Null).ConfigureAwait(false); }
        catch (Exception e) when (e is IOException or ObjectDisposedException or InvalidOperationException) { }
    }

    static void StopOwnedHelper(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: false);
                process.WaitForExit(2000);
            }
        }
        catch (Exception e) when (e is InvalidOperationException or Win32Exception or NotSupportedException) { }
        finally { process.Dispose(); }
    }

    static string ReadyPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !Path.IsPathFullyQualified(path))
            throw new IOException("The gallery helper needs an absolute launch-file path.");
        var full = Path.GetFullPath(path);
        if (!Regex.IsMatch(Path.GetFileName(full), @"(?:^|[-.])[0-9a-fA-F]{32}\.json$", RegexOptions.CultureInvariant))
            throw new IOException("The gallery helper launch file must have its own random identifier.");
        return full;
    }

    static void CheckOwnedPaths(string path)
    {
        foreach (var candidate in new[] { path, path + ".tmp" })
            for (var current = candidate; current is not null; current = Path.GetDirectoryName(current))
                if ((File.Exists(current) || Directory.Exists(current)) && (File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
                    throw new IOException("The gallery helper launch path cannot contain links.");
    }

    static void Cleanup(string path)
    {
        // ReadyPath was checked before starting. No wildcard or directory cleanup.
        foreach (var candidate in new[] { path, path + ".tmp" })
            try
            {
                CheckOwnedPaths(path);
                File.Delete(candidate);
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException) { }
    }

    internal static string ValidateReady(ReadOnlySpan<byte> bytes, int expectedPid)
    {
        using var document = JsonDocument.Parse(bytes.ToArray());
        var root = document.RootElement;
        if (expectedPid <= 0 || root.ValueKind != JsonValueKind.Object || !root.TryGetProperty("pid", out var pid) ||
            pid.ValueKind != JsonValueKind.Number || !pid.TryGetInt32(out var actualPid) || actualPid != expectedPid || !root.TryGetProperty("url", out var urlValue) ||
            urlValue.ValueKind != JsonValueKind.String)
            throw new IOException("The gallery helper launch response does not match its process.");
        var url = urlValue.GetString()!;
        if (!Regex.IsMatch(url, @"^http://127\.0\.0\.1:[0-9]{1,5}/?$", RegexOptions.CultureInvariant) ||
            !Uri.TryCreate(url, UriKind.Absolute, out var uri) || uri.Port is < 1 or > 65535 || uri.Host != "127.0.0.1" ||
            uri.Scheme != "http" || uri.UserInfo != "" || uri.Query != "" || uri.Fragment != "" || uri.AbsolutePath != "/")
            throw new IOException("The gallery helper must listen on a local loopback origin.");
        return uri.GetLeftPart(UriPartial.Authority);
    }

    internal static void SelfTest()
    {
        static byte[] Ready(string url, int pid = 1234) => JsonSerializer.SerializeToUtf8Bytes(new { pid, url });
        if (ValidateReady(Ready("http://127.0.0.1:45678/"), 1234) != "http://127.0.0.1:45678")
            throw new Exception("Gallery loopback origin normalization failed.");
        foreach (var invalid in new[] { "http://localhost:45678", "http://2130706433:45678", "http://127.0.0.1:0", "https://127.0.0.1:45678", "http://127.0.0.1:45678/path", "http://127.0.0.1:45678/?key=secret", "http://127.0.0.1:45678/#x", "http://name@127.0.0.1:45678", "http://127.0.0.1:65536" })
        {
            try { ValidateReady(Ready(invalid), 1234); }
            catch (IOException) { continue; }
            throw new Exception("Invalid gallery helper origin was accepted.");
        }
        try { ValidateReady(Ready("http://127.0.0.1:45678", 4321), 1234); throw new Exception("Foreign gallery helper process was accepted."); }
        catch (IOException) { }
        var portable = Path.Combine(Path.GetTempPath(), "Private Worlds with spaces", "gallery-ready-" + Guid.NewGuid().ToString("N") + ".json");
        if (ReadyPath(portable) != Path.GetFullPath(portable)) throw new Exception("Portable gallery launch path failed.");
        try { ReadyPath(Path.Combine(Path.GetTempPath(), "ready.json")); throw new Exception("Unowned gallery launch filename was accepted."); }
        catch (IOException) { }
    }
}
