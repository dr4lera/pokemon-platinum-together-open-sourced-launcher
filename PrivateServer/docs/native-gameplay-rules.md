# Native gameplay rules and independent client

Current launcher releases prepare and run an unmodified game supplied by the
player. Our launcher, backend and editors are source-controlled; that does not
make the supplied Together executable our source code.

The [independent source client](https://github.com/dr4lera/pokemon-platinum-together-open-sourced-client)
is now a separate migration project. It starts from the public Platinum PC-port
engine and implements community behavior in source. Full Together 0.1.7 parity,
modern content, co-op and native private-server integration are unfinished there.
Its rule changes do not silently modify an existing prepared reference client.

## Checked reference behavior — 0.1.7

- EXP caps already apply regardless of original trainer. Already-overlevel
  Pokémon gain no further EXP at that cap; they are not reduced in level.
- The native battle caught-ball marker is implemented, including supported
  modern species and received-trade Dex registration. Caught and merely seen
  are different states. The marker belongs to the opposing wild-battle HUD;
  some battle/layout types suppress it intentionally.
- Foreign-OT badge disobedience remains a separate rule in the reference.
- The two-opponent ball refusal exists, but the co-op wild-double battle flags
  miss its exact-type check. Local and peer inventory-removal paths can also
  ignore failure. Trainer-owned capture refusal is a separate existing check.

These findings come from code inspection of the exact installed artifact; no
new gameplay test is claimed. Decompilation/disassembly remains a private
reference, and the original signed executable and runtime checks are unchanged.

## Implemented independent-client rules

The source client applies cap rules to battle EXP, Candy and DayCare paths;
removes extra foreign-OT disobedience while preserving provenance; shares a
wild-only, exactly-one-active-opponent predicate between UI and execution; and
requires successful single-ball removal before scheduling a throw. Duplicate
actor/turn actions and stale authorizations are rejected, and the capture target
is fixed for that throw. Existing caught-HUD handling is retained.

Portable C rule and config checks pass. They do not establish complete native
play, save compatibility, co-op inventories or Together parity. Read the client
repository's migration matrix for the current build and integration status.

New launcher/server releases are paused during this migration. The last published
downloads remain available; source updates must not be presented as a completed
1:1 independently playable Together client.
