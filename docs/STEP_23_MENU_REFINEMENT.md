# Step 23: Menu Refinement

This patch tightens the player menu so it feels more like a playable game board and less like a large landing page.

## Changed

- Reduced the menu title scale with a dedicated menu header layout.
- Moved progress into a compact "Your date" panel beside the title on desktop.
- Added segmented progress dots that mirror completed and next-up state.
- Reworked game cards with clearer top, copy, and footer zones.
- Added card action labels:
  - `Start here` for the next unfinished game.
  - `Play` for locked-in normal entries.
  - `Play again` for completed entries.

## Notes

- No database changes were made.
- Existing localStorage progress is preserved.
- Existing controller routes and admin systems are unchanged.
