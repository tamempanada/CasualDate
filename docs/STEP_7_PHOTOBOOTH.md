# Step 7: Photobooth

This step adds the first playable mini-game.

## Added

- Camera preview using `navigator.mediaDevices.getUserMedia`.
- 3-second countdown.
- Flash effect.
- Four captured photos.
- Browser-generated photo strip on `<canvas>`.
- Download button for the finished strip.
- Placeholder mode if the camera is unavailable or permission is denied.
- Retake button.
- Complete Photobooth button that marks progress in `localStorage`.

## Important Browser Note

Camera access works on:

```txt
localhost
HTTPS
```

It usually does not work on plain `http://` from another device or non-local host.

## Testing

1. Open `/Game/Photobooth`.
2. Click `Start Camera`.
3. Allow camera permission.
4. Click `Take Photo` four times.
5. Click `Save Photo Strip`.
6. Click `Complete Photobooth`.
7. Return to `/Home/Menu` and confirm progress updated.

Fallback test:

1. Deny camera permission or click `Use Placeholder`.
2. Save the generated placeholder strip.
3. Complete the game.

## Next Improvements

Later admin customization can add:

- Number of photos.
- Strip title.
- Strip colors.
- Stickers/decorations.
- Uploaded frame/background images.
