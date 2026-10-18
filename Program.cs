using CasualDate.Data;
using CasualDate.Models;
using CasualDate.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Missing DefaultConnection connection string.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 36))));

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/admin/login";
    options.AccessDeniedPath = "/admin/access-denied";
});

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<DatabaseSchemaService>();
builder.Services.AddScoped<PlayerProfileService>();
builder.Services.AddScoped<TokenWalletService>();
builder.Services.AddScoped<GameRewardService>();
builder.Services.AddScoped<GachaService>();
builder.Services.AddScoped<BuiltInCosmeticCatalogService>();
builder.Services.AddScoped<PlayerCosmeticService>();
builder.Services.AddScoped<ScrapbookService>();
builder.Services.AddScoped<HangmanService>();
builder.Services.AddScoped<BlackjackService>();
builder.Services.AddScoped<LoveBlocksService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

try
{
    using var migrationScope = app.Services.CreateScope();
    var db = migrationScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await EnsureGameSettingsBannerColumnAsync(db);
    try
    {
        await db.Database.MigrateAsync();
    }
    catch (Exception migrationException)
    {
        // Legacy databases can already contain a column from an interrupted migration.
        // Continue to the idempotent runtime repair below so current game features remain usable.
        app.Logger.LogWarning(migrationException, "Database migration did not finish; applying compatible runtime schema repairs.");
    }

    await EnsureRuntimeTablesAsync(db);
    await SeedData.InitializeAsync(app.Services);
}
catch (Exception ex)
{
    app.Logger.LogWarning(ex, "Database migration or admin seed skipped. Check the MySQL connection string before using database-backed features.");
}

app.Run();

