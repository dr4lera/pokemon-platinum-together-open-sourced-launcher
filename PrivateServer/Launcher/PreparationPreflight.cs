using System.Security.Cryptography;

namespace PlatinumPrivateLauncher;

static class PreparationPreflight
{
    // Validate the complete input set before replacing even the first asset.
    // Copy-time validation remains necessary if an official updater runs later.
    internal static void Validate(string source,string destination,(string Name,string Hash)[] files)
    {
        foreach(var (relative,expected) in files)
        {
            var from=LauncherChecks.Child(source,relative);
            var to=LauncherChecks.Child(destination,relative);
            RejectLinks(from);RejectLinks(to);
            if(!File.Exists(from))throw new IOException("Missing installed asset: "+relative);
            using var input=File.OpenRead(from);
            if(!Convert.ToHexString(SHA256.HashData(input)).Equals(expected,StringComparison.OrdinalIgnoreCase))
                throw new IOException("Installed asset failed validation: "+relative+". No game assets were replaced.");
        }
    }

    static void RejectLinks(string path)
    {
        for(var current=Path.GetFullPath(path);current!=null;current=Path.GetDirectoryName(current))
            if((File.Exists(current)||Directory.Exists(current))&&(File.GetAttributes(current)&FileAttributes.ReparsePoint)!=0)
                throw new IOException("Choose real installation and private-game folders, outside directory links.");
    }
}
