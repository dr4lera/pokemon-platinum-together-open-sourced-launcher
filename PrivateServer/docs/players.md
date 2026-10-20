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
with real local HTTP/WebSocket clients. Ordinary, unmodified 0.1.5 lab clients have completed the manual
relay entry path below, including after a backend restart. Automatic shortcut
handoff, remote gameplay, cross-map travel and recovery still need testing.

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

Use **Controls > Reset private game to title** while this launcher's private game
is running. Save before the encounter you intend to hunt and finish any active
save first. The launcher requests a normal window close. If the game asks
**Close the game?**, select **Yes** within 60 seconds to discard unsaved progress.
**No** keeps playing; the pending reset expires without terminating the game.
After it closes, the launcher starts the same private executable with its own
private application-data folder. Continue your last save to retry the encounter.
Co-op disconnects during reset. Resetting does not guarantee a shiny.

Only the executable at this launcher's private path is eligible. Unknown or
blocked builds and multiple matching processes are refused. Reset does not force
termination, inject keys, write Pokemon, restore saves or touch the original
installed game. The old DS button chord was unreliable in the PC client and is
no longer used. A local 0.1.5 test reopened the ordinary private client at title
and confirmed that the existing `save*.bin` hashes were unchanged. A separate
test kept the game alive after declining its close prompt.


## Manual relay joining in 0.1.5

Use the combined backend for co-op relay, separate from persistent-world and
Inverna deployment roles. Both players need private names on the same backend
and compatible native builds.

1. The host opens their world with **Host my world → Use Relay**.
2. The guest loads their own world, opens **Friends → Join code**, enters the
   host's code and selects **Join**. This joins the party.
3. The guest opens **Menu → Party → Join World**. This sends the native world
   request and enters the host's world over the relay.

The shortcut's “Joining your friend's world” message is insufficient on its own.
The ordinary 0.1.5 shortcut does not create a pending world request. Do not send
an invented request ID from the server to compensate: the guest rejects it.
An actual **Join World** request preserves native correlation and host admission
checks. This sequence passed twice locally, including a clean backend restart
without an experimental allocation hook or process-memory modification. It does
not establish Soul Link startup or independent cross-map travel.

The launcher writes its DPAPI-protected private sign-in to both native locations
inside `Client`: `Game/online-sign-in.dat` and
`AppData/PlatinumTogether/online-sign-in.dat`. It stages both updates, refuses a
running game/concurrent file change, rejects linked paths and retains the previous
protected file as `.previous`. It does not import the original game's account.
Treat these files and backups as private; never include them in a release.
