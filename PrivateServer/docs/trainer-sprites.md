# Custom trainer sprites

Use the [official Sprite Creator](https://platinumtogether.online/creators/) for
drawing, layers, undo history and pose editing. Download your editable
`.ptdraft.json` project to keep a local backup. The site stores projects in the
current browser, so clearing browser data can remove unsaved work.

In the launcher, open **Create & packs → Trainer sprites**. You can also choose
**Trainer sprites** inside Map Studio. Import your version 3 creator project.
The tool reads your own saved frames, shows them at integer pixel scale and can
preview each pose sequence. Empty slots remain transparent; it does not fill them
with the game's reference trainer.

- **Back up project** preserves the complete original file, including layers,
  extra fields and history, byte for byte.
- **Export frame PNG** writes the selected frame at its original resolution.
- **Export pose sheet** keeps frame indices and transparent gaps. Known native
  pose-group slot counts are preserved.
- **Export art pack** creates a `.pttrainer` authoring ZIP with pose PNGs, hashes,
  layout metadata and the original editable project.

Only artwork saved in your project is included. Use artwork you own or have
permission to share and credit its artist. The original site's reference art and
application code are not bundled in this toolset.

These are authoring tools. The official site says game-pack support is still
being built; this rework's native sprite-install adapter is also unfinished.
Exporting an art pack does not automatically install a playable trainer or publish
anything to the community. Use **Open official creator** to continue editing and
reimport a new download when you are ready.
