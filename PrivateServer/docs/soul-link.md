# Soul Link: co-op relay mode

Soul Link belongs to the native co-op game and its relay connection. It is not a
custom persistent-server mode, Inverna world template or server plugin. Do not add
a `soulLink` option to hosting configuration, or select a custom world server to
start it. The relay carries authenticated peer traffic; native game rules apply
the encounter pairing, linked losses and partner logs.

## Release checks still in progress

The following must pass with separate native clients and disposable saves before
this preview advertises working Soul Link:

1. A host selects Soul Link through the existing co-op creation controls.
2. A second account joins through the party/code flow over the relay.
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
passed in a different test world. That evidence does **not** yet verify the Soul
Link start/continue flow or its rules across two players. Normal native Join-code
handoff remains under investigation. No new completed release has been uploaded.

The current 0.1.5 native creation path was inspected through **Host my world →
New world → Challenge Run → Custom Challenge → Soul Link**. It still enters a
solo-creation context and disables the final choice as **Needs a friend**, despite
both lab identities belonging to the party. This is an unresolved client startup
integration issue, not evidence that Soul Link needs a custom persistent server.

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
