# Casual Date

Casual Date is a personalized browser-based mini-game collection built with ASP.NET Core MVC, Razor, JavaScript, EF Core, ASP.NET Core Identity, and MySQL. It includes Photobooth, Bake a Cake, Perfect Date, a personal quiz, Hangman, Blackjack, Girlfriend Gacha, and Love Blocks, plus protected administration, player profiles, persistent token wallets, and uploaded media.

## Requirements

- .NET 10 SDK
- MySQL 8.0 compatible server
- A persistent writable directory for uploaded media
- HTTPS and a persistent ASP.NET Core Data Protection key directory in production

## Run Locally

1. Clone the repository and enter the `CasualDate` directory.
2. Create a MySQL database and a dedicated database user with access only to that database.
3. Set the following environment variables for the process that runs the app:

```powershell
$env:ConnectionStrings__DefaultConnection = "server=localhost;port=3306;database=casual_date;user=casualdate_user;password=YOUR_LOCAL_PASSWORD;"
$env:SeedAdmin__Email = "you@example.com"
$env:SeedAdmin__Password = "Use-a-unique-strong-password"
```

4. Restore and run:

```powershell
dotnet restore --configfile .\NuGet.Config
dotnet run
```

The configured seed administrator is assigned both `Admin` and `SuperAdmin`. The seed values are applied at startup; keep them private and do not use development credentials in production. Local `appsettings.json` files are ignored by Git.

The app applies EF Core migrations at startup and also contains compatibility repairs for older databases. Review [the deployment checklist](docs/DEPLOYMENT_CHECKLIST.md) and back up an existing database before the first upgraded deployment.

## Project Documentation

- [Project history and feature progression](docs/PROJECT_HISTORY.md)
- [Production deployment checklist](docs/DEPLOYMENT_CHECKLIST.md)
- [Architecture](docs/ARCHITECTURE.md)
- [Database schema notes](docs/DATABASE_SCHEMA.md)
- [Original implementation milestone notes](docs/)

## Security and Data

- Never commit credentials, database exports, private gift media, or real player data.
- Treat MySQL as persistent state and `wwwroot/uploads/` as user data that must be backed up and stored on a persistent volume.
- Deploy only after checking the database migration state and confirming the application can connect with its production configuration.
