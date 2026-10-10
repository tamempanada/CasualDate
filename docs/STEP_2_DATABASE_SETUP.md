# Step 2: Database Setup

This step connects the C# project to MySQL through Entity Framework Core.

## Important Port Note

Do not use port `5500` for this project.

Port `5500` is usually VS Code Live Server. It only serves static files, so it will show folders/files instead of running ASP.NET Core.

Use:

```powershell
dotnet run
```

Then open the URL printed by ASP.NET Core, for example:

```txt
http://localhost:5188
```

## Package Version Recommendation

This project currently uses:

```txt
Pomelo.EntityFrameworkCore.MySql 9.0.0
Microsoft.EntityFrameworkCore.Design 9.0.0
Microsoft.AspNetCore.Identity.EntityFrameworkCore 9.0.0
SixLabors.ImageSharp 4.1.2
```

The EF-related packages should stay on the same major version. Mixing EF Core 10 packages with Pomelo 9 can cause compatibility problems.

If your local install added EF 10 packages, run:

```powershell
dotnet add package Microsoft.EntityFrameworkCore.Design --version 9.0.0
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore --version 9.0.0
dotnet restore
```

## MySQL Connection String

Edit:

```txt
appsettings.Development.json
```

Replace:

```json
"DefaultConnection": "server=localhost;port=3306;database=casual_date;user=root;password=CHANGE_ME;"
```

with your real local MySQL username and password.

Example:

```json
"DefaultConnection": "server=localhost;port=3306;database=casual_date;user=root;password=your_mysql_password;"
```

Do not put this password in JavaScript, Razor views, or frontend files.

## Create the Database with EF Migrations

From inside the project folder:

```powershell
cd "C:\Users\Mark raz\Documents\Codex\2026-10-05\do\CasualDate"
dotnet restore
dotnet ef migrations add InitialCreate
dotnet ef database update
```

If `dotnet ef` is not available, install the EF tool:

```powershell
dotnet tool install --global dotnet-ef --version 9.0.0
```

If it is already installed but old:

```powershell
dotnet tool update --global dotnet-ef --version 9.0.0
```

## Alternative: Manual SQL

If migrations give you trouble while learning, you can create the database manually using:

```txt
docs/mysql-step-2-schema.sql
```

Open it in MySQL Workbench and run the whole file.

Migrations are still recommended because they keep the C# models and database changes connected.

## What Was Added in This Step

New files:

```txt
Models/ApplicationUser.cs
Data/ApplicationDbContext.cs
docs/STEP_2_DATABASE_SETUP.md
```

Updated files:

```txt
Program.cs
appsettings.json
appsettings.Development.json
CasualDate.csproj
```

The app now knows:

- Which tables should exist.
- How the main tables relate.
- Which starter mini-games should be seeded.
- Where the MySQL connection string belongs.

## What Comes Next

Step 3 is Admin Authentication:

- Add admin login/logout.
- Seed one admin account.
- Protect `/admin`.
- Add a basic admin layout.

The Step 3 scaffolding has now been started. Before running the app, update this section in `appsettings.Development.json`:

```json
"SeedAdmin": {
  "Email": "your-email@example.com",
  "Password": "YourPassword123"
}
```

The seed password must not stay as `CHANGE_ME`. If it stays unchanged, the app skips admin-user creation.
