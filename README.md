# Pokemon Platinum Together Open Sourced - Launcher

Community private-world tooling for an existing Platinum Together installation.
This is a development source release, not the original game source. Bring your
own supported game and ROM. No game assets, ROMs, saves or account keys are included.

Multiplayer support targets matching project-prepared clients. Joining a project
host from a stock official client is unsupported in this preview; a user reported
**World not found**. Official game-file compatibility does not establish
official-service interoperability. See [connection compatibility](PrivateServer/docs/players.md#connection-compatibility).

## Downloads

**[Download the full Windows bundle — dev11 preview](https://github.com/dr4lera/pokemon-platinum-together-open-sourced-launcher/releases/download/v0.1.0-dev.11/PokemonPlatinumTogetherOpenSourced-Windows-x64-dev11.zip)**

Extract the complete ZIP and open `Launcher/PlatinumPrivateLauncher.exe`. Keep
the folder together. MailSender, Map Studio, the backend and Windows runtimes
are included. Read the [quick start](PrivateServer/docs/quick-start.md) first.

| Component | Download page | Setup |
|---|---|---|
| Launcher and full Windows bundle | [dev11 release and checksum](https://github.com/dr4lera/pokemon-platinum-together-open-sourced-launcher/releases/tag/v0.1.0-dev.11) | [Install and play](PrivateServer/docs/quick-start.md) |
| Shared server / relay | [Server release](https://github.com/dr4lera/pokemon-platinum-together-open-sourced-server/releases/tag/v0.1.0-dev.11) | [Hosting guide](PrivateServer/docs/server-hosting.md) |
| Standalone Windows editors | [Editors release](https://github.com/dr4lera/pokemon-platinum-together-open-sourced-editors/releases/tag/v0.1.0-dev.11) | [Map and sprite tools](https://github.com/dr4lera/pokemon-platinum-together-open-sourced-editors#download) |
| Separate Inverna hosting | [Inverna release](https://github.com/dr4lera/pokemon-platinum-together-open-sourced-inverna-server/releases/tag/v0.1.0-dev.11) | [Inverna setup](https://github.com/dr4lera/pokemon-platinum-together-open-sourced-inverna-server#first-run) |
| Separate world backend hosting | [World release](https://github.com/dr4lera/pokemon-platinum-together-open-sourced-world-server/releases/tag/v0.1.0-dev.11) | [World setup](https://github.com/dr4lera/pokemon-platinum-together-open-sourced-world-server#first-run) |

All five repositories have matching dev11 **development previews**. Each release
includes a SHA-256 checksum file and documents its tested
scope and unfinished native features. The Latest label identifies the current
download; it does not establish completion of the native gameplay rework.

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
- [Mail, Pokémon, eggs and gifts](PrivateServer/docs/mail-and-gifts.md)
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

Manual local relay joining is verified with two unmodified 0.1.5 clients through
Menu → Party → Join World after joining the party code. A guest also crossed
Eterna Forest → Route 205 and back while the host stayed stationary and both
remained connected. That verifies the tested route, not every map or story event.
Automatic shortcut handoff, Soul Link startup, arbitrary-FPS native gameplay,
unattended world simulation and native custom map/sprite loading remain unfinished.
Backend tests and editor previews do not establish those features. See the linked
status page before deploying.

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
