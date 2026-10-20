using System.Text.Json;
using Microsoft.Win32;

namespace PlatinumPrivateLauncher;
sealed partial class Launcher
{
    DiscordParty? discordParty;
    readonly System.Windows.Forms.Timer invitePoll = new() { Interval = 20000 };
    bool invitePolling, inviteJoining;
    string? queuedDiscordInvite;

    void SetupDiscordInvites(FlowLayoutPanel card)
    {
        Add(card, "Enable Discord invite launch", () => {
            using var protocol = Registry.CurrentUser.CreateSubKey(@"Software\Classes\discord-" + DiscordPresence.ApplicationId);
            protocol.SetValue("", "URL:Plat Together private game invite");protocol.SetValue("URL Protocol", "");
            using var command = protocol.CreateSubKey(@"shell\open\command");
            command.SetValue("", "\"" + Environment.ProcessPath + "\" --discord-launch \"%1\"");
            Log("Discord can open this portable launcher. Enable this again if you move its folder.");return Task.CompletedTask;
        });
        invitePoll.Tick += async (_, _) => await RefreshDiscordParty();
        Shown += (_, _) => invitePoll.Start();
        FormClosed += (_, _) => { invitePoll.Stop();invitePoll.Dispose(); };
    }

    async Task RefreshDiscordParty()
    {
        if (invitePolling || busy || inviteJoining || !discordEnabled.Checked) return;
        if (queuedDiscordInvite is string queued) { queuedDiscordInvite = null;ReceiveDiscordInvite(queued);return; }
        if (!GameRunning() || !File.Exists(Profile)) { Volatile.Write(ref discordParty, null);return; }
        invitePolling = true;var selectedProfile=Profile;var output = Path.Combine(root, "discord-party-" + Guid.NewGuid().ToString("N") + ".json");
        try {
            await Command("discord-invite", "--profile", selectedProfile, "--out", output);
            using var data = JsonDocument.Parse(await File.ReadAllTextAsync(output));var v = data.RootElement.GetProperty("invite");
            DiscordParty? party = null;
            if (v.ValueKind == JsonValueKind.Object && v.GetProperty("expires").GetInt64() > DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() && DiscordJoinAddress.Valid(v.GetProperty("secret").GetString()))
                party = new(v.GetProperty("party").GetString()!, v.GetProperty("size").GetInt32(), v.GetProperty("max").GetInt32(), v.GetProperty("secret").GetString()!);
            Volatile.Write(ref discordParty, selectedProfile == Profile && GameRunning() ? party : null);
        } catch { Volatile.Write(ref discordParty, null); }
        finally { if (File.Exists(output)) File.Delete(output);invitePolling = false; }
    }

    internal void ReceiveDiscordInvite(string secret)
    {
        if (!DiscordJoinAddress.Valid(secret) || IsDisposed || !IsHandleCreated) return;
        try { BeginInvoke(async () => {
            if (inviteJoining) return;
            if (busy || invitePolling) { queuedDiscordInvite = secret;return; }
            inviteJoining = true;
            busy=true;SetInviteControls(false);
            try {
                var origin = new Uri(secret).GetLeftPart(UriPartial.Authority);
                if (GameRunning() && !string.Equals(Url, origin, StringComparison.OrdinalIgnoreCase))
                    throw new IOException("Save and close your private game before joining an invite to another server. Click the Discord invite again afterward.");
                // An external invite is untrusted input. Show its destination before contacting it or using an identity.
                if (MessageBox.Show(this, "Join the private party on " + origin + "?\nYour own name and account will be used on this server.", "Discord game invite", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
                using var http = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }) { Timeout = TimeSpan.FromSeconds(8), MaxResponseContentBufferSize = 4096 };
                using var response = await http.GetAsync(secret, HttpCompletionOption.ResponseHeadersRead);response.EnsureSuccessStatusCode();
                await using var stream = await response.Content.ReadAsStreamAsync();var bytes = new byte[4097];int count = 0, read;
                while (count < bytes.Length && (read = await stream.ReadAsync(bytes.AsMemory(count))) > 0) count += read;
                if (count == bytes.Length) throw new IOException("Invalid Discord invite response.");
                using var data = JsonDocument.Parse(bytes.AsMemory(0, count));var v = data.RootElement;
                var code = v.GetProperty("code").GetString();
                if (v.GetProperty("server").GetString()?.TrimEnd('/') != origin || v.GetProperty("expires").GetInt64() <= DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() ||
                    code is null || !System.Text.RegularExpressions.Regex.IsMatch(code, "^[A-HJKMNP-Z2-9]{6}$")) throw new IOException("This Discord invite expired or is invalid.");
                using var info = JsonDocument.Parse(await http.GetStringAsync(origin + "/private/server-info"));
                if (info.RootElement.GetProperty("hub_key").GetString() != v.GetProperty("hub_key").GetString()) throw new IOException("The invite server identity changed.");
                remoteUrl.Text = origin;remote.Checked = true;
                if (!File.Exists(Profile) && string.IsNullOrWhiteSpace(name.Text)) { showLauncherPage?.Invoke("Name");throw new IOException("Pick your name first, then click the Discord invite again."); }
                await StartServer();await Command("party-join", "--profile", Profile, "--code", code!);
                RememberServer(origin);RefreshAccount();showLauncherPage?.Invoke("Game");Activate();
                Log("Discord party joined. Native automatic world entry is still under development; use the game's party controls.");
            } catch (Exception e) { Log(e.Message);MessageBox.Show(this, e.Message, "Discord game invite", MessageBoxButtons.OK, MessageBoxIcon.Information); }
            finally { inviteJoining = false;busy=false;SetInviteControls(true); }
        }); } catch(InvalidOperationException) { }
    }
    void SetInviteControls(bool enabled)
    {
        if(IsDisposed)return;
        foreach(var control in actions.Cast<Control>().Concat(new Control[]{source,rom,name,port,remote,remoteUrl,lan,advertised,savedServerList}))control.Enabled=enabled;
        if(enabled){remoteUrl.Enabled=remote.Checked;advertised.Enabled=lan.Checked;}
    }
}
