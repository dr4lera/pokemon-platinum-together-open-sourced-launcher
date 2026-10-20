# Map Studio and content packs

Run `MapStudio/InvernaMapStudio.exe`. Save an editable project before exporting.
Projects support multiple maps with terrain, collision, spawn positions, dialogue,
warps and objects. Objects include buildings, benches, planters, lamps, NPCs,
signs and effects, with dimensions, elevation, rotation, visibility, colors,
lighting and effect parameters. Some fields are metadata awaiting native support.

Select objects using the editor's object picker. Duplicate when reusing a building
or effect. Check collision and spawn placement before export. Warps must name an
existing map and a valid destination. Keep stable unique IDs for maps and objects.

The `.ptpack` format is a ZIP containing exactly `manifest.json` and `project.json`.
It records schema, target, version and a SHA-256 content digest. Import validates
the archive entries, limits, digest, IDs, map dimensions and supported values.
Do not manually add assets to the ZIP: the current schema rejects extra entries.

**Native game map loading is not connected yet.** An exported pack is an authoring
artifact, not an installed in-game expansion. The preview uses the editor's own
renderer and is not evidence that the native engine will show those objects.
Inverna's native rendering fixes and the runtime adapter remain open work.
