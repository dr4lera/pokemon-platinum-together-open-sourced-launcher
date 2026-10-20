# Plat Together Open Sourced

Community private-world tooling for an existing Platinum Together installation.
This is a development source release, not the original game source. Bring your
own supported game and ROM. No game assets, ROMs, saves or account keys are included.

## Project repositories

- [Launcher, mail app and full documentation](https://github.com/dr4lera/plat-together-open-sourced)
- [Server and relay backend](https://github.com/dr4lera/plat-together-server)
- [Map Studio and trainer sprite tools](https://github.com/dr4lera/plat-together-map-studio)

## Start here

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
git clone --recurse-submodules https://github.com/dr4lera/plat-together-open-sourced.git
cd plat-together-open-sourced
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

Native two-player joining, unattended world simulation, all game modes and native
custom map/sprite loading remain unfinished. Backend tests and editor previews do
not establish those features. See the linked status page before deploying.

## Credits and license

Original tooling developed with OpenAI Codex assistance for dr4lera. Credit the
original Platinum Together developers for their game and creator website, and
the PKHeX authors for PKHeX.Core. No official endorsement is implied.
GPL-3.0-or-later; see [LICENSE](LICENSE) and [third-party notices](THIRD_PARTY_NOTICES.md).