static async Task EnsureRuntimeTablesAsync(ApplicationDbContext db)
{
    await db.Database.ExecuteSqlRawAsync("""
        CREATE TABLE IF NOT EXISTS `PlayerProfiles` (
            `Id` int NOT NULL AUTO_INCREMENT,
            `ApplicationUserId` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
            `DisplayName` varchar(120) CHARACTER SET utf8mb4 NOT NULL,
            `TokenBalance` int NOT NULL,
            `CreatedAt` datetime(6) NOT NULL,
            `UpdatedAt` datetime(6) NOT NULL,
            PRIMARY KEY (`Id`),
            UNIQUE KEY `IX_PlayerProfiles_ApplicationUserId` (`ApplicationUserId`),
            CONSTRAINT `FK_PlayerProfiles_AspNetUsers_ApplicationUserId`
                FOREIGN KEY (`ApplicationUserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
        ) CHARACTER SET=utf8mb4;
        """);

    await db.Database.ExecuteSqlRawAsync("""
        CREATE TABLE IF NOT EXISTS `TokenRewardSettings` (
            `Id` int NOT NULL AUTO_INCREMENT,
            `EventKey` varchar(120) CHARACTER SET utf8mb4 NOT NULL,
            `DisplayName` varchar(160) CHARACTER SET utf8mb4 NOT NULL,
            `TokenAmount` int NOT NULL,
            `IsEnabled` tinyint(1) NOT NULL,
            `CreatedAt` datetime(6) NOT NULL,
            `UpdatedAt` datetime(6) NOT NULL,
            PRIMARY KEY (`Id`),
            UNIQUE KEY `IX_TokenRewardSettings_EventKey` (`EventKey`)
        ) CHARACTER SET=utf8mb4;
        """);

    await db.Database.ExecuteSqlRawAsync("""
        INSERT IGNORE INTO `TokenRewardSettings` (`Id`, `CreatedAt`, `DisplayName`, `EventKey`, `IsEnabled`, `TokenAmount`, `UpdatedAt`) VALUES
            (1, '2026-10-05 00:00:00.000000', 'Complete Photobooth', 'complete:photobooth', TRUE, 20, '2026-10-05 00:00:00.000000'),
            (2, '2026-10-05 00:00:00.000000', 'Complete Bake a Cake', 'complete:cake', TRUE, 25, '2026-10-05 00:00:00.000000'),
            (3, '2026-10-05 00:00:00.000000', 'Complete Perfect Date', 'complete:perfect-date', TRUE, 35, '2026-10-05 00:00:00.000000'),
            (4, '2026-10-05 00:00:00.000000', 'Complete Quiz', 'complete:quiz', TRUE, 30, '2026-10-05 00:00:00.000000');
        """);

    await db.Database.ExecuteSqlRawAsync("""
        CREATE TABLE IF NOT EXISTS `PlayerGameCompletions` (
            `Id` int NOT NULL AUTO_INCREMENT,
            `PlayerProfileId` int NOT NULL,
            `EventKey` varchar(120) CHARACTER SET utf8mb4 NOT NULL,
            `TokenAmountAwarded` int NOT NULL,
            `CompletedAt` datetime(6) NOT NULL,
            PRIMARY KEY (`Id`),
            UNIQUE KEY `IX_PlayerGameCompletions_PlayerProfileId_EventKey` (`PlayerProfileId`, `EventKey`),
            CONSTRAINT `FK_PlayerGameCompletions_PlayerProfiles_PlayerProfileId`
                FOREIGN KEY (`PlayerProfileId`) REFERENCES `PlayerProfiles` (`Id`) ON DELETE CASCADE
        ) CHARACTER SET=utf8mb4;
        """);

    await db.Database.ExecuteSqlRawAsync("""
        CREATE TABLE IF NOT EXISTS `TokenTransactions` (
            `Id` int NOT NULL AUTO_INCREMENT,
            `PlayerProfileId` int NOT NULL,
            `Amount` int NOT NULL,
            `BalanceAfter` int NOT NULL,
            `TransactionType` varchar(80) CHARACTER SET utf8mb4 NOT NULL,
            `ReferenceKey` varchar(120) CHARACTER SET utf8mb4 NOT NULL,
            `Description` varchar(300) CHARACTER SET utf8mb4 NOT NULL,
            `CreatedAt` datetime(6) NOT NULL,
            PRIMARY KEY (`Id`),
            KEY `IX_TokenTransactions_PlayerProfileId` (`PlayerProfileId`),
            CONSTRAINT `FK_TokenTransactions_PlayerProfiles_PlayerProfileId`
                FOREIGN KEY (`PlayerProfileId`) REFERENCES `PlayerProfiles` (`Id`) ON DELETE CASCADE
        ) CHARACTER SET=utf8mb4;
        """);

    await db.Database.ExecuteSqlRawAsync("""
        CREATE TABLE IF NOT EXISTS `ScrapbookPages` (
            `Id` int NOT NULL AUTO_INCREMENT,
            `PlayerProfileId` int NOT NULL,
            `PageNumber` int NOT NULL,
            `Title` varchar(120) CHARACTER SET utf8mb4 NOT NULL,
            `DateLabel` varchar(80) CHARACTER SET utf8mb4 NOT NULL,
            `Body` longtext CHARACTER SET utf8mb4 NOT NULL,
            `PageDesign` varchar(40) CHARACTER SET utf8mb4 NOT NULL,
            `TextColor` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
            `TextAlign` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
            `FontSize` int NOT NULL,
            `CreatedAt` datetime(6) NOT NULL,
            `UpdatedAt` datetime(6) NOT NULL,
            PRIMARY KEY (`Id`),
            UNIQUE KEY `IX_ScrapbookPages_PlayerProfileId_PageNumber` (`PlayerProfileId`, `PageNumber`),
            CONSTRAINT `FK_ScrapbookPages_PlayerProfiles_PlayerProfileId`
                FOREIGN KEY (`PlayerProfileId`) REFERENCES `PlayerProfiles` (`Id`) ON DELETE CASCADE
        ) CHARACTER SET=utf8mb4;
        """);

    await db.Database.ExecuteSqlRawAsync("""
        CREATE TABLE IF NOT EXISTS `HangmanProgresses` (
            `Id` int NOT NULL AUTO_INCREMENT,
            `PlayerProfileId` int NOT NULL,
            `AdventureUnlockedLevel` int NOT NULL,
            `AdventureCompletedLevels` varchar(160) CHARACTER SET utf8mb4 NOT NULL,
            `EndlessTotalWins` int NOT NULL,
            `EndlessBestStreak` int NOT NULL,
            `CreatedAt` datetime(6) NOT NULL,
            `UpdatedAt` datetime(6) NOT NULL,
            PRIMARY KEY (`Id`),
            UNIQUE KEY `IX_HangmanProgresses_PlayerProfileId` (`PlayerProfileId`),
            CONSTRAINT `FK_HangmanProgresses_PlayerProfiles_PlayerProfileId`
                FOREIGN KEY (`PlayerProfileId`) REFERENCES `PlayerProfiles` (`Id`) ON DELETE CASCADE
        ) CHARACTER SET=utf8mb4;
        """);

    await db.Database.ExecuteSqlRawAsync("""
        CREATE TABLE IF NOT EXISTS `GachaPullBatches` (
            `Id` int NOT NULL AUTO_INCREMENT,
            `PlayerProfileId` int NOT NULL,
            `RequestToken` varchar(80) CHARACTER SET utf8mb4 NOT NULL,
            `BatchToken` varchar(80) CHARACTER SET utf8mb4 NOT NULL,
            `PullCount` int NOT NULL,
            `TokenCost` int NOT NULL,
            `CreatedAt` datetime(6) NOT NULL,
            PRIMARY KEY (`Id`),
            UNIQUE KEY `IX_GachaPullBatches_BatchToken` (`BatchToken`),
            UNIQUE KEY `IX_GachaPullBatches_PlayerProfileId_RequestToken` (`PlayerProfileId`, `RequestToken`),
            CONSTRAINT `FK_GachaPullBatches_PlayerProfiles_PlayerProfileId`
                FOREIGN KEY (`PlayerProfileId`) REFERENCES `PlayerProfiles` (`Id`) ON DELETE CASCADE
        ) CHARACTER SET=utf8mb4;
        """);

    await db.Database.ExecuteSqlRawAsync("""
        CREATE TABLE IF NOT EXISTS `GachaPityStates` (
            `Id` int NOT NULL AUTO_INCREMENT,
            `PlayerProfileId` int NOT NULL,
            `LegendaryPityCount` int NOT NULL,
            `CreatedAt` datetime(6) NOT NULL,
            `UpdatedAt` datetime(6) NOT NULL,
            PRIMARY KEY (`Id`),
            UNIQUE KEY `IX_GachaPityStates_PlayerProfileId` (`PlayerProfileId`),
            CONSTRAINT `FK_GachaPityStates_PlayerProfiles_PlayerProfileId`
                FOREIGN KEY (`PlayerProfileId`) REFERENCES `PlayerProfiles` (`Id`) ON DELETE CASCADE
        ) CHARACTER SET=utf8mb4;
        """);

    await EnsureColumnAsync(db, "GachaPulls", "GachaPullBatchId", "int NULL");
    await EnsureColumnAsync(db, "GachaPulls", "IsPityGuaranteed", "tinyint(1) NOT NULL DEFAULT FALSE");
    await EnsureColumnAsync(db, "GachaPulls", "IsTenPullGuaranteed", "tinyint(1) NOT NULL DEFAULT FALSE");
    await EnsureColumnAsync(db, "GachaPulls", "RefundAmount", "int NOT NULL DEFAULT 0");
    await EnsureColumnAsync(db, "GachaRewards", "BuiltInCosmeticId", "varchar(80) CHARACTER SET utf8mb4 NULL");
    await EnsureColumnAsync(db, "PlayerProfiles", "EquippedThemeId", "varchar(80) CHARACTER SET utf8mb4 NOT NULL DEFAULT 'default-theme'");
    await EnsureColumnAsync(db, "PlayerProfiles", "EquippedMusicSkinId", "varchar(80) CHARACTER SET utf8mb4 NOT NULL DEFAULT 'default-music'");
    await EnsureColumnAsync(db, "PlayerProfiles", "EquippedEffectId", "varchar(80) CHARACTER SET utf8mb4 NOT NULL DEFAULT 'none-effect'");
    await EnsureColumnAsync(db, "PlayerProfiles", "EquippedCompanionRewardId", "int NULL");
    await EnsureColumnAsync(db, "PlayerProfiles", "SelectedMusicMediaId", "int NULL");
    await EnsureGameSettingsBannerColumnAsync(db);

    await db.Database.ExecuteSqlRawAsync("""
        CREATE TABLE IF NOT EXISTS `GameCompletionAttempts` (
            `Id` int NOT NULL AUTO_INCREMENT,
            `PlayerProfileId` int NOT NULL,
            `GameSlug` varchar(40) CHARACTER SET utf8mb4 NOT NULL,
            `AttemptToken` varchar(80) CHARACTER SET utf8mb4 NOT NULL,
            `Status` varchar(40) CHARACTER SET utf8mb4 NOT NULL,
            `Score` int NULL,
            `ResultRangeId` int NULL,
            `CaptureCount` int NOT NULL DEFAULT 0,
            `CreatedAt` datetime(6) NOT NULL,
            `CompletedAt` datetime(6) NULL,
            PRIMARY KEY (`Id`),
            UNIQUE KEY `IX_GameCompletionAttempts_AttemptToken` (`AttemptToken`),
            KEY `IX_GameCompletionAttempts_PlayerProfileId_GameSlug_Status` (`PlayerProfileId`, `GameSlug`, `Status`),
            CONSTRAINT `FK_GameCompletionAttempts_PlayerProfiles_PlayerProfileId`
                FOREIGN KEY (`PlayerProfileId`) REFERENCES `PlayerProfiles` (`Id`) ON DELETE CASCADE
        ) CHARACTER SET=utf8mb4;
        """);

    await EnsureColumnAsync(db, "GameCompletionAttempts", "CaptureCount", "int NOT NULL DEFAULT 0");
    await EnsureColumnAsync(db, "TokenRewardSettings", "TriggerInterval", "int NOT NULL DEFAULT 1");

    await db.Database.ExecuteSqlRawAsync("""
        CREATE TABLE IF NOT EXISTS `BlackjackRounds` (
            `Id` int NOT NULL AUTO_INCREMENT,
            `PlayerProfileId` int NOT NULL,
            `RoundToken` varchar(80) CHARACTER SET utf8mb4 NOT NULL,
            `BetAmount` int NOT NULL,
            `Status` varchar(40) CHARACTER SET utf8mb4 NOT NULL,
            `PayoutAmount` int NOT NULL,
            `DeckJson` longtext CHARACTER SET utf8mb4 NOT NULL,
            `PlayerHandJson` longtext CHARACTER SET utf8mb4 NOT NULL,
            `DealerHandJson` longtext CHARACTER SET utf8mb4 NOT NULL,
            `CreatedAt` datetime(6) NOT NULL,
            `UpdatedAt` datetime(6) NOT NULL,
            `ResolvedAt` datetime(6) NULL,
            PRIMARY KEY (`Id`),
            UNIQUE KEY `IX_BlackjackRounds_RoundToken` (`RoundToken`),
            KEY `IX_BlackjackRounds_PlayerProfileId_Status` (`PlayerProfileId`, `Status`),
            CONSTRAINT `FK_BlackjackRounds_PlayerProfiles_PlayerProfileId`
                FOREIGN KEY (`PlayerProfileId`) REFERENCES `PlayerProfiles` (`Id`) ON DELETE CASCADE
        ) CHARACTER SET=utf8mb4;
        """);

    await db.Database.ExecuteSqlRawAsync("""
        CREATE TABLE IF NOT EXISTS `LoveBlocksRuns` (
            `Id` int NOT NULL AUTO_INCREMENT,
            `PlayerProfileId` int NOT NULL,
            `RunToken` varchar(80) CHARACTER SET utf8mb4 NOT NULL,
            `LastRequestId` varchar(80) CHARACTER SET utf8mb4 NOT NULL,
            `Board` varchar(64) CHARACTER SET utf8mb4 NOT NULL,
            `PiecesJson` longtext CHARACTER SET utf8mb4 NOT NULL,
            `ProcessedRequestIdsJson` longtext CHARACTER SET utf8mb4 NOT NULL,
            `Score` int NOT NULL,
            `LinesCleared` int NOT NULL,
            `MovesMade` int NOT NULL,
            `TokensEarned` int NOT NULL,
            `ScoreMilestonesAwarded` int NOT NULL,
            `LineMilestonesAwarded` int NOT NULL,
            `PersonalBestRewarded` tinyint(1) NOT NULL,
            `IsOver` tinyint(1) NOT NULL,
            `ScoreRewardAmount` int NOT NULL,
            `ScoreRewardInterval` int NOT NULL,
            `ScoreRewardEnabled` tinyint(1) NOT NULL,
            `LineRewardAmount` int NOT NULL,
            `LineRewardInterval` int NOT NULL,
            `LineRewardEnabled` tinyint(1) NOT NULL,
            `BestRewardAmount` int NOT NULL,
            `BestRewardEnabled` tinyint(1) NOT NULL,
            `CreatedAt` datetime(6) NOT NULL,
            `UpdatedAt` datetime(6) NOT NULL,
            PRIMARY KEY (`Id`),
            UNIQUE KEY `IX_LoveBlocksRuns_RunToken` (`RunToken`),
            KEY `IX_LoveBlocksRuns_PlayerProfileId_IsOver` (`PlayerProfileId`, `IsOver`),
            CONSTRAINT `FK_LoveBlocksRuns_PlayerProfiles_PlayerProfileId`
                FOREIGN KEY (`PlayerProfileId`) REFERENCES `PlayerProfiles` (`Id`) ON DELETE CASCADE
        ) CHARACTER SET=utf8mb4;
        """);

    await db.Database.ExecuteSqlRawAsync("""
        CREATE TABLE IF NOT EXISTS `LoveBlocksPlayerRecords` (
            `Id` int NOT NULL AUTO_INCREMENT,
            `PlayerProfileId` int NOT NULL,
            `BestScore` int NOT NULL,
            `UpdatedAt` datetime(6) NOT NULL,
            PRIMARY KEY (`Id`),
            UNIQUE KEY `IX_LoveBlocksPlayerRecords_PlayerProfileId` (`PlayerProfileId`),
            CONSTRAINT `FK_LoveBlocksPlayerRecords_PlayerProfiles_PlayerProfileId`
                FOREIGN KEY (`PlayerProfileId`) REFERENCES `PlayerProfiles` (`Id`) ON DELETE CASCADE
        ) CHARACTER SET=utf8mb4;
        """);

    await db.Database.ExecuteSqlRawAsync("""
        UPDATE `MiniGames`
        SET `Slug` = 'love-blocks', `Name` = 'Love Blocks',
            `Description` = 'Place the pieces, clear the board, and beat your high score.',
            `IconLabel` = 'Blocks', `UpdatedAt` = UTC_TIMESTAMP(6)
        WHERE `Slug` = 'scrapbook';
        """);
}

