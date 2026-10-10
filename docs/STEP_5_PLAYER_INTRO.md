# Step 5: Player Introduction

This step improves the first screen before the main menu.

## What Changed

The intro page now uses admin settings for:

- Game title
- Girlfriend name through `{name}`
- Intro message
- Intro subtitle
- Main menu welcome message
- YES button text
- NO button text
- Main colors
- Number of NO clicks before the NO button fades
- YES button growth rate

## Admin Page

Edit these values at:

```txt
/admin/settings
```

Recommended intro message:

```txt
Would you like to go on a date with me, {name}?
```

Recommended values:

```txt
NoButtonDisableAfterClicks: 8
YesButtonGrowthRate: 0.28
```

## Migration Required

This step adds new columns to `GameSettings`, so run:

```powershell
dotnet ef migrations add PlayerIntroSettings
dotnet ef database update
```

Then restart the app:

```powershell
dotnet run
```

## Notes

The NO button behavior stays playful:

- Clicking NO grows the YES button.
- Short messages appear after each NO click.
- After enough clicks, NO becomes disabled.

The rest of the game still works even if the user goes directly to `/Home/Menu`.
