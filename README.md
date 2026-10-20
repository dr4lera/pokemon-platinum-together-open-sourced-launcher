# Pokemon Platinum Together Open Sourced - Launcher

Community private-world tooling for an existing Platinum Together installation.
This is a development source release, not the original game source. Bring your
own supported game and ROM. No game assets, ROMs, saves or account keys are included.

Multiplayer support targets matching project-prepared clients. Joining a project
host from a stock official client is unsupported in this preview; a user reported
**World not found**. Official game-file compatibility does not establish
official-service interoperability. See [connection compatibility](PrivateServer/docs/players.md#connection-compatibility).

Native gameplay, friend battles and trading work with matching project-prepared
0.1.7 clients on the combined private backend, confirmed by the user on
2026-10-20. Dev12 also fixes Inverna movement packets, trainer names/appearance
updates and world allocation while the host is still in the city. Optional
community friend codes are available. See [verification status](PrivateServer/docs/status.md).

## Independent gameplay client

The current launcher still runs a separately prepared game supplied by the player.
The [source-client migration](https://github.com/dr4lera/pokemon-platinum-together-open-sourced-client)
is a separate public project. The [2026-10-20 source checkpoint](https://github.com/dr4lera/pokemon-platinum-together-open-sourced-client/blob/checkpoint-2026-10-20/docs/CHECKPOINT_2026_10_20.md)
builds on Windows and includes cap/OT/ball rules, native accounts and transport,
visible Union Room nametags, bounded modern data/form adapters, persistent modern
caught flags and trainer-art decoding/downloads. Two-game battle/trade completion,
custom avatar rendering, shared-world behavior and much of the0.1.10
modernization remain unfinished. See [the distinction and inspected native
rules](PrivateServer/docs/native-gameplay-rules.md).
Gameplay-client releases remain paused during this migration; server tools have
their own dev12 previews below.

## Downloads

**[Download the full Windows bundle â€” dev11 preview](https://github.com/dr4lera/pokemon-platinum-together-open-sourced-launcher/releases/download/v0.1.0-dev.11/PokemonPlatinumTogetherOpenSourced-Windows-x64-dev11.zip)**

Extract the complete ZIP and open `Launcher/PlatinumPrivateLauncher.exe`. Keep
the folder together. MailSender, Map Studio, the backend and Windows runtimes
are included. Read the [quick start](PrivateServer/docs/quick-start.md) first.

| Component | Download page | Setup |
|---|---|---|
| Launcher and full Windows bundle | [dev11 release and checksum](https://github.com/dr4lera/pokemon-platinum-together-open-sourced-launcher/releases/tag/v0.1.0-dev.11) | [Install and play](PrivateServer/docs/quick-start.md) |
| Shared server / relay | [dev12 server release](https://github.com/dr4lera/pokemon-platinum-together-open-sourced-server/releases/tag/v0.1.0-dev.12) | [Hosting guide](PrivateServer/docs/server-hosting.md) |
| Standalone Windows editors | [Editors release](https://github.com/dr4lera/pokemon-platinum-together-open-sourced-editors/releases/tag/v0.1.0-dev.11) | [Map and sprite tools](https://github.com/dr4lera/pokemon-platinum-together-open-sourced-editors#download) |
| Separate Inverna hosting | [dev12 Inverna release](https://github.com/dr4lera/pokemon-platinum-together-open-sourced-inverna-server/releases/tag/v0.1.0-dev.12) | [Inverna setup](https://github.com/dr4lera/pokemon-platinum-together-open-sourced-inverna-server#first-run) |
| Separate world backend hosting | [dev12 world release](https://github.com/dr4lera/pokemon-platinum-together-open-sourced-world-server/releases/tag/v0.1.0-dev.12) | [World setup](https://github.com/dr4lera/pokemon-platinum-together-open-sourced-world-server#first-run) |

Server downloads are dev12 with SHA-256 checksums. The Windows launcher/editor
downloads remain dev11 while the independent gameplay client is developed.
Native gameplay, battling and trading are user-confirmed. Dedicated unattended
world simulation, public matchmaking and custom map/story execution remain unfinished.

## Project repositories

- [Launcher, mail app and full documentation](https://github.com/dr4lera/pokemon-platinum-together-open-sourced-launcher)
- [Server and relay backend](https://github.com/dr4lera/pokemon-platinum-together-open-sourced-server)
- [Map Studio and trainer sprite tools](https://github.com/dr4lera/pokemon-platinum-together-open-sourced-editors)

## Start here

- [Windows portable development preview](https://github.com/dr4lera/pokemon-platinum-together-open-sourced-launcher/releases/tag/v0.1.0-dev.11)
- [Official 0.1.7 compatibility and starter checks](PrivateServer/docs/native-017.md)
- [Full documentation](PrivateServer/docs/README.md)
- [Install and play](PrivateServer/docs/quick-start.md)
- [Host a server, LAN and Internet relay](PrivateServer/docs/server-hosting.md)
- [Mail, PokÃ©mon, eggs and gifts](PrivateServer/docs/mail-and-gifts.md)
- [Map packs](PrivateServer/docs/maps.md)
- [Custom trainer sprites](PrivateServer/docs/trainer-sprites.md)
- [Backups and troubleshooting](PrivateServer/docs/maintenance.md)
- [Verification status and unfinished features](PrivateServer/docs/status.md)

## Build

Clone recursively so the server and editor match the pinned revisions:

```powershell
git clone --recurse-submodules https://github.com/dr4lera/pokemon-platinum-together-open-sourced-launcher.git
cd pokemon-platinum-together-open-sourced-launcher
dotnet build PrivateServer/Launcher/Launcher.csproj -c Release
dotnet build PrivateServer/MailSender/MailSender.csproj -c Release
dotnet run --project PrivateServer/Checks/GiftChecks.csproj -c Release
node --test PrivateServer/Server/test/backend.test.mjs
.\PrivateServer\build.ps1 -RuntimeDirectory C:\path\to\verified-runtime
```

The runtime directory must contain Node.js and cloudflared with their notices.
The build script creates a new portable folder and never replaces a running
installation or mutable player data. See [development](PrivateServer/docs/development.md).

## Current limits

Dev10 adds preparation and native launch for the verified official 0.1.7 build,
carrying its current native code and matching assets. Update your locally
supplied game with the official launcher, then prepare the private copy again.
The 0.1.7 gift save adapter is enabled after a separate native-format audit and
copied-save checks. Native collection remains unverified; the legacy injected
trainer remains disabled for 0.1.7. See [0.1.7 evidence and limits](PrivateServer/docs/native-017.md).

Use matching prepared clients and the combined backend for player-hosted co-op,
friend battles and trades. Enter a friend's world through the native Party menu;
friend-code membership does not automatically move the game. Leave Inverna for
your own world before a guest joins it.

Custom trainer support now includes owned native PTTR pack publication, native
gallery delivery and appearance metadata. In-game custom-art rendering still
needs verification, and the editor's authoring ZIP is not a native PTTR pack.
See [trainer sprites](PrivateServer/docs/trainer-sprites.md). Official 0.1.10
support awaits the released game artifact; no 0.1.10 parity is claimed.

## Credits and license

Original tooling developed with OpenAI Codex assistance for dr4lera. Credit the
original Platinum Together developers for their game and creator website, and
the PKHeX authors for PKHeX.Core. No official endorsement is implied.
GPL-3.0-or-later; see [LICENSE](LICENSE) and [third-party notices](THIRD_PARTY_NOTICES.md).


## Separate hosting projects

Server settings live in server config files and host administration tools. The
[Inverna server](https://github.com/dr4lera/pokemon-platinum-together-open-sourced-inverna-server) and [world server](https://github.com/dr4lera/pokemon-platinum-together-open-sourced-world-server) projects
contain distinct hosting guides and deployment configs for shared-city and co-op
protocol services. These repositories are development source previews. Both depend on the
[shared backend](https://github.com/dr4lera/pokemon-platinum-together-open-sourced-server).
See the [configuration reference](https://github.com/dr4lera/pokemon-platinum-together-open-sourced-launcher/blob/main/PrivateServer/docs/server-config.md)
for roles, independent data folders and current native/federation limitations.
