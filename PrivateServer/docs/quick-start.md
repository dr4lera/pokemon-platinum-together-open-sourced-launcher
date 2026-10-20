# Quick start

## Requirements

- Windows x64 for the launcher, editor and mail sender.
- A locally installed, supported Platinum Together game: verified 0.1.5 or 0.1.7.
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

To adopt the official 0.1.7 update, first update your original installation with
the official launcher. Save and close the private game, then run **Extract &
prepare private game** again in this launcher. This validates and replaces the
owned game assets while preserving the private save and profile. Back up your
private `Client` folder first. Updating our launcher alone does not update a
previously prepared game. Read [0.1.7 compatibility](native-017.md) for the exact
build and remaining adapter restrictions. The quarantined 0.1.6 build stays blocked.

## Before inviting friends

Use matching project-prepared clients and the combined backend. Native gameplay,
friend battles and trading are user-confirmed with 0.1.7. Join the party by its
native code or optional [community friend code](friend-codes.md), then use the
game's Party menu to enter the host's world. The host must return from Inverna
to their own world first. Both launchers must select the same reachable server.

Read [verification status](status.md) for remaining limits, including public
matchmaking, interrupted-trade recovery and unattended world simulation.
