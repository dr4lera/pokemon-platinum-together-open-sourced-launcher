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
| Discord presence | Local Discord accepted application 1561916972508385380 and SET_ACTIVITY; framing and bounds tests pass | Uploaded logo asset and visual check in Discord |
| Bans and plugins | Owner-signed ban/disconnect/restart tests; proxy IP checks; plugin worker and private chat command tests | Live native UI verification and final installation |
| Controls | Capture, SDL conversion and INI preservation checks | Every remap tested in-game |
| Map Studio | Editor, project/pack round trips, integrity/path rejection | Native loading, assets and rendering fixes |
| Modes | Truthful capability table | Adventures, runs, trading, guilds, PvP and other missing routes |

The isolated native runtime loads and saves a disposable world with normal game
file integrity checks retained. A real guest now joins a party by code and requests
a world; the native host authenticates to the relay. An isolated guest loaded its local world, authenticated to the same relay and
replicated northward movement visibly on an uninstrumented host. Normal Join-code
handoff, remote play, recovery and persistent worker lifecycle remain unverified.
The research helper is not a shipped dedicated server and is excluded from builds.

Custom-world scope also includes owner-editable story definitions (dialogue,
quests, conditions, NPC behavior, progression and encounters), versioned alongside
world maps and persistent progress. This is requested work, not implemented native
support. Existing story flag storage alone does not fulfill this requirement.

Do not change capability flags to conceal these gaps. A production release needs
repeatable native tests with separate profiles/saves, failure recovery and no
interference with the player's original game.


## Latest local development changes

- Separate Inverna/world hosting repositories are published as development source previews.
- Backend config validates keys/types and resolves paths against its file.
- Role-specific feature declarations and protocol refusal have automated tests.
- Joined-world history requires authenticated entry, respects account/membership
  boundaries and survives restart. The launcher shows the selected server's list.
- Name includes saved server selection. Server management stays in config/host CLI.
- 24 backend/config tests passed locally; Windows launcher builds without warnings.
- Docker and Linux service recipes are documented but not executed on a Linux host.
- Normal native join handoff, persistent simulation, cross-server identity
  federation and native story/map integration remain unfinished. No new upload.

Discord invite transport and authenticated party joins are implemented locally.
The desktop Discord client accepted ACTIVITY_JOIN subscription and SET_ACTIVITY.
A real two-person invite and native automatic world transition are still pending;
see [Discord setup and limits](discord.md).

Soul Link must use native co-op/relay, independently of custom persistent world
servers. The 0.1.5 relay creation UI currently retains a solo-creation flag and
shows **Needs a friend** in its Soul Link picker. This is not fixed or verified.
Independent cross-map exploration and frame-rate independence are release checks;
see [Soul Link](soul-link.md) and [timing/movement](timing-and-movement.md).

The isolated 0.1.6 client started with manifest-validated assets and unmodified
signature checks, then Windows Security blocked its lab executable on 2026-10-19
as `Trojan:Win32/Bearfoos.A!ml`. No security settings or exclusions were changed.
Native 0.1.6 testing is blocked pending review of that detection. It is not being
worked around by copying/renaming the detected executable.

## Security rollback and safe testing

The experimental process-memory-patching test entry point has been retired locally.
It is not included in this source release or portable package. The launcher refuses
preparation, gift-adapter support and launch for SHA-256
`5520b7d176a3598b67f3c126359deada0f4099a6f961143453d7d790d69ab6c2`,
the lab game build quarantined by Windows Security. This hash block is not a malware
scanner and does not certify other builds as safe. No exclusions or quarantine
restores were performed. The packaging script refuses game executables and that
hash. Defender completed a custom scan of the dev4 portable folder on 2026-10-19
without a new detection recorded.

Backend tests passed (24), Discord IPC checks passed, and gift/save-integrity checks
passed after this rollback. MailSender builds, but NuGet vulnerability metadata
was unavailable during restore (NU1900). Native Soul Link startup, cross-map travel
and frame-rate independence remain unverified. These source updates must not be
represented as a completed gameplay release.

### Original-installation detection follow-up

Defender also recorded the same threat ID for the original installed
`AppData/Local/PlatinumTogether/Game/Platinum.exe` on 2026-10-19.
The cached 0.1.6 release envelope's Ed25519 signature verifies against the
production key embedded in the official launcher. Its executable hash is the
same blocked hash listed above. Before quarantine, the lab copy matched it.
This establishes release provenance, not that the executable is harmless.
No independently confirmed false-positive resolution was found, and no file
was restored, excluded, re-downloaded or renamed to evade the detection.
