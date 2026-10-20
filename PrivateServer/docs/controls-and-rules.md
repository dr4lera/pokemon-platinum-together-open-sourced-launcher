# Controls and world rules

## Rebind controls

Open **Controls**, load bindings, select an action, then **Change selected key**.
You can also double-click its keyboard cell. Pick controller buttons/D-pad entries
from the native SDL choices. Save only after closing the private game.

The launcher modifies the private game's `sim_config.ini` `[PAD]` binding values.
It backs up the previous file, preserves unrelated configuration and refuses a
stale file. Keyboard capture/key conversion and file preservation are tested;
every mapping still requires an in-game check. The original game's controls are
separate from the private copy.

## New Pokémon → Preview

This world rule is `modern_types=1` in the world's `world.ini`; it is not a field
in `save.bin`. Keep the configuration alongside the correct world's save.
Close the game before editing and reopen the world afterward. Do not replace a
whole `world.ini` merely to change this line, because other rules also live there.

The native UI describes this preview as newer Pokémon and Fairy-type changes;
some animations and abilities are still unfinished in the underlying game.
Enabling the rule cannot supply missing native implementations.
