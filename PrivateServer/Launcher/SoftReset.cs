using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PlatinumPrivateLauncher;

static class ResetKeys
{
    internal static ushort[] FromBindings(ControlBindings bindings)
    {
        var keys = new[] { "LKey", "RKey", "StartKey", "SelectKey" }.Select(name => WindowsKey(bindings.Values[name])).ToArray();
        if (keys.Distinct().Count() != 4) throw new IOException("Soft reset needs four distinct keyboard bindings for L, R, Start and Select.");
        return keys;
    }
    static ushort WindowsKey(int sdl)
    {
        // Prefer the side-specific modifier keys over their Windows aliases.
        if (sdl is >= 1073742048 and <= 1073742054)
            return sdl switch { 1073742048 => 0xa2, 1073742049 => 0xa0, 1073742050 => 0xa4, 1073742052 => 0xa3, 1073742053 => 0xa1, 1073742054 => 0xa5, _ => throw new IOException("Unsupported reset modifier.") };
        foreach (var key in Enum.GetValues<Keys>())
        {
            if ((int)key <= 0 || (int)key > 255) continue;
            try { if (ControlBindings.FromWindowsKey(key) == sdl) return (ushort)key; } catch (IOException) { }
        }
        throw new IOException("A reset binding has no supported Windows keyboard key. Rebind L, R, Start or Select in Controls.");
    }
    internal static void SelfTest()
    {
        var bindings = new ControlBindings([]);
        if (!FromBindings(bindings).SequenceEqual(new ushort[] { 0x51, 0x45, 0x0d, 0xa1 })) throw new Exception("Default reset chord mismatch.");
        bindings.Values["LKey"] = 'k';
        if (FromBindings(bindings)[0] != 0x4b) throw new Exception("Remapped reset key ignored.");
        bindings.Values["RKey"] = 'k';
        try { FromBindings(bindings); } catch (IOException) { return; }
        throw new Exception("Duplicate reset chord accepted.");
    }
}

sealed partial class Launcher
{
    async Task SoftResetGame()
    {
        var path = Path.Combine(GameDir, "Platinum.exe");
        await GameSafety.CheckFile(path);
        using var target = FindResetTarget(path);
        var config = Path.Combine(GameDir, "sim_config.ini");
        var keys = ResetKeys.FromBindings(new ControlBindings(File.Exists(config) ? await File.ReadAllBytesAsync(config) : []));
        if (keys.Any(key => (ResetInput.GetAsyncKeyState(key) & 0x8000) != 0)) throw new IOException("Release L, R, Start and Select before using soft reset.");
        target.Refresh();
        var window = target.MainWindowHandle;
        if (window == IntPtr.Zero) throw new IOException("The private game's main window is unavailable.");
        ResetInput.SetForegroundWindow(window);
        await Task.Delay(100);
        if (target.HasExited || ResetInput.GetForegroundWindow() != window) throw new IOException("Bring the private game to the foreground, then try soft reset again.");
        try
        {
            ResetInput.Send(keys, false);
            for (int i = 0; i < 5; i++)
            {
                await Task.Delay(50);
                if (target.HasExited || ResetInput.GetForegroundWindow() != window) break;
            }
        }
        finally { ResetInput.Send(keys.Reverse().ToArray(), true); }
        Log("Sent native L + R + Start + Select. Check the game for the reset; it may refuse during saving or communication. No save files were written.");
    }
    static Process FindResetTarget(string path)
    {
        Process? target = null;
        foreach (var candidate in Process.GetProcessesByName("Platinum"))
        {
            bool matches;
            try { matches = string.Equals(candidate.MainModule?.FileName, path, StringComparison.OrdinalIgnoreCase); } catch { matches = false; }
            if (!matches) { candidate.Dispose(); continue; }
            if (target != null) { candidate.Dispose(); target.Dispose(); throw new IOException("More than one private game is running. Close the extra instance before resetting."); }
            target = candidate;
        }
        return target ?? throw new IOException("Start this launcher's private game before using soft reset.");
    }
}

static class ResetInput
{
    [StructLayout(LayoutKind.Sequential)] struct Keyboard { internal ushort Key, Scan; internal uint Flags, Time; internal IntPtr Extra; }
    [StructLayout(LayoutKind.Explicit, Size = 40)] struct Input { [FieldOffset(0)] internal uint Type; [FieldOffset(8)] internal Keyboard Keyboard; }
    [DllImport("user32.dll")] internal static extern bool SetForegroundWindow(IntPtr window);
    [DllImport("user32.dll")] internal static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] internal static extern short GetAsyncKeyState(int key);
    [DllImport("user32.dll", SetLastError = true)] static extern uint SendInput(uint count, Input[] inputs, int size);
    internal static void Send(ushort[] keys, bool release)
    {
        var inputs = keys.Select(key => new Input { Type = 1, Keyboard = new Keyboard { Key = key, Flags = release ? 2u : 0u } }).ToArray();
        var sent = SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<Input>());
        if (!release && sent != inputs.Length) throw new IOException("Windows did not accept the complete reset key sequence.");
    }
}