static async Task EnsureGameSettingsBannerColumnAsync(ApplicationDbContext db)
{
    var connection = db.Database.GetDbConnection();
    var shouldClose = connection.State == ConnectionState.Closed;

    if (shouldClose)
    {
        await connection.OpenAsync();
    }

    try
    {
        await using (var tableCommand = connection.CreateCommand())
        {
            tableCommand.CommandText = """
                SELECT COUNT(*)
                FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_SCHEMA = DATABASE()
                    AND TABLE_NAME = 'GameSettings';
                """;

            var tableResult = await tableCommand.ExecuteScalarAsync();
            if (Convert.ToInt32(tableResult) == 0)
            {
                return;
            }
        }

        await using (var columnCommand = connection.CreateCommand())
        {
            columnCommand.CommandText = """
                SELECT COUNT(*)
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_SCHEMA = DATABASE()
                    AND TABLE_NAME = 'GameSettings'
                    AND COLUMN_NAME = 'GachaBannerMediaId';
                """;

            var columnResult = await columnCommand.ExecuteScalarAsync();
            if (Convert.ToInt32(columnResult) > 0)
            {
                return;
            }
        }

        await using var alterCommand = connection.CreateCommand();
        alterCommand.CommandText = "ALTER TABLE `GameSettings` ADD COLUMN `GachaBannerMediaId` int NULL;";
        await alterCommand.ExecuteNonQueryAsync();
    }
    finally
    {
        if (shouldClose)
        {
            await connection.CloseAsync();
        }
    }
}

