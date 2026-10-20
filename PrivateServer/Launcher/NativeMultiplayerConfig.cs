using System.Text;

namespace PlatinumPrivateLauncher;

static class NativeMultiplayerConfig
{
    internal static byte[] Update(byte[] original, string origin, string hubKey)
    {
        if (original.Length > 1048576) throw new IOException("The native multiplayer configuration is unexpectedly large.");
        if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https") || uri.UserInfo != "" || uri.AbsolutePath != "/" || uri.Query != "" || uri.Fragment != "")
            throw new IOException("Invalid private server origin.");
        byte[] key;
        try { key = Convert.FromBase64String(hubKey); } catch (FormatException) { throw new IOException("Invalid private server key."); }
        if (key.Length != 32) throw new IOException("Invalid private server key.");
        var text = new UTF8Encoding(false, true).GetString(original).TrimStart('\uFEFF');
        var newline = text.Contains("\r\n") ? "\r\n" : "\n";
        var lines = text.Replace("\r\n", "\n").Split('\n');
        var result = new List<string>();
        bool wroteUrl = false, wroteKey = false, hasLast = false;
        foreach (var line in lines)
        {
            var token = line.TrimStart().Split([' ', '\t'], 2)[0];
            if (token == "hub_url") { if (!wroteUrl) result.Add("hub_url " + uri.GetLeftPart(UriPartial.Authority)); wroteUrl = true; }
            else if (token == "hub_key") { if (!wroteKey) result.Add("hub_key " + Convert.ToBase64String(key)); wroteKey = true; }
            else { result.Add(line); if (token == "last") hasLast = true; }
        }
        if (!wroteUrl) result.Add("hub_url " + uri.GetLeftPart(UriPartial.Authority));
        if (!wroteKey) result.Add("hub_key " + Convert.ToBase64String(key));
        // New profiles start solo. Existing native choices are opaque and retained.
        if (!hasLast) result.Add("last solo");
        return Encoding.UTF8.GetBytes(string.Join(newline, result));
    }

    internal static void SelfTest()
    {
        var original = Encoding.UTF8.GetBytes("# native settings\r\nhub_url https://old.example\r\nhub_key old\r\nlast chosen-native-mode 123\r\nserver favorite.example opaque-id\r\nunknown keep this\r\n");
        var before = original.ToArray();
        var key = Convert.ToBase64String(new byte[32]);
        var output = Update(original, "https://new.example", key);
        var text = Encoding.UTF8.GetString(output);
        if (!original.SequenceEqual(before) || !text.Contains("last chosen-native-mode 123\r\n") || !text.Contains("server favorite.example opaque-id\r\n") || !text.Contains("unknown keep this\r\n") || text.Contains("last solo") || text.Contains("old.example"))
            throw new Exception("Native mode/bookmark preservation failed.");
        if (!Update(output, "https://new.example", key).SequenceEqual(output)) throw new Exception("Native config update is not idempotent.");
        if (!Encoding.UTF8.GetString(Update([], "http://127.0.0.1:8788", key)).Contains("last solo")) throw new Exception("Fresh native mode missing.");
        try { Update(original, "https://new.example/\nlast solo", key); } catch (IOException) { return; }
        throw new Exception("Invalid native server origin accepted.");
    }
}
