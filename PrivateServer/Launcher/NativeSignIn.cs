namespace PlatinumPrivateLauncher;

static class NativeSignIn
{
    // Native builds use the working-directory file as well as app data. Never
    // import the installed game's identity: both copies get our private profile.
    internal static void Write(string client, byte[] sealedProfile, Func<bool> gameRunning)
    {
        if (sealedProfile.Length is < 20 or > 32768) throw new IOException("Invalid protected private profile.");
        var root = Path.GetFullPath(client);
        if (!File.Exists(Path.Combine(root, ".private-launcher"))) throw new IOException("Private client marker missing.");
        var paths = new[] { Path.Combine(root, "Game", "online-sign-in.dat"), Path.Combine(root, "AppData", "PlatinumTogether", "online-sign-in.dat") };
        foreach (var path in paths.SelectMany(path => new[] { path, path + ".previous" }))
        {
            if (File.Exists(path) && new FileInfo(path).Length > 32768) throw new IOException("Existing private sign-in is unexpectedly large.");
            for (var current = path; current is not null && current.StartsWith(root, StringComparison.OrdinalIgnoreCase); current = Path.GetDirectoryName(current))
                if ((File.Exists(current) || Directory.Exists(current)) && (File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
                    throw new IOException("Private sign-in paths cannot be links.");
        }
        var before = paths.Select(path => File.Exists(path) ? File.ReadAllBytes(path) : null).ToArray();
        var stages = paths.Select(path => path + ".launcher-" + Guid.NewGuid().ToString("N") + ".tmp").ToArray();
        var committed = 0;
        try
        {
            if (gameRunning()) throw new IOException("Close the private game before updating its sign-in.");
            for (var i = 0; i < paths.Length; i++)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(paths[i])!);
                File.WriteAllBytes(stages[i], sealedProfile);
            }
            if (gameRunning()) throw new IOException("The private game started while preparing sign-in. Close it and try again.");
            for (var i = 0; i < paths.Length; i++)
            {
                var current = File.Exists(paths[i]) ? File.ReadAllBytes(paths[i]) : null;
                if ((current is null) != (before[i] is null) || current is not null && !current.SequenceEqual(before[i]!))
                    throw new IOException("Private sign-in changed during launch; existing credentials retained.");
            }
            for (var i = 0; i < paths.Length; i++)
            {
                if (before[i] is not null && !before[i]!.SequenceEqual(sealedProfile)) File.WriteAllBytes(paths[i] + ".previous", before[i]!);
                File.Move(stages[i], paths[i], true);
                committed++;
            }
        }
        catch
        {
            for (var i = committed - 1; i >= 0; i--)
                if (before[i] is { } previous) File.WriteAllBytes(paths[i], previous); else File.Delete(paths[i]);
            throw;
        }
        finally { foreach (var stage in stages) if (File.Exists(stage)) File.Delete(stage); }
    }

    internal static void SelfTest()
    {
        var root = Path.Combine(Path.GetTempPath(), "private-signin-test-" + Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(root);
            var first = Enumerable.Repeat((byte)1, 32).ToArray();
            var next = Enumerable.Repeat((byte)2, 32).ToArray();
            try { Write(root, first, () => false); throw new Exception("Unmarked client accepted."); } catch (IOException) { }
            File.WriteAllText(Path.Combine(root, ".private-launcher"), "2\n");
            Write(root, first, () => false);
            var paths = new[] { Path.Combine(root, "Game", "online-sign-in.dat"), Path.Combine(root, "AppData", "PlatinumTogether", "online-sign-in.dat") };
            if (paths.Any(path => !File.ReadAllBytes(path).SequenceEqual(first))) throw new Exception("Private native sign-in copies disagree.");
            try { Write(root, next, () => true); throw new Exception("Running game accepted."); } catch (IOException) { }
            var calls = 0;
            try { Write(root, next, () => ++calls == 2); throw new Exception("Concurrent game launch accepted."); } catch (IOException) { }
            if (paths.Any(path => !File.ReadAllBytes(path).SequenceEqual(first))) throw new Exception("Running game sign-in changed.");
            Write(root, next, () => false);
            if (paths.Any(path => !File.ReadAllBytes(path).SequenceEqual(next) || !File.ReadAllBytes(path + ".previous").SequenceEqual(first))) throw new Exception("Private sign-in update/backup failed.");
            if (Directory.EnumerateFiles(root, "*.tmp", SearchOption.AllDirectories).Any()) throw new Exception("Sign-in staging files leaked.");
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
    }
}
