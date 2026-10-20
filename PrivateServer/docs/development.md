# Build and contribute

Use Windows x64, the .NET 10 SDK and Node 24.12 or later. Native trainer adapters
also need a compatible x64 C++ compiler. The compatible backend is original code;
no official backend source or copied game executable is part of this project.

From the source root:

```powershell
dotnet build PrivateServer/Launcher/Launcher.csproj -c Release
dotnet build PrivateServer/MailSender/MailSender.csproj -c Release
dotnet build MapStudio/MapStudio.csproj -c Release
node --test PrivateServer/Server/test/backend.test.mjs PrivateServer/Server/test/config.test.mjs
```

Published GUIs accept `--selftest`. Run local integration tests with loopback
network access; sandbox `EACCES` failures are not protocol failures. Do not change
test assertions merely to make a blocked network environment pass.

Keep native testing in separate lab folders with disposable profiles. Do not
attach experimental adapters to a user's running game. Verify executable hashes,
exact process ownership, new log timestamps and save checksums. A stale screenshot
or log is not evidence for a new build. Record outstanding native gaps honestly.

Never commit game files, ROMs, copied/extracted artwork, research disassembly,
personal saves, `.env`, private keys, identity profiles, databases or mail receipts.
Use an explicit source allowlist for export, not a broad copy of a portable folder.
The local launcher may read artwork from the user's installed game at runtime.

The launcher and mail sender depend on PKHeX.Core (GPL-3.0-or-later). The vendored
WebSocket library retains its MIT license. Bundled runtimes require their own
upstream notices. Include source and licenses with redistributed builds. Original
tooling was developed with OpenAI Codex assistance; the original game's authors
retain credit for their game. This is not an official game-source release.
