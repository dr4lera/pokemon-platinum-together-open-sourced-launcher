# Announced 0.1.10 migration

The supplied 0.1.10 notes describe an announced release. No released 0.1.10 build
was available for this inspection. The installed verified game is 0.1.7. Original
community launcher/server source remains separate from the game's binary/assets.
This document records work to do, not shipped 0.1.10 parity.

| Area from the announcement | Our route | Acceptance required |
| --- | --- | --- |
| Six-letter invites | Optional original community-server extension shipped in dev12 | Backend tests plus ordinary two-client launcher/game handoff |
| Battling and trading | Working on matching prepared 0.1.7 clients via combined backend (user-confirmed); preserve the native trade journal | Complete battle and trade with separate disposable saves; reload both; interrupted trade/reconnect recovery |
| Reconnect, quiet restarts and clearer join errors | Extend server session lifecycle only after auditing the native recovery contract | Drop each connection mid-battle/trade without losing or duplicating Pokémon |
| Early co-op, Soul Link, tags, Fly, guest EXP and doubles | Audit new native messages/rules when the released artifact is available | Matching clients, starting-world checks, mode/rule agreement and cross-map travel |
| New Pokémon, routes, fossils, cries, abilities, animations and trainer items | Prepare the complete legitimately supplied new installation, then audit data/adapters | Native content, battle effects, capture/Dex/storage and save/reload checks |
| Controls, EXP Share, sound and accessibility | Read actual new binding/config schema before exposing controls | Preserve existing bindings; native keyboard/controller and preference checks |
| Story, rendering, followers, Underground and menus | Inherit the released game's fixes through complete validated preparation | Reproduce each affected area with copied test saves |
| Save, autosave, trade and Frontier safety | Prioritize before enabling new game/save adapters | Malformed-save refusal, interrupted-write recovery and unchanged original profile |
| Linux | Existing Node backend portability is distinct from a Linux native game package | Official Linux artifact and actual platform tests |

Do not add guessed executable hashes, fabricated item IDs, guessed abilities,
unverified save offsets or unsupported native entrypoints. Capsules and mints were
not found in the installed 0.1.7 item bank; the notes do not establish their IDs.
Friend battles and direct trading are user-confirmed on matching private clients.
Public matchmaking is a separate unfinished service; its capability flags remain disabled.

Dev12 checks: all 40 Node backend tests pass, including optional code
authentication, normalization, party capacity and disabled-mode compatibility.
The launcher builds without warnings and its self-test includes invite round trips,
wrong-server/pin rejection and malformed-code rejection. The released player refuses
`--render-check` with "A released game does not run hidden checks"; native verification
uses ordinary controls. No bypass is used.
