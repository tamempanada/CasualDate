# Step 17: Background Music

## What Was Added

- Admin settings can now choose uploaded audio as background music.
- Player pages show a small music play/pause control when music is selected.
- Music preference is saved in browser localStorage.
- Media uploads now allow audio files up to 20 MB.
- Audio files in the media library show native audio controls.

## Files Updated

```txt
Controllers/Admin/SettingsController.cs
Controllers/Admin/MediaController.cs
ViewModels/Admin/GameSettingEditViewModel.cs
Views/Admin/Settings/Index.cshtml
Views/Admin/Media/Index.cshtml
Views/Shared/_Layout.cshtml
wwwroot/js/site.js
wwwroot/css/site.css
docs/NEXT_STEPS.md
```

## Notes

This patch does not require a migration. The `GameSettings` model already had `MusicMediaId`.

Browsers block automatic audio playback, so the player must click **Play music** once. After that, the preference is remembered for the browser.

## Spotify Recommendation

Spotify search/playback is doable later, but it should be a separate integration patch because it needs Spotify developer credentials, OAuth setup, API calls, and playback restrictions. Uploaded audio is the safer foundation for now.
