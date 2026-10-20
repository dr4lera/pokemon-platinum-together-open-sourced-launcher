# Custom trainer sprites

Use the [official Sprite Creator](https://platinumtogether.online/creators/) for
drawing, layers, undo history and pose editing. Download your editable
`.ptdraft.json` project to keep a local backup. The site stores projects in the
current browser, so clearing browser data can remove unsaved work.

In the launcher, open **Create & packs â†’ Trainer sprites**. You can also choose
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

## Native PTTR packs on private servers — dev12

The backend now accepts owned native PTTR binary packs (versions 1/2, template
`platinum-trainer-209-v1`), publishes the native collection/download routes and
stores the selected art separately from the built-in trainer look. Native
`trainer.art` changes update peers in Inverna. Launcher Play and reset provide a
loopback gallery adapter pinned to the selected private server; the helper stops
with its game. No official gallery credentials are used.

This transport has structural, signature, ownership, hash and persistence tests.
Rendering a real custom pack and all its animations still require native testing.
The server checks PTTR structure; the native installer additionally validates
palette, frame and pose semantics.

An editor **Export art pack** is an authoring ZIP, even with a `.pttrainer` suffix.
It is rejected by native PTTR import. The ZIP-to-PTTR compiler remains unfinished.
Use a native binary pack you own; do not rename a ZIP to bypass that distinction.

From the portable folder, import using your own selected server profile:

```powershell
.\Runtime\node.exe .\Server\main.mjs trainer-art --profile C:\path\your-profile.json --action import --file C:\path\trainer.pttrainer --metadata C:\path\metadata.json --no-select --out C:\path\import-result.json
```

Example `metadata.json` (choose stable unique creation/version IDs):

```json
{"creation":"my-trainer","version":"v1","title":"My trainer","creator":"Artist name","label":"First version","template":"platinum-trainer-209-v1"}
```

Creation/version IDs contain 1–40 ASCII letters, digits, `_` or `-`. Versions are
immutable; publish edited bytes under a new version. Collections allow at most
40 versions and must fit the native 16 KiB response limit. `--no-select` adds to
your library; choose the available pack through the native trainer appearance UI.
CLI actions `list`, `select` (with `--art` JSON containing the returned four-field
art object) and `clear` manage your own selection. Account profiles are private
keys: never share them with artwork or include them in a pack.
