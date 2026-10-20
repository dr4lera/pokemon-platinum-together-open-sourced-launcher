# Quick start

## Requirements

- Windows x64 for the launcher, editor and mail sender.
- A locally installed, supported Platinum Together game: verified 0.1.5 or 0.1.6.
- Your own Pokémon Platinum ROM if you want fresh extraction. An already extracted,
  validated installation can also supply the required assets.
- A complete toolset folder. Moving only the launcher EXE breaks its sibling paths.

## Create your private copy

1. Open `Launcher/PlatinumPrivateLauncher.exe`.
2. Pick a name: 3–16 letters, numbers or underscores, starting with a letter.
3. In **Game setup**, select your installed game's folder.
4. Optionally select or drop your own `.nds` ROM.
5. Choose **Extract & prepare private game**. The launcher checks the game hash
   and required asset hashes. An unknown version is refused.
6. Return to **Game**, choose **Use this name**, then **Play**.

The separate game lives in `Client/Game`. The original installation remains separate.
Do not share `Client`, `Data`, `Profiles`, `OwnerKeys`, account backups or saves.

## Before inviting friends

Read [the current support table](status.md). The backend supports social messages,
parties and authenticated relay routing, but complete two-player native gameplay
and unattended world simulation still require verification and implementation.
Do not treat this development build as a replacement for a finished dedicated server.
