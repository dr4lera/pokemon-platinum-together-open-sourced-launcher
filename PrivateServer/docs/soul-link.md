# Soul Link: native co-op

Soul Link belongs to the native co-op game, using its peer-to-peer or relay
connection. It is not a custom persistent-server mode, Inverna world template or
server plugin. Do not add
a `soulLink` option to hosting configuration, or select a custom world server to
start it. The relay carries authenticated peer traffic; native game rules apply
the encounter pairing, linked losses and partner logs.

## Native 0.1.7 selection route and connection limit

**Host my world → New world → Challenge Run → Custom Challenge → Soul Link →
Invite friends** was exercised in an ordinary isolated 0.1.7 client. The enabled
primary action advances to settings and a **Custom Soul Link** review. With
user-assisted navigation, **Lock and start** reached the native hosting/invite
lobby with those rules locked. A separate clean 0.1.7 client joined through its
direct-connect code, displayed matching rules, and readied Player 2. The host
then started both clients into the new-world introduction. The guest's
introduction explicitly acknowledged the host's Nuzlocke with Soul Link rules.
This verifies local lobby joining, readiness and coordinated startup; linked gameplay and relay
startup still need their own checks.

The action selects **FRIENDS: RIGHT AWAY**. The actual settings UI displays a
peer-to-peer IP warning, and the observed lobby used that connection mode.
Native code also says **Later (safe)** hosting unlocks after the first
Gym badge. Switching that timing preserves Soul Link rules, but it cannot be
represented as relay hosting from the first route. No supported per-draft relay
selector has yet been verified. Relay Soul Link startup remains unverified.

Show which native connection mode is in use; neither is a custom persistent-world
server. The remaining two-client acceptance checks below are still required.

## Release checks still in progress

The following must pass with separate native clients and disposable saves before
this preview advertises working Soul Link:

1. A host selects Soul Link through the existing co-op creation controls.
2. A second account joins through the party/code flow. Direct local joining
   passed in 0.1.7; repeat over the relay before advertising relay Soul Link.
3. Both clients enter with matching Soul Link rules and observe each other.
4. Each player's encounter log reaches the partner; linked-loss behavior applies
   to the intended pair, including a partner temporarily disconnected.
5. Reconnection and save/reload retain the pairing without duplicate losses or
   overwriting either player's unrelated saves.
6. Failed invites, a full party or an unavailable relay leave a recoverable UI.
7. Players move independently with their own camera, cross doors/routes and enter
   different maps without pulling other players along. A battle, menu or loading
   transition on one client must not force followers or teleport the party.
   Soul Link rule messages must still arrive when partners occupy different maps.

Current evidence: the shipped native client includes Soul Link rule selection,
partner-log exchange and linked-loss handlers. Local native relay movement has
passed in a different test world, including an Eterna Forest/Route 205 round
trip in 0.1.5. The separate 0.1.7 direct-connect startup check above does not
establish linked-loss, reconnection or cross-map rule delivery. In 0.1.5 the
native code screen joins the party; **Menu > Party >
Join World** is the tested manual world-entry route. Automatic handoff remains
under investigation. Published updates remain development previews.

The current 0.1.5 native creation path was inspected through **Host my world →
New world → Challenge Run → Custom Challenge → Soul Link**. It still enters a
solo-creation context and disables the final choice as **Needs a friend**, despite
both lab identities belonging to the party. This is an unresolved client startup
integration issue, not evidence that Soul Link needs a custom persistent server.

A 2026-10-20 follow-up repeated creation in a new disposable profile with no
copied saves or worlds. Read-only diagnostics confirmed normal image integrity,
an authenticated private identity and party membership. The native Host screen
required creating a world before relay or peer-to-peer hosting, and its fresh
Custom Challenge > Soul Link choice still displayed **Needs a friend**. Fresh
profile setup alone therefore does not resolve this startup issue. No game
binary, process memory or rule validation was modified for this test.

Discord invites use the same co-op party flow; see [Discord setup](discord.md).
Custom persistent server configuration remains separate: [hosting](server-hosting.md).


## Launcher mode-preservation correction

The launcher previously replaced the native multiplayer configuration on every
launch with `last solo`, dropping the game's saved mode and server entries.
It now updates only the private hub address/key and preserves the native `last`
line, saved server entries and unknown settings. Only a fresh configuration gets
a default solo entry. The update is staged atomically and refuses a concurrent
native-game/settings change. Preservation/idempotency checks pass. This fixes a
launcher regression; it does not by itself establish Soul Link startup or repair
the native solo-creation context described above.
