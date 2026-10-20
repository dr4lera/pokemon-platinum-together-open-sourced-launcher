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

The ordinary 0.1.5 lab passed a bounded independent-travel check: the guest left
Eterna Forest for Route 205 and returned while the host stayed at its original
position in Eterna Forest. Both remained connected to the relay. An earlier
guest battle also left the host free and stationary. This verifies that tested
boundary and battle, not every route, story transition or Soul Link event.
Soul Link startup and multiple-FPS native checks have not passed. Windows
Security blocked the separate 0.1.6 lab executable on
2026-10-19, so that build's native tests are currently blocked. Source/build tests
alone must not be presented as proof that these gameplay requirements work.


## Implemented backend elapsed-time correction

Connection traffic windows and relay byte/packet budgets now use a monotonic
clock (`performance.now`) instead of wall-clock `Date.now`. Byte refill is
proportional to elapsed seconds with a capped burst after stalls. Tests cover
clock-zero boundaries, backward-clock defense and equal refill at 30/60/120/144
callback rates. Protocol timestamps and durable expiry remain UTC-based.
This concerns transport scheduling; the native game simulation/render loop still
requires its own implementation review and in-game verification.


## Implemented editor elapsed-time correction

Scene effects and sprite previews now use a monotonic stopwatch. Timer callbacks
request a redraw; animation time and pose selection derive from elapsed time.
Delayed callbacks skip old presentation frames instead of slowing the animation
or replaying an unbounded backlog. Checks cover pose boundaries, missed frames
and 30/60/120/144 callback frequencies. This is verified editor behavior.

The native 0.1.5 game also has supported `60Fps` and `60FpsSpeedFix` settings,
already enabled by default in the lab. Its existing correction adjusts several
movement increments and durations. That does not demonstrate preservation of
original RNG/task cadence or arbitrary-FPS rendering; those native requirements
remain open.
