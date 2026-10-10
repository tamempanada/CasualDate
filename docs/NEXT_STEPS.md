# Next Steps

## Step 2: Database

Add EF Core and MySQL packages:

```powershell
dotnet add package Pomelo.EntityFrameworkCore.MySql
dotnet add package Microsoft.EntityFrameworkCore.Design --version 9.0.0
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore --version 9.0.0
dotnet add package SixLabors.ImageSharp
```

Keep EF-related package major versions aligned. This starter uses Pomelo `9.0.0`, so the Microsoft EF packages are pinned to `9.0.0`.

If NuGet fails with an SSL/authentication error, run the same commands from Visual Studio's Package Manager Console or from a normal Windows terminal. The sandbox used while creating this starter could not complete package downloads from `nuget.org`.

Then create:

```txt
Data/ApplicationDbContext.cs
Models/ApplicationUser.cs
Models/GameSetting.cs
Models/MiniGame.cs
Models/UploadedMedia.cs
Models/Message.cs
Models/ResultRange.cs
Models/DateCategory.cs
Models/DateOption.cs
Models/CakeItem.cs
Models/CakeRule.cs
Models/QuizQuestion.cs
Models/QuizAnswer.cs
```

These model files have now been created as plain C# classes. The next database step is to install the packages and add `Data/ApplicationDbContext.cs`.

`Data/ApplicationDbContext.cs` has now been added. Follow `docs/STEP_2_DATABASE_SETUP.md` to run migrations locally.

You can also create the first MySQL schema manually with:

```txt
docs/mysql-step-2-schema.sql
```

## Step 3: Admin Authentication

After the database works:

- Configure Identity in `Program.cs`.
- Add admin login and logout.
- Seed one admin user.
- Protect `/admin` with `[Authorize(Roles = "Admin")]`.

## Step 4: Admin Content Management

Start with:

- General settings editor.
- Message editor.
- Image/audio upload.
- Media gallery.

Only after those work should the game-specific editors be added.

This step has now started. See:

```txt
docs/STEP_4_ADMIN_CONTENT.md
```

## Step 5: Player Introduction

This step has now started. See:

```txt
docs/STEP_5_PLAYER_INTRO.md
```

## Step 6: Main Menu

This step has now started. See:

```txt
docs/STEP_6_MAIN_MENU.md
```

## Step 7: Photobooth

This step has now started. See:

```txt
docs/STEP_7_PHOTOBOOTH.md
```

## Step 8: Bake a Cake

This step has now started. See:

```txt
docs/STEP_8_BAKE_A_CAKE.md
```

## Step 9: Perfect Date

This step has now started. See:

```txt
docs/STEP_9_PERFECT_DATE.md
```

## Step 10: Quiz

This step has now started. See:

```txt
docs/STEP_10_QUIZ.md
```

## Step 11: Final Reward Polish

This step has now started. See:

```txt
docs/STEP_11_FINAL_REWARD.md
```

## Step 12: Admin Quiz Editor

This step has now started. See:

```txt
docs/STEP_12_ADMIN_QUIZ.md
```

## Step 13: Admin Perfect Date Editor

This step has now started. See:

```txt
docs/STEP_13_ADMIN_PERFECT_DATE.md
```

## Step 14: Admin Cake Editor

This step has now started. See:

```txt
docs/STEP_14_ADMIN_CAKE.md
```

## Step 15: Admin Results Editor

This step has now started. See:

```txt
docs/STEP_15_ADMIN_RESULTS.md
```

## Step 16: SuperAdmin User Management

This step has now started. See:

```txt
docs/STEP_16_SUPERADMIN_USERS.md
```

## Step 17: Background Music

This step has now started. See:

```txt
docs/STEP_17_BACKGROUND_MUSIC.md
```

## Step 18: Background Image

This step has now started. See:

```txt
docs/STEP_18_BACKGROUND_IMAGE.md
```

## Step 19: Admin Preview Shortcuts

This step has now started. See:

```txt
docs/STEP_19_ADMIN_PREVIEW_SHORTCUTS.md
```

## Step 20: Completion Flow Polish

This step has now started. See:

```txt
docs/STEP_20_COMPLETION_FLOW.md
```

## Step 21: Menu Next Step Highlight

This step has now started. See:

```txt
docs/STEP_21_MENU_NEXT_STEP.md
```

## Step 22: Invitation And Hidden Admin Polish

This step has now started. See:

```txt
docs/STEP_22_INVITATION_HIDDEN_ADMIN.md
```

## Step 23: Menu Refinement

This step has now started. See:

```txt
docs/STEP_23_MENU_REFINEMENT.md
```

## Step 24: Game Media Images

This step has now started. See:

```txt
docs/STEP_24_GAME_MEDIA_IMAGES.md
```

## Step 25: Media Library Details

This step has now started. See:

```txt
docs/STEP_25_MEDIA_LIBRARY_DETAILS.md
```

## Step 26: Music Player Polish

This step has now started. See:

```txt
docs/STEP_26_MUSIC_PLAYER_POLISH.md
```

## Step 27: Album Music Player

This step has now started. See:

```txt
docs/STEP_27_ALBUM_MUSIC_PLAYER.md
```

## Step 28: No Button Randomizer

This step has now started. See:

```txt
docs/STEP_28_NO_BUTTON_RANDOMIZER.md
```

## Step 29: Personalized Completion Endings

This step has now started. See:

```txt
docs/STEP_29_PERSONALIZED_COMPLETION_ENDINGS.md
```

## Step 30: Result Modals And Music Minimize

This step has now started. See:

```txt
docs/STEP_30_RESULT_MODALS_AND_MUSIC_MINIMIZE.md
```

## Restore Note

In this sandbox, `dotnet restore` initially failed because access to the user-level NuGet config was blocked:

```txt
C:\Users\Mark raz\AppData\Roaming\NuGet\NuGet.Config
```

The project includes a local `NuGet.Config` so you can run:

```powershell
dotnet restore --configfile .\NuGet.Config
dotnet run
```

from inside the `CasualDate` folder.
