# Step 28: No Button Randomizer

This patch keeps the existing invitation screen and adds a small playful No button mechanic.

## What Changed

- The existing admin setting is now shown as `No click limit`.
- Each successful No click still grows the original Yes button and updates the playful message.
- If the click limit has not been reached, the No button moves to a random valid viewport position outside the invitation card.
- Random positions are rejected when they overlap the invitation card, header, footer, music player, or invitation text/buttons.
- When the No click reaches the configured limit, the existing No button turns into another Yes button.
- The original Yes button stays exactly where it is.
- Both Yes buttons now send the player to the existing menu flow.

## Admin Usage

Open `Admin > Settings`, set `No click limit`, and save.

No database migration is needed because the app already had a stored No click count setting.

## Verification

- `node --check wwwroot/js/intro.js`
- `dotnet build --no-restore -o ..\work\build-check`

The build still shows the existing ImageSharp license warning, but there are no compile errors.
