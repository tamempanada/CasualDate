# Step 22: Invitation And Hidden Admin Polish

## What Was Added

- Opening invitation is now a compact centered card instead of a giant hero.
- YES/NO buttons start visible beside each other.
- YES grows within sensible limits.
- NO remains visible and clickable, but becomes smaller and shifts slightly.
- YES no longer uses transform scaling that can cover the question or NO button.
- Background music is hidden on the startup invitation page.
- Visible Admin navigation link was removed from the player layout.
- Secret admin shortcut added:

```txt
Ctrl + Alt + A
```

The shortcut only navigates to `/admin/login`; authentication is still required.

## Files Updated

```txt
Views/Home/Index.cshtml
Views/Shared/_Layout.cshtml
wwwroot/js/intro.js
wwwroot/js/site.js
wwwroot/css/site.css
docs/NEXT_STEPS.md
```
