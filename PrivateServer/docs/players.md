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
