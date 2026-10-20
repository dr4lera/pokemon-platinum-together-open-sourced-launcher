# Optional community friend codes

Enable `"invitations": {"friendCodes": true}` in `Server/server.json`, validate
the config, then restart the backend. Existing configs default to disabled.
This extension is for our community servers and does not require official 0.1.10.
It does not replace the native direct-connect path.

Select the same server on both launchers' **Name** pages. Create your native
trainer there first. Open **Friend codes**, choose **Get my friend code**, then
share the six-character code or copyable `pt-private` invite. New parties use six
letters excluding ambiguous characters. An existing host party keeps its native
code, which can include digits. The recipient pastes the code/invite and chooses
**Join friend code**. Use the game's party controls to enter the host's world.

The invite contains the server origin, public identity pin and party code. It
contains no account credentials. Pasting an invite for a different server requires
selecting that server first; a mismatched identity pin is refused. No OS URL
handler is installed. A loopback address works only on the host computer; Internet
friends need the host's reachable configured address or tunnel.

Codes belong to a party on one server, allow four members, and stop working when
that party is gone or the server restarts. Joining never moves someone out of an
existing party, allocates a world, or reserves a relay seat. Native world entry is
a separate in-game step. Brief-disconnect and restart seat recovery are separate
unfinished work.

Backend revision 6 exposes `friend_code_protocol: 1` and the optional
`features.friend_codes` capability. The Node CLI accepts `friend-code-create` and
`friend-code-join` with an owned `--profile`, optional `--out`, and `--code` for
joining. These commands authenticate using the existing pinned-account protocol.
World-only deployments leave this invitation extension disabled.
