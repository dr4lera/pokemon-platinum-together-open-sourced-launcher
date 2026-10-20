# Verification status

This document is a release gate, not a promise that every requested feature is done.

| Component | Evidence | Still required |
|---|---|---|
| Launcher | Builds; UI/DPI checks; private preparation reaches native welcome | Fresh user setup with actual ROM extraction |
| Names | Unique names, key ownership, rename/recovery integration tests | Full native fresh-player flow |
| Friends/invites | Consent, presence, capacity and leadership integration tests; native parser fixtures | Native two-player acceptance and join |
| Relay | Authenticated ticket/nonce/routing tests; public HTTPS/WebSocket tunnel test | Native remote world join and synchronization |
| Persistent storage | SQLite and save integrity/restart tests | Unattended native world simulation |
| Mail/gifts | Owner authentication and durable claim tests; native save/CRC fixtures | Actual native claim/collection |
| Controls | Capture, SDL conversion and INI preservation checks | Every remap tested in-game |
| Map Studio | Editor, project/pack round trips, integrity/path rejection | Native loading, assets and rendering fixes |
| Modes | Truthful capability table | Adventures, runs, trading, guilds, PvP and other missing routes |

The hidden native runtime experiment can load/save a disposable world, but a
recent renamed-process launch timed out without producing a fresh runtime log.
It is not a shipped dedicated server and is excluded from the public build.

Do not change capability flags to conceal these gaps. A production release needs
repeatable native tests with separate profiles/saves, failure recovery and no
interference with the player's original game.
