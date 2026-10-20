# Timing and independent movement requirements

The rework must keep gameplay speed independent of presentation frame rate.
Rendering FPS, simulation frequency and network packet frequency are distinct.

## Native co-op and Soul Link

- Preserve the original game's fixed simulation ticks and frame-based RNG. Render
  frequency must not decide how many simulation ticks or RNG advances occur.
- Movement, battle state and gameplay timers advance from simulation time. UI,
  camera interpolation and visual effects may use elapsed render time in seconds.
- A simulation accumulator may execute multiple fixed ticks for a slower render
  frame. Limit catch-up work after stalls and define pause/reconnect behavior;
  never pass one giant delta into physics or replay an unbounded backlog.
- Soul Link rule events are independent of rendering and map visibility. Apply
  each loss/log event once, even if a partner changes maps or reconnects.
- Each player has independent input, position, camera and map transitions. The
  co-op relay must not force movement, teleport guests to the host, or require all
  players to leave an area together. A player's battle/menu must not pull peers.

## Server, launcher and editor

The current backend has no game simulation tick loop. It forwards authenticated
native packets and stores explicit world data. Its timers and deadlines use time
values rather than rendered frames; packet budgets are abuse limits, not movement
speeds. This does not establish native frame-rate independence. Use monotonic
elapsed time for future runtime scheduling and interpolation. Keep wall-clock UTC
for account expiry, signed protocol timestamps and durable visit dates.

Launcher presence polling and background requests use elapsed timers, independent
of UI redraw. Editor animation must derive its pose from elapsed time; collision,
map coordinates and exported content must not change with editor refresh rate.
Do not implement gameplay as one movement increment per UI timer callback.

## Required checks before claiming completion

Measure the same timed movement, gameplay timers and battle/RNG simulation at
30/60/120/144 presentation FPS and under jitter, frame stalls and lost focus.
Compare simulation ticks and movement over the same elapsed interval. Confirm
doors/routes and different-map exploration while partners remain stationary or
enter menus/battles. Check that linked Soul Link events still arrive across maps.

Current evidence is limited to independent local native relay movement in the
0.1.5 lab. Cross-map behavior, Soul Link startup and multiple-FPS native checks
have not passed. Windows Security blocked the separate 0.1.6 lab executable on
2026-10-19, so that build's native tests are currently blocked. Source/build tests
alone must not be presented as proof that these gameplay requirements work.
