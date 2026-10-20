using System.Diagnostics;

namespace PlatinumPrivateLauncher;

// PC builds do not reliably honor the original DS key chord. A normal window
// close and fresh process return to title without injecting keys or editing saves.
static class NativeGameReset
{
    internal static ProcessStartInfo LaunchInfo(string gameDirectory)
    {
        var directory = Path.GetFullPath(gameDirectory);
        var client = Directory.GetParent(directory)?.FullName ?? throw new IOException("Private game folder missing.");
        var info = new ProcessStartInfo(Path.Combine(directory, "Platinum.exe")) { WorkingDirectory = directory, UseShellExecute = false };
        info.Environment["LOCALAPPDATA"] = Path.Combine(client, "AppData");
        return info;
    }

    internal static async Task<Process> Restart(string gameDirectory, Func<ProcessStartInfo, Task<Process>>? launch = null)
    {
        var info = LaunchInfo(gameDirectory);
        var client = Directory.GetParent(info.WorkingDirectory)!.FullName;
        var marker = Path.Combine(client, ".private-launcher");
        if (!File.Exists(marker) || !File.ReadAllText(marker).StartsWith("2\n")) throw new IOException("Prepare this launcher's private game before resetting.");
        await GameSafety.CheckFile(info.FileName);
        using var target = FindTarget(info.FileName) ?? throw new IOException("The private game is no longer running.");
        if (!target.CloseMainWindow()) throw new IOException("The private game did not accept a normal close request. Close it normally before starting again.");
        // The native PC client asks before discarding unsaved progress. Leave
        // that decision in its own UI; never automate Yes or terminate it.
        try { await target.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(60)); }
        catch (TimeoutException) { throw new IOException("Reset cancelled: the private game did not close within 60 seconds. It was left running. Select Yes in its close prompt, or No to keep playing; use Play after a later close."); }
        await GameSafety.CheckFile(info.FileName);
        using var other = FindTarget(info.FileName, required: false);
        if (other is not null) throw new IOException("Another private game window has opened. It was left running; no duplicate was started.");
        return launch is not null ? await launch(info) : Process.Start(info) ?? throw new IOException("The private game closed but could not restart. Use Play to start it again.");
    }

    static Process? FindTarget(string path, bool required = true)
    {
        Process? target = null;
        foreach (var candidate in Process.GetProcessesByName("Platinum"))
        {
            bool matches;
            try { matches = string.Equals(candidate.MainModule?.FileName, path, StringComparison.OrdinalIgnoreCase); } catch { matches = false; }
            if (!matches) { candidate.Dispose(); continue; }
            if (target is not null) { candidate.Dispose(); target.Dispose(); throw new IOException("More than one private game is running. Close the extra instance before resetting."); }
            target = candidate;
        }
        return target ?? (required ? throw new IOException("Start this launcher's private game before using reset.") : null);
    }

    internal static void SelfTest()
    {
        var directory = Path.Combine(Path.GetTempPath(), "private-reset-fixture", "Client", "Game");
        var info = LaunchInfo(directory);
        if (info.FileName != Path.Combine(directory, "Platinum.exe") || info.WorkingDirectory != directory || info.UseShellExecute || info.Environment["LOCALAPPDATA"] != Path.Combine(Path.GetDirectoryName(directory)!, "AppData") || info.ArgumentList.Count != 0)
            throw new Exception("Reset launch isolation failed.");
    }
}

sealed partial class Launcher
{
    async Task SoftResetGame()
    {
        Log("Reset requested. In the private game, confirm Yes on 'Close the game?' to discard unsaved progress. The launcher waits up to 60 seconds, then reopens at title. No keeps the game open.");
        var restarted = await NativeGameReset.Restart(GameDir, LaunchWithGallery);
        game?.Dispose();
        game = restarted;
        restarted.EnableRaisingEvents = true;
        restarted.Exited += (_, _) => { if (ReferenceEquals(game, restarted)) Volatile.Write(ref discordState, "In the launcher"); };
        Volatile.Write(ref discordState, "Playing a private world");
        Log("Private game restarted at title. Continue your last save to hunt the encounter again. Reset did not write or restore save files.");
    }
}
