# Verification status

This document is a release gate, not a promise that every requested feature is done.

## Current dev11 checks (2026-10-20)

- All 28 backend tests pass, including two-build relay authentication/routing,
  native 0.1.7 ready-field/limit checks and mismatched-build refusal.
- Both standalone hosting wrappers pass real Windows startup, role/health and
  owner-signed shutdown checks. Arguments are strict; `--check` is read-only,
  missing configs fail clearly, and Node 24.12+ is enforced. Windows/Linux CI is
  configured separately; see each repository's Actions result for executed scope.
- Map Studio can select authoring targets 0.1.5/0.1.6/0.1.7 in Pack details;
  export/import preserves each target. Native installation remains unavailable.
- Launcher preflights the complete copied asset set before replacing files. An
  actual preparation test rejected a late corrupt source asset without replacing
  earlier files. Repair preserves save, identity and binding canaries.
- The separately audited 0.1.7 gift save adapter is enabled. Native collection,
  capture/Dex/storage/reload and two-player connection-status tests remain pending.
- Native title rendering was visually observed in a focused isolated 0.1.7
  window. Input guards detected human activity; further gameplay testing waits
  for an uninterrupted private test window.
- A supported native 0.1.7 Soul Link **Invite friends** route was identified;
  it has not yet passed the two-client acceptance sequence.
- The 0.1.7 retail player explicitly reports authoring unavailable and rejects
  Studio import entrypoints. No supported unattended headless world runtime was
  identified. Custom map/story execution and permanent native simulation are
  implementation gaps, not features of the Node storage service.

## Dev10 compatibility checks (historical)

The launcher now accepts the verified official 0.1.7 executable for native play
and preparation, separately from the older save/memory adapters. The release
signature, cached package and installed executable hashes were checked; the
ordinary isolated client reached its title/start menu with native integrity
checks active. Defender's custom executable scan completed without a detection.
That scan is not a security certification. The quarantined 0.1.6 hash remains blocked.

The actual launcher preparation method also passed against the installed 0.1.7:
2,819 asset entries validated, cached native release envelopes copied, and no
original saves/worlds/credentials imported. A second preparation preserved
private save, credential and control-binding canaries byte-for-byte.

The 0.1.7 native client carries the revised connection-status UI, modern-catch
code and its regression guard, current patches/translations and native pacing.
Our relay's ready-message limits fit its parser. Full ordinary capture/Dex/save
and two-player 0.1.7 online-badge tests remain pending. Automated window capture
returned black frames/timeouts for both the original and isolated 0.1.7 client;
therefore title/start evidence comes from the current native log, not a screenshot.

The existing managed builder passed 2,400 starter constraint combinations and
4,800 encrypted PK4 round trips. This does not validate an old injected trainer
against 0.1.7. Dev10 restricted custom Poké Mart delivery to 0.1.5; dev11
separately audits and enables its 0.1.7 format as described above. No two-player
Soul Link pass is established by these checks. See
[build details, starter behavior and test limits](native-017.md).

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

Earlier research demonstrated movement using an experimental runtime helper.
That helper is retired and excluded from releases; it is not proof of ordinary
client usability. On 2026-10-19, two ordinary, unmodified 0.1.5 lab clients
authenticated, joined a party by the native code UI, then entered the same world
through **Menu → Party → Join World**. Both clients displayed the peer. This was
repeated after a clean backend restart without the discarded unsolicited
allocation hook. See [the tested manual sequence](players.md#manual-relay-joining-in-015).
The combined backend now advertises that manual relay capability. Separate
Inverna/world deployments do not advertise it. Native shortcut handoff, Soul Link
startup, remote gameplay, arbitrary-FPS independence
and persistent worker lifecycle remain unfinished.

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
Broader cross-map coverage and frame-rate independence are release checks;
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
was unavailable during restore (NU1900). Native Soul Link startup and frame-rate
independence remain unverified. These source updates must not be
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


## Ordinary native relay entry and private sign-in correction

The native code screen accepts party membership but does not create the guest's
pending world request. The existing native Party menu does. Local testing observed
both clients authenticate and display one another using that menu, then repeated
the sequence after restarting the backend. Native integrity checks remained
active; no process writes, injection or game binary changes were used.

Launcher sign-in now updates both private native credential locations, with
staging, previous-file preservation, running-game/concurrent-change guards and
path-link rejection. The launcher build and self-tests passed. All 27 backend,
configuration and transport timing tests passed with loopback network access.
The regression test explicitly refuses unsolicited allocation on party-code join.
The complete rework is still unfinished.

## Ordinary native travel, reset and editor timing checks (2026-10-20)

Two unmodified 0.1.5 lab clients passed a bounded independent-travel check. The
guest moved from Eterna Forest to Route 205 and back while the host stayed at
the same forest coordinates. Both remained connected. A guest wild battle and
normal flee also left the host stationary and unaffected. This covers that
route boundary and battle, not every map/story transition or Soul Link event.

The reset action now uses a normal close and restart of only the private client.
Its native close confirmation remains visible; it is never force-terminated.
Local testing observed a reopened title screen and unchanged save-file hashes,
plus a declined-close timeout that left the client alive. The unreliable DS key
chord has been removed from the launcher action.

Map Studio scene effects and sprite previews derive animation time from a
monotonic stopwatch. Boundary, skipped-frame and 30/60/120/144 callback-rate
checks passed. This does not establish native game simulation independence.
Soul Link startup and arbitrary-FPS native simulation remain unfinished.
