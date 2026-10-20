namespace PlatinumPrivateLauncher;
sealed partial class Launcher
{
    readonly CheckBox discordEnabled = new() { Text = "Show activity on Discord", AutoSize = true, ForeColor = Color.White, Checked = true };
    readonly Label discordStatus = new() { AutoSize = true, MaximumSize = new(750, 0), Text = "Discord: waiting", ForeColor = Color.LightSteelBlue };
    DiscordPresence? discord;
    readonly SemaphoreSlim discordChange = new(1, 1);
    bool discordReady;
    string discordState = "In the launcher";
    void DiscordOptions(FlowLayoutPanel card)
    {
        card.Controls.Add(discordEnabled); card.Controls.Add(discordStatus);
        SetupDiscordInvites(card);
        discordEnabled.CheckedChanged += async (_, _) => await UpdateDiscord();
        Shown += async (_, _) => { discordReady = true; await UpdateDiscord(); };
        FormClosed += async (_, _) => { if (discord is not null) { await discord.DisposeAsync(); discord = null; } };
    }
    async Task UpdateDiscord()
    {
        if (!discordReady) return;
        await discordChange.WaitAsync();
        try {
        if (discord is not null) { var previous = discord; discord = null; await previous.DisposeAsync(); }
        if (IsDisposed) return;
        if (!discordEnabled.Checked) { discordStatus.Text = "Discord: off"; return; }
        discord = new DiscordPresence(() => Volatile.Read(ref discordState), message =>
        {
            if (!IsDisposed && IsHandleCreated)
                try { BeginInvoke(() => { if (!IsDisposed) discordStatus.Text = message; }); } catch (InvalidOperationException) { }
        }, party: () => Volatile.Read(ref discordParty), join: ReceiveDiscordInvite);
        } finally { discordChange.Release(); }
    }
}
