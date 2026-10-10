# Step 25: Media Library Details

This patch makes the media library easier to maintain after files are uploaded.

## Added

- Edit uploaded media display name.
- Edit uploaded media category.
- Reassign uploaded media to a mini-game.
- Open uploaded files from the media card.
- Show file size and image dimensions on media cards.

## Notes

- No database migration was needed.
- Existing media files stay in place.
- Renaming a media item changes its display name only, not the stored file path.
