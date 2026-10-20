using System.Security.Cryptography;

namespace PlatinumPrivateLauncher;

// This is a local incident block, not a malware scanner or safety certificate.
static class GameSafety
{
    internal const string QuarantinedHash = "5520b7d176a3598b67f3c126359deada0f4099a6f961143453d7d790d69ab6c2";
    internal static bool Blocked(string hash) => string.Equals(hash, QuarantinedHash, StringComparison.OrdinalIgnoreCase);
    internal static void CheckHash(string hash)
    {
        if (Blocked(hash)) throw new IOException("This game executable matches the lab build quarantined by Windows Security. Preparation and launch are disabled for this build pending review. Keep it quarantined; do not add an antivirus exclusion.");
    }
    internal static async Task CheckFile(string path)
    {
        await using var file = File.OpenRead(path);
        var hash = Convert.ToHexString(await SHA256.HashDataAsync(file));
        CheckHash(hash);
        if (!LauncherChecks.Supported(hash)) throw new IOException("This executable is not a verified supported native game build.");
    }
}
