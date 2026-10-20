# Platinum Together private launcher — development preview

Run `Portable/Launcher/PlatinumPrivateLauncher.exe`. Keep the complete Portable folder together. Launcher and Map Studio include .NET; Runtime contains Node.js.

## Setup and Play

In **Game setup**, choose the installed verified 0.1.5 or 0.1.6 game folder. Optionally select or drag in your own Pokémon Platinum `.nds` ROM. **Extract & prepare private game** copies validated packaged assets, extracts the ROM's FNT/FAT filesystem if selected, and validates required ROM assets against the installed manifest. Without a ROM selection it copies already extracted, validated ROM data.

The source ROM and original installation are read only. The separate copy lives in `Portable/Client/Game`, with its own saves. Its local console firmware/profile is copied once because this native build requires it for a fresh start. Repairs preserve that private profile. Original saves and online account credentials are not imported. The game executable is unchanged and version pinned.

**Play private game** starts/reuses the selected server, signs in with the saved private identity, protects game sign-in using Windows DPAPI and starts the separate game. **Launch installed game** opens the original game normally.

## Accounts and unique usernames

In **Account**, choose a username and click **Create account / sign in**. Names use 3–16 letters, numbers or underscores, starting with a letter. Each private server reserves usernames case-insensitively: `PlayerName` and `playername` cannot belong to different accounts. Sign-in proves possession of the saved Ed25519 key; entering an existing name alone cannot claim it. Registration can recover a lost server response using the locally saved pending key.

**Change my username** keeps the same player ID, key, friends and world memberships while reserving the new name. The old name becomes available. The native trainer display is limited to 10 characters; account handles remain unique up to 16 characters. Close the private game before switching or renaming accounts. Existing local `Trainer` accounts can be renamed from this page.

**Export my account backup** saves the private sign-in key. **Restore an account backup** shows its server address for confirmation, verifies sign-in, and backs up any existing profile before replacing it. Keep these files private. Names are unique within the selected private server, not a reservation on the official service or every independent host.

To join another private server, enable **Connect to another private server**, enter its URL, and create/sign in to your account there. This does not start a local host. The launcher remembers local/remote settings and keeps remote profiles separate from its local server-owner identity.

## Actual support

- Fresh game reaches the native welcome screen (live verified).
- Private lobby sign-in worked in an earlier isolated client. Full friends/invite UI flows are not yet live verified.
- Native parser/codec checks accept social snapshots, friend requests, party invites and city movement.
- Eleven real HTTP/WebSocket tests cover authentication, account name uniqueness/key ownership/rename/recovery, friends, city movement, invitations, parties, world save integrity/restart recovery, refresh, chat and graceful owner-only shutdown.
- World records/uploaded saves persist in SQLite. The backend stays running after closing the launcher. **It does not yet simulate a world without a game client.**
- Adventures/Rifts, player-world session allocation, guilds, trading and public PvP are not implemented in the private backend. Enabling their flags alone will not fix them. The mode page reports current support.
- Map Studio supports terrain, collision, objects, dialogue, spawn and warps, with validated versioned `.ptpack` exports. **Native game map loading is not connected.** Inverna rendering investigation remains in progress.

The host binds to 127.0.0.1 by default. **World hosting** can enable LAN connections and advertise a chosen LAN/HTTPS server address. Stop and restart the local server after changing its binding or address. No firewall, startup, router or official-service settings are changed automatically. LAN/public native gameplay is not certified.

## Private data and sharing

`Portable/Data` and `Portable/Profiles` contain sensitive private keys, player credentials, database and logs. Do not share them. `Portable/Client` contains locally copied game assets and saves; do not redistribute it. DPAPI game sign-in belongs to your Windows account. The raw private profile is still sensitive.

A clean package contains only Launcher, MailSender, MapStudio, Runtime, Server, ContentPacks and documentation. No copied game assets, ROMs, research, credentials or test saves belong in a release.

## Checks

From PrivateServer, run `Portable/Runtime/node.exe --test Server/test/backend.test.mjs`. Each GUI executable accepts `--selftest`, writing compact result JSON alongside itself. ROM tests use a synthetic filesystem; selected actual-ROM extraction has not been live tested, while copying validated installed ROM data has.

Target executable SHA-256: `27075fceb04b5a3140e707bae1b12d69e04824b86e960121f566ad5418728488` (0.1.5). New versions require an adapter audit.


## Mail, Pokémon, eggs and custom items

Run `Portable/MailSender/PlatinumMailSender.exe` on the owner PC. Its RSA sender key is Windows DPAPI protected under `Portable/OwnerKeys`; other PCs do not get that key. The backend trusts only its public key and accepts publication from loopback with a fresh signed challenge. Regular account keys cannot publish gifts.

Native MAIL/SUPPLIES handles the supported named items, money, boots, lures and generated eggs. Import an exact `.pk4` Pokémon/egg, or choose an item from the full installed item catalog, to use the native Poké Mart delivery queue. The recipient selects **Gift inbox** in the launcher, closes their private game, selects a gift and chooses their private world's save. The original save is backed up. Then collect it from the Poké Mart deliveryman. The queue has eight slots; existing gifts are preserved. Claim journals and server account/save binding prevent duplicate retries. A failed acknowledgement is recovered before the launcher allows Play.

Game 0.1.6 native save-size, CRC, item gift getter, social/mail/chat/relay parser and city codec fixtures passed. This is format validation; end-to-end native multiplayer and gift collection are still pending live verification.

## Internet relay

World hosting → Start internet relay opens an outbound Cloudflare Quick Tunnel. Share its HTTPS URL; router port forwarding is unnecessary. The hostname changes when a new tunnel is created. The host PC and backend must remain running. This is temporary relay hosting; unattended native world simulation is still unfinished.

An isolated internet test passed HTTPS health, server identity pin verification and authenticated WebSocket sign-in through the tunnel. Its test server/tunnel were then stopped. Local backend and source tests cover actual authenticated binary routing and stable peer seats.

## Report privacy

This private launcher has no automatic diagnostic-report uploader. The official launcher's separate local privacy patch was rebuilt, installed and checked for launcher 0.1.16. Its fake queued report made zero signing/HTTP calls and remained on disk. Future official launcher updates can replace the patch; `../LauncherPrivacy/Check.ps1` checks the currently running official launcher. Ordinary online gameplay and server traffic remain enabled.


## Launcher home, names and controls

The launcher now follows the installed launcher's layout: compact sidebar, scene background, wordmark, name card and large Play button. Background scenes/wordmark are loaded from the local official launcher Assets folder; a plain fallback works without it. The app icon is embedded in the EXE and used by the window.

Use **Name** or the home name field, then **Use this name**. There is no password/email screen. A unique private identity is still kept behind the name, and name backups remain available. Names remain unique per server.

**Controls** reads the private game's `sim_config.ini` PAD settings. Select an action and **Change selected key**, or double-click its keyboard cell. Controller button and D-pad choices use native SDL indices. **Save bindings** makes an original-settings backup, refuses a running private game or a stale settings file, and changes only PAD binding values. Other sections, hardware address, console profile values and joystick deadzone are preserved. Changes apply on the next private game launch. The original installed game's bindings are not modified.

The published launcher passes UI construction, ROM safety, key conversion and INI preservation checks. A live key-capture dialog captured a key; the actual game configuration hash remained unchanged during that UI check. In-game behavior of every remapped action still needs a private-client test.
