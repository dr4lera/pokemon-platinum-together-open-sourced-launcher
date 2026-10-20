# Names, joining and friends

Names are unique case-insensitively within one private server. A saved Ed25519
identity proves account ownership; typing another person's name does not sign in
as them. Independent servers have separate name registries. Native trainer names
have a 10-character display limit even though account handles allow 16.

Use **Name** to rename the same identity. It keeps player ID, friends and world
memberships. Close the private game before switching identities or renaming.
Use **Back up my name** to keep a private recovery copy. Losing the identity key
cannot be fixed simply by entering the old name.

To connect to another host, enable **Connect to another private server**, enter
its URL, then create/sign in with your name. The launcher stores a separate profile
per server and checks its signing identity. A changed signing key produces an
error; check with the owner before replacing trust or restoring old credentials.

The backend supports friend requests, acceptance/rejection/removal, presence,
private party invitations, four-player party capacity, leader changes and relay
allocation. Invitations require acceptance. Server tests exercise these flows
with real local HTTP/WebSocket clients. The complete native menu-to-world-join
flow is still pending, so do not assume a successful party invitation proves that
co-op world synchronization works.

The game-mode page reports available implementation rather than unlocking every
button. Adventures/Rifts, guilds, trading, public PvP and other unsupported modes
need their own implementation and native tests.


## Change Inverna/community servers

Use the existing Name tab. Enter the server origin in the address field and enable
remote mode, or choose a saved address and **Use selected Inverna server**. Close
the private game before switching, then choose **Use this name**. Each server
keeps its own identity/profile: a name on one server does not reserve it everywhere.
Saved addresses contain no signing seeds. Account backups remain private.

## View joined worlds

The existing World hosting tab now lists worlds from the selected server. Choose
**Refresh joined worlds**. Rows show the world, owner, type, last connection and ID.
The server records authenticated realm/relay entry; invitations alone do not count.
Records survive server restart and are visible only to their account. This is a
connection history, not a claim that native guest simulation is verified. Entry
and federation between separate Inverna/world deployments remain in development.


## Soft reset for encounter hunting

Use **Controls > Soft reset private game** while this launcher's private game is
running. It sends the native L + R + Start + Select chord using the saved keyboard
bindings in `sim_config.ini`; unsaved edits in the controls grid are not used.
Defaults are **Q + E + Enter + Right Shift**, which can also be held directly in
the game. The control does not kill/restart the process or edit/save Pokemon.
Only the private game at this launcher's path is eligible. An unknown or blocked
build, missing window, ambiguous process, held reset key, unsupported binding or
failure to focus the intended window prevents sending the chord. It releases
its keys after a short elapsed-time hold, including on failure or focus loss.

The native game decides whether reset is permitted (saving/communication can
inhibit it). A sent chord is not confirmation that the game reset. Save before
an encounter if you want to return to it. Resetting does not guarantee a shiny.
Default/remapped-key and duplicate-binding checks pass; in-game behavior in
Together remains to be verified with a trusted client and native UI access.
