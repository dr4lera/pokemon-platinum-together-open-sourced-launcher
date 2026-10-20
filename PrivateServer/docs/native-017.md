# Official 0.1.7 compatibility

The dev11 launcher accepts the complete, locally supplied official 0.1.7 native
client. It does not distribute the game or port old memory offsets into the new
executable. Keep all matching official assets together; do not replace only
`Platinum.exe` in an old private game.

## Verified identity

| Field | Value |
|---|---|
| Game version | 0.1.7 |
| Save schema | 1 |
| Gameplay release ID | `b113ab54d7b0e9a690c51e87` |
| Executable SHA-256 | `3688d6b1c5b762b9a1f8d74b64f3a6a62d4b07d525074a94b432e4b0b2866ec9` |
| Official package SHA-256 | `e28ded04dd366d6a59d0ed31cf916a2f1fd13157d041ede358d6772baa425f1a` |

The cached release envelope's Ed25519 signature was verified against the
production public key embedded in the official launcher. The cached package,
signed executable entry and installed executable matched. Native startup used
the original signature/integrity checks. Defender's custom scan completed with
no detection; this does not establish that every build or behavior is safe.
The previously quarantined 0.1.6 hash remains blocked. No exclusions or restores
are needed or recommended.

## Useful upstream behavior carried by native launch

- **Connection indicator:** 0.1.7 uses its connection-status resolver in the
  title's previous default OFFLINE branch. Its ready relay connection can report
  ONLINE. This follows actual connection state; a saved name/token alone does not
  establish a live connection. A one-client private-account ONLINE panel was
  observed live; the two-client relay gameplay check remains pending.
- **Modern Pokémon capture:** the client contains a modern Pokédex-entry helper
  used after normal capture and by a native entry-generation guard. That guard
  alone does not test storage or saved caught state. The user reported the
  official update fixes the old crashes. We have not yet reproduced every species
  capture, Dex entry, storage and reload in an ordinary disposable world.
- **Matching content:** preparation carries the complete owned asset manifest,
  including updated field scripts, effects, translations and Discord runtime.
  The game and these assets stay locally supplied.
- **Pacing:** the native client owns its performance-clock pacing and reports
  game steps separately from rendered frames. The isolated default run logged
  about 59.8 game steps/second. This does not prove arbitrary-FPS independence
  across all gameplay systems; no external speed or frame hooks were added.

The relay supplies a complete ready message with limits of 120 frames/second,
262144 bytes/second and a 262144-byte burst. These fit the inspected 0.1.7
relay parser. Persistent realm messages are a different protocol and are not
evidence of native unattended simulation.

## Starter shininess

Static inspection found a normal Gen-IV shiny check, not a general starter shiny
lock. The chooser caches three full personality IDs and compares each against
the current trainer ID. Changing region or reopening the picker reuses those
three rolls. It does not produce a fresh set for every displayed starter.
Final starter creation consumes the selected cached roll.

For hunting, restart to the title and begin the starter sequence again rather
than repeatedly switching the region. A fresh sequence can still roll non-shiny
many times; the inspection does not promise a shiny or a particular seed. Preview
and final metadata paths were inspected, but their equality has not been checked
in a live 0.1.7 shiny encounter.

The existing managed builder passed 16 catalogued starter species × 3 trainer
IDs × 25 natures × shiny/non-shiny = 2,400 combinations. Both explicit PID search
and automatic generation passed, with 4,800 encrypted PK4 round trips, 2,400
incompatible-trainer-ID rejections and 7,200 original-object preservation checks.
This verifies its PID algorithm only. It does not enable the old trainer bridge
for 0.1.7, add absent species, prove game legality or validate native capture.

## Upgrade and limits

Follow [Quick start](quick-start.md) to update the original game officially,
back up the private client and prepare it again with the dev11 launcher. Existing
private saves/profiles are retained; native code owns any supported save handling.

Custom Poké Mart gift loading uses a separately audited save adapter, enabled
for exact verified 0.1.5 and 0.1.7 builds. All 38 native save descriptor sizes
match; the gift offset (46292), data/block sizes (4904/4912), CRC placement,
8-slot payload layout, accepted active-generation selection and Pokémon/egg/item
callback structure agree. The adapter conservatively refuses recovery states
that the native game can sometimes repair. Copied-save testing passed 68 builds,
96 PK4 decode checks, 12 capacity fills and 11 refusal checks. Source saves were
unchanged. Those copies originated in 0.1.5, so ordinary 0.1.7 deliveryman
collection and expanded-species registration remain separate pending tests.
Backups, user confirmation, closed-game checks and claim journals still apply.
The legacy injected trainer is not enabled for 0.1.7. Native MAIL uses its own route.

The 0.1.7 frontend supplies a new **Invite friends** action after selecting
Soul Link. It deliberately switches creation into co-op context through the
native initializer; its selection, review and locked hosting/invite lobby were
observed live with user-assisted navigation. A separate clean 0.1.7 guest
joined by direct-connect code, readied, and entered the introduction when the
host started. This establishes local coordinated startup. The route
defaults to peer-to-peer **Right away**, while native **Later** hosting is locked
until the first Gym badge. A relay start from the first route is not verified.
Soul Link linked-loss/reconnection and relay startup, arbitrary-FPS native gameplay,
unattended world simulation and native map/story integration remain unfinished.
See [verification status](status.md), [Soul Link](soul-link.md) and
[timing/movement](timing-and-movement.md).
