# Step 8: Bake a Cake

This step adds the second playable mini-game.

## Added

- Four-step cake builder:
  - Base
  - Frosting
  - Topping
  - Decoration
- Visual cake preview.
- Database-backed cake items.
- Default cake items are inserted automatically if the table is empty.
- Score calculation.
- Funny result messages.
- Complete button that updates menu progress.

## Data Source

Cake options come from:

```txt
CakeItems
```

The current player version uses simple color-based visuals. Later, the admin editor can assign uploaded images to cake items.

## Testing

1. Open `/Game/Cake`.
2. Pick one option from each step.
3. Click `Finish Cake`.
4. Read the result.
5. Click `Complete Bake a Cake`.
6. Return to `/Home/Menu` and confirm progress updates.

## Next Improvements

- Admin Cake editor.
- Custom result ranges.
- Combination rules from `CakeRules`.
- Uploaded ingredient images.
- Custom cake text/initials.
