# Verification status — dev12

Dev12 server previews publish the tested revision 6 backend and standalone hosting
profiles. Gameplay-client release publication remains paused during the
[independent gameplay-client migration](native-gameplay-rules.md). The native
play evidence below concerns matching prepared reference clients, not the new
source client. Its EXP/OT/capture rules are implemented separately; full Together
parity remains unfinished.

Native gameplay, friend battles and trading work with matching project-prepared
0.1.7 clients on the combined private backend. The user confirmed battle and trade
success on 2026-10-20 after the native game tests. This is user-confirmed gameplay
evidence; the automated suite does not drive a full native battle or trade.

## Changes and checks

- All 40 Node backend tests pass: authenticated relay routing, party ownership,
  ready messages for 0.1.5/0.1.7, city packet layout, trainer revision/name
  preservation, appearance updates, city-to-world allocation guard, optional
  friend codes, custom trainer ownership/pack structure and gallery delivery.
- The Windows launcher builds with zero warnings and passes its self-tests,
  including pinned friend invites and gallery-helper response validation.
- Native city position packets use the authenticated sender slot. Name, chosen
  look and outfit changes are sent to peers already in Inverna. The native game
  hides the local player's own nametag; peer nametags require a valid pose.
- Invitations can form a party in Inverna. World allocation is refused while
  the host remains there. Return to the host's own world before the guest uses
  the native Party menu to join. Joining a code alone never allocates a seat.
- Custom trainer support adds signed owned PTTR publication, persistent selection,
  native collection/download routes and a pinned loopback gallery helper that
  follows the private game's lifetime, including launcher reset.
- Standalone role wrappers have Windows startup, role/health, read-only config
  and owner-signed shutdown tests. Consult Actions for executed CI platforms.

## Supported path and remaining work

Use matching prepared clients and the combined backend for the confirmed native
gameplay path. Stock official-client interoperability is unsupported; see
[connection compatibility](players.md#connection-compatibility). Separate Inverna
and world roles retain their own restrictions and do not establish cross-server
federation or player-world handoff.

Public matchmaking/PvP services, automatic invite-to-world transition, interrupted
battle/trade reconnect recovery, exhaustive trade save/reload/failure coverage,
relay Soul Link startup and linked-loss coverage, arbitrary-FPS native simulation,
unattended native world simulation and custom map/story execution remain unfinished
or unverified. Successful friend battles/trades do not establish those features.

Native PTTR transport and structural/ownership validation are tested. A real custom
trainer's rendering and every animation still require native verification. The
editor exports an authoring ZIP; converting that ZIP into a native PTTR pack is
unfinished. See [trainer sprites](trainer-sprites.md).

Official 0.1.10 is announced but was not available to inspect. No new executable
hashes, item IDs or 0.1.10 parity have been invented. See [migration](upgrade-0110.md).

## Existing compatibility and safety checks

The verified 0.1.7 native game and complete local assets can be prepared into an
isolated private client. Earlier preparation checks validated 2,819 asset entries
and preserved private save, identity and control-binding canaries on repair. The
0.1.7 gift save adapter has copied-save/CRC coverage; full native gift collection
remains unverified. The legacy injected trainer stays disabled for 0.1.7. See
[0.1.7 details](native-017.md).

The quarantined 0.1.6 executable hash remains blocked. No security exclusions,
quarantine restores or game executables are included. Process-memory test helpers,
research disassembly, game art, ROMs, saves, account profiles, private keys and
databases are excluded from releases. Original tooling uses normal native network
interfaces and separately prepared game files supplied by the player.
