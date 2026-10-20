# Backups, updates and troubleshooting

## Back up and restore

Close private clients, stop the backend cleanly, then copy the entire `Data`
directory to private backup storage. Preserve `Profiles`, `OwnerKeys`, private game
world folders, and mail claim receipts too. Treat these backups as credentials.
Restore a complete consistent backup while stopped. Never mix a SQLite database
from one date with unrelated identity keys or claim journals from another.

Game/ROM assets are replaceable from your own installation. Saves, owner keys,
server signing identity and player identity keys are not. Export map projects
separately; an unsaved editor session is not backed up by the server.

## Update

Keep mutable folders out of application replacement. Stop the owned processes,
back up, replace only application/runtime/server files, then restart and verify.
Do not overwrite `Client`, `Data`, `Profiles`, `OwnerKeys` or receipts with a
developer's portable folder. New game versions require a hash/adapter audit.
The private setup accepts only known builds; do not bypass that rejection.

## Reports and privacy

The private launcher does not upload diagnostic reports automatically. The
official launcher's separate local privacy patch can be replaced by official
updates and must be checked against the exact launcher build. Gameplay traffic,
private server synchronization and owner-controlled server logs remain distinct
from diagnostic-report uploads. Keep logs private before reviewing/redacting them.

## Common problems

| Symptom | Check |
|---|---|
| Missing runtime/server files | Restore the complete folder, preserving private data. |
| Unknown game hash | Use the supported installation; audit the new build before porting. |
| Address already in use | Choose a free port or stop the backend that owns it. |
| LAN setting not applied | Stop/restart the backend after changing bind/address. |
| Friends cannot connect from the Internet | Check the tunnel process and current URL; keep host awake. |
| Server identity changed | Verify the host's identity/backup; do not blindly replace trust. |
| Username already reserved | Use the original identity backup or another name. |
| Game mode is disabled | Read the support table; setting a feature flag does not implement it. |
| Save changed while editing | Close game, reload the save and review edits again. |
| Gift queue full | Collect existing native gifts; retry without deleting receipts. |
| Exported map not in-game | Native pack loading is not implemented yet. |
| New Pokémon still OFF | Edit the correct world's `world.ini`, then reload it. |

Activity appears in the launcher's **Activity** page. Backend metadata logs are
in `Data/events.jsonl`; they deliberately omit tokens, keys and raw save bodies.
Self-tests write result files beside the checked EXE.
