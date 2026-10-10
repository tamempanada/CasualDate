# Step 18: Background Image

## What Was Added

- Admin settings can now choose an uploaded image as the player background.
- Player pages render the selected image as a full-page background.
- A soft overlay keeps text and cards readable.

## Files Updated

```txt
Controllers/Admin/SettingsController.cs
ViewModels/Admin/GameSettingEditViewModel.cs
Views/Admin/Settings/Index.cshtml
Views/Shared/_Layout.cshtml
wwwroot/css/site.css
docs/NEXT_STEPS.md
```

## Notes

This patch does not require a migration. The `GameSettings` model already had `BackgroundMediaId`.

Use `/admin/media` to upload an image first, then choose it in `/admin/settings`.

## Recommended Later Improvements

- Add per-page backgrounds.
- Add background position and brightness controls.
- Add mobile-specific background images.
