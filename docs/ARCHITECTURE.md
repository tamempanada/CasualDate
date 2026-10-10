# Casual Date Architecture

Casual Date is an ASP.NET Core MVC browser game with two sides:

- A player side for the romantic mini-game experience.
- A protected admin side for editing game content without changing source code.

The project should stay simple enough for a student developer to understand. That means MVC controllers, Razor views, EF Core models, and small JavaScript files instead of a separate frontend framework.

## Recommended Downloads

- Visual Studio Community 2026
  - Install the `ASP.NET and web development` workload.
- .NET 10 SDK
  - Recommended because it is the current long-term-support option for this project.
- MySQL Community Server
  - Use a stable Community Server release, not early-access builds.
- MySQL Workbench
  - Useful for checking tables and testing data.
- Git
  - Strongly recommended so each milestone can be saved.

## Recommended NuGet Packages

Add these when Step 2 and Step 3 begin:

```txt
Pomelo.EntityFrameworkCore.MySql
Microsoft.EntityFrameworkCore.Design
Microsoft.AspNetCore.Identity.EntityFrameworkCore
SixLabors.ImageSharp
```

Why:

- `Pomelo.EntityFrameworkCore.MySql` lets EF Core use MySQL.
- `Microsoft.EntityFrameworkCore.Design` enables migrations.
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore` provides admin users, roles, cookies, and password hashing.
- `SixLabors.ImageSharp` helps validate uploaded image dimensions and format.

## Folder Responsibilities

```txt
Controllers/
  Player-facing route logic.

Controllers/Admin/
  Admin editor routes. These will be protected with authentication in Step 3.

Data/
  ApplicationDbContext, database configuration, and seed data.

Models/
  Database entities such as GameSetting, MiniGame, UploadedMedia, QuizQuestion, and QuizAnswer.

ViewModels/
  Page-specific data passed from controllers to Razor views.

Views/Home/
  Intro and main menu pages.

Views/Game/
  Player mini-game pages.

Views/Admin/
  Admin editor pages.

wwwroot/css/
  Player and admin CSS.

wwwroot/js/
  Browser-only behavior such as the intro buttons, local progress, camera, drag/drop, and previews.

wwwroot/uploads/images/
  Uploaded image files.

wwwroot/uploads/audio/
  Uploaded audio files.
```

## Player Routes

```txt
/
/Home/Menu
/Game/Photobooth
/Game/Cake
/Game/PerfectDate
/Game/Quiz
/Game/Final
```

## Admin Routes

```txt
/admin
/admin/settings
/admin/media
/admin/games
/admin/cake
/admin/perfect-date
/admin/quiz
/admin/final-message
```

Admin controllers should eventually use:

```csharp
[Authorize(Roles = "Admin")]
```

The current `/admin` page is only a placeholder. It must not be considered secure until Step 3 adds Identity login and route protection.

## Data Flow

Player side:

```txt
Browser requests page
-> Controller loads game content from MySQL
-> Razor renders the page
-> JavaScript handles mini-game interaction
-> localStorage stores progress and unlocks
```

Admin side:

```txt
Admin logs in
-> ASP.NET Core Identity checks the password hash
-> Admin edits settings, media, questions, choices, or messages
-> Controller validates the submitted data
-> EF Core saves changes to MySQL
-> Player pages automatically use the updated content
```

## Upload Storage

Files should be saved here:

```txt
wwwroot/uploads/images/
wwwroot/uploads/audio/
```

MySQL should store metadata only:

```txt
OriginalFileName
StoredFileName
FilePath
ContentType
FileSize
Width
Height
Category
AssignedGame
UploadedAt
```

Do not store large images or audio files directly in MySQL.

## Current Starter State

This starter includes:

- MVC project shell.
- Custom intro page.
- Main menu page.
- Placeholder mini-game pages.
- Placeholder admin dashboard.
- Browser localStorage progress helper.
- Upload folders.
- Architecture and database documentation.

Database and authentication are intentionally not wired yet. They should be implemented as the next milestones so each part can be explained, tested, and debugged separately.