static async Task EnsureColumnAsync(ApplicationDbContext db, string tableName, string columnName, string columnDefinition)
{
    var isSupportedColumn = (tableName, columnName) switch
    {
        ("GachaPulls", "GachaPullBatchId") => true,
        ("GachaPulls", "IsPityGuaranteed") => true,
        ("GachaPulls", "IsTenPullGuaranteed") => true,
        ("GachaPulls", "RefundAmount") => true,
        ("GachaRewards", "BuiltInCosmeticId") => true,
        ("PlayerProfiles", "EquippedThemeId") => true,
        ("PlayerProfiles", "EquippedMusicSkinId") => true,
        ("PlayerProfiles", "EquippedEffectId") => true,
        ("PlayerProfiles", "EquippedCompanionRewardId") => true,
        ("PlayerProfiles", "SelectedMusicMediaId") => true,
        ("GameCompletionAttempts", "CaptureCount") => true,
        ("TokenRewardSettings", "TriggerInterval") => true,
        _ => false
    };
    if (!isSupportedColumn)
    {
        throw new InvalidOperationException("Unsupported runtime schema repair.");
    }

    var connection = db.Database.GetDbConnection();
    var shouldClose = connection.State == ConnectionState.Closed;

    if (shouldClose)
    {
        await connection.OpenAsync();
    }

    try
    {
        await using var columnCommand = connection.CreateCommand();
        columnCommand.CommandText = """
            SELECT COUNT(*)
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_SCHEMA = DATABASE()
                AND TABLE_NAME = @tableName
                AND COLUMN_NAME = @columnName;
            """;

        var tableParameter = columnCommand.CreateParameter();
        tableParameter.ParameterName = "@tableName";
        tableParameter.Value = tableName;
        columnCommand.Parameters.Add(tableParameter);

        var columnParameter = columnCommand.CreateParameter();
        columnParameter.ParameterName = "@columnName";
        columnParameter.Value = columnName;
        columnCommand.Parameters.Add(columnParameter);

        var exists = Convert.ToInt32(await columnCommand.ExecuteScalarAsync()) > 0;
        if (exists)
        {
            return;
        }

#pragma warning disable EF1002 // Identifiers are restricted to the fixed allowlist above.
        await db.Database.ExecuteSqlRawAsync($"ALTER TABLE `{tableName}` ADD COLUMN `{columnName}` {columnDefinition};");
#pragma warning restore EF1002
    }
    finally
    {
        if (shouldClose)
        {
            await connection.CloseAsync();
        }
    }
}
