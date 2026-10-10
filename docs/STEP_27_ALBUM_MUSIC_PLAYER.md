# Step 27: Album Music Player

This patch upgrades the player background music from one fixed audio file into a compact album-style music player.

## What Changed

- Audio uploads can now have an optional album cover image.
- The player shows a square album cover beside the track title, time, seek bar, and controls.
- The player can switch tracks with previous and next buttons.
- The player has a loop toggle.
- The player has a Pick music panel that lists uploaded audio tracks.
- Tracks can be added to a small browser-side queue.
- Playback position is saved per track in the browser.

## Admin Usage

1. Upload the square album image in `Admin > Media`.
2. Upload the audio track in `Admin > Media`.
3. Open `Edit` on the audio file.
4. Pick the uploaded image in `Album cover`.
5. Save.

The cover image should be a classic square image. A 1:1 upload works best because the player displays it as an album tile.

## Database

This step adds `AlbumCoverMediaId` to `UploadedMedia`.

Run this once after updating the project:

```powershell
dotnet ef database update
```

If the app is already running and locks the normal executable during build, stop the running app first, then run the command again.

## Verification

- `node --check wwwroot/js/site.js`
- `dotnet build --no-restore -o ..\work\build-check`

The build still shows the existing ImageSharp license warning, but there are no compile errors.
