# Step 24: Game Media Images

This patch lets existing uploaded images appear inside the player mini-games.

## Added

- Cake items can now choose an optional image from the media library.
- Perfect Date options can now choose an optional image from the media library.
- Quiz questions can now choose an optional question image from the media library.
- Player screens render those images when present and fall back to the existing text-only layout when no image is selected.

## How To Use

1. Upload images in `Admin > Media`.
2. Open one of these editors:
   - `Admin > Cake`
   - `Admin > Perfect Date`
   - `Admin > Quiz`
3. Choose an image in the optional image dropdown.
4. Save.
5. Open the player game screen to see the image.

## Notes

- No migration was needed because `MediaId` already existed on cake items, date options, and quiz questions.
- Existing content without images still works.
- Image uploads should be landscape or square for best results.
