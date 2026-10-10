# Step 6: Main Menu

This step turns the menu into a small game hub.

## Added

- Progress counter.
- Progress bar.
- Completed/not-completed labels on each game card.
- Locked final reward card.
- Final reward unlocks when every enabled game is completed.
- Reset Progress button.

## How Progress Works

Progress is stored in browser `localStorage` under:

```txt
casual-date-progress
```

This is intentionally local to the browser. The player does not need an account.

## Testing

1. Open `/Home/Menu`.
2. Open each placeholder mini-game.
3. Click `Mark complete for now`.
4. Go back to `/Home/Menu`.
5. Watch the progress bar fill.
6. When all games are complete, open the final reward.

## Notes

The final reward page currently uses the editable final title/message from `/admin/settings`.
