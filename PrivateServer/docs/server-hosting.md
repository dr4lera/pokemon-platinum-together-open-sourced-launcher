# Server hosting

## What runs on the host

The Node backend provides private identities, names, social messages, party records,
relay forwarding, mail, and SQLite save storage. Closing the launcher does not
automatically stop that backend. It currently does **not** run the native world
simulation on its own. Stored world data is persistence; it does not mean NPCs,
battles or scripts keep updating without a native host.

## Host on this PC

1. Keep `Launcher`, `Runtime`, and `Server` as siblings in the complete folder.
2. In **World hosting**, choose a free port; the launcher's default is 8788.
3. Leave remote-server mode off and start the local server.
4. Use the stop-server button to shut down cleanly. Close the private game first.

The default listener is `127.0.0.1`, accessible only on the host PC. The original
official service is not used by this backend. Do not launch a second server against
the same data directory, or copy a live SQLite database without its WAL state.

## LAN

Enable LAN connections, enter the reachable address, and stop/restart the backend
to apply it. Example: `http://192.168.1.20:8788`. The listener becomes `0.0.0.0`.
Allow the chosen application/port through your firewall only if needed for your
own LAN. The launcher does not change firewall or router settings automatically.
Other players use remote-server mode with that address.

## Internet without port forwarding

Choose **Start internet relay**. The bundled Cloudflare client opens an outbound
Quick Tunnel and the launcher advertises its HTTPS address. Share that address.
Keep the host PC, backend and tunnel running. The test suite verifies authenticated
binary routing, and a separate public tunnel test verified HTTPS and WebSocket
authentication. Native remote gameplay is still unverified.

Quick Tunnel hostnames change when recreated. They provide temporary development
hosting rather than a permanent service guarantee. A stable address requires a
separately configured named tunnel and a domain/account you control. Do not publish
Cloudflare tokens, tunnel credentials, private identity keys or owner mail keys.

## Headless backend from source

Install Node 24.12 or newer with `node:sqlite` support, then run from the toolset:

```powershell
node Server/main.mjs enroll --data ./Data --name HostName --url http://127.0.0.1:8788 --out ./Data/launcher-player.json
node Server/main.mjs serve --data ./Data --port 8788 --bind 127.0.0.1 --url http://127.0.0.1:8788
```

Enrollment is a one-time local operation. Existing identities must be preserved;
do not rerun enrollment into an existing profile path. The generated profile
contains a private key. Never paste it into chat or commit it.

For an explicitly configured LAN listener, replace `--bind` with `0.0.0.0` and
`--url` with the reachable LAN address. For public exposure, terminate TLS at your
own reverse proxy/tunnel, preserve WebSocket upgrades, and keep the backend's
owner/control endpoints restricted to loopback. Do not put a shared unauthenticated
HTTP forwarder in front of owner-only endpoints.

## Keeping a server available

Use a host that does not sleep, preserve its `Data` directory, back up while stopped,
and use a service supervisor that restarts the Node process after failure. The
backend can run on a separate machine, but the Windows launcher and native game
still need Windows. No supported unattended native-world worker is shipped yet.
