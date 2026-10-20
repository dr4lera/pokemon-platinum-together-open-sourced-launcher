# Owner mail and gifts

Run `MailSender/PlatinumMailSender.exe` on the owner PC. Its publication key is
protected with Windows CurrentUser DPAPI. The backend accepts publication only
through its local owner channel with a fresh RSA-signed challenge. Ordinary player
identity keys cannot send owner gifts. Copying the sender EXE alone does not copy
the owner's key. A compromised owner Windows account remains able to use it.

## Send

1. Connect the sender to your local backend and initialize its owner key if needed.
2. Choose recipients and enter the letter title/body.
3. Add supported native supply items, money, boots, lure or generated eggs; or
   import an exact supported `.pk4` Pokémon/egg.
4. Individual Poké Mart items use one of eight queue slots each. Supported native
   supply attachments can carry quantities through their own format.
5. Send. Keep the operation receipt if the UI reports a network failure. Retrying
   the same operation recovers it without publishing a duplicate.

## Receive a custom Pokémon, egg or Poké Mart item

The save-writing adapter accepts exact verified 0.1.5 and 0.1.7 builds. The
0.1.7 native layout, CRCs, selection rules and gift callbacks were audited
separately, with copied-save roundtrips. Ordinary native collection is still
pending; an inbox publication alone does not prove collection. See
[0.1.7 evidence and limits](native-017.md).

1. In the private launcher, open **Gift inbox**.
2. Close your private game normally.
3. Choose the gift and the intended private world's `save.bin`.
4. Apply the delivery. The launcher validates the format/checksums and backs up
   the save before writing. A full queue is refused without replacing existing gifts.
5. Reopen that world and collect from the native Poké Mart deliveryman.

Prepared deliveries are bound to the recipient identity and selected save. A claim
journal recovers interrupted writes/acknowledgements; do not delete it to retry a
gift. The launcher resolves pending acknowledgements before allowing Play.

Items and Pokémon must be supported by the pinned game build. Arbitrary code,
unsupported species/items, or invented attachment formats are not valid gifts.
Native parser/save fixtures and backend claim tests pass. Actual deliveryman
collection still needs a complete live verification.
