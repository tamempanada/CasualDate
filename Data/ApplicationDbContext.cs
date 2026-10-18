using CasualDate.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CasualDate.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<GameSetting> GameSettings => Set<GameSetting>();

    public DbSet<MiniGame> MiniGames => Set<MiniGame>();

    public DbSet<UploadedMedia> UploadedMedia => Set<UploadedMedia>();

    public DbSet<Message> Messages => Set<Message>();

    public DbSet<ResultRange> ResultRanges => Set<ResultRange>();

    public DbSet<DateCategory> DateCategories => Set<DateCategory>();

    public DbSet<DateOption> DateOptions => Set<DateOption>();

    public DbSet<CakeItem> CakeItems => Set<CakeItem>();

    public DbSet<CakeRule> CakeRules => Set<CakeRule>();

    public DbSet<QuizQuestion> QuizQuestions => Set<QuizQuestion>();

    public DbSet<QuizAnswer> QuizAnswers => Set<QuizAnswer>();

    public DbSet<PlayerProfile> PlayerProfiles => Set<PlayerProfile>();

    public DbSet<TokenTransaction> TokenTransactions => Set<TokenTransaction>();

    public DbSet<TokenRewardSetting> TokenRewardSettings => Set<TokenRewardSetting>();

    public DbSet<PlayerGameCompletion> PlayerGameCompletions => Set<PlayerGameCompletion>();

    public DbSet<GameCompletionAttempt> GameCompletionAttempts => Set<GameCompletionAttempt>();

    public DbSet<GachaReward> GachaRewards => Set<GachaReward>();

    public DbSet<GachaPullBatch> GachaPullBatches => Set<GachaPullBatch>();

    public DbSet<GachaPull> GachaPulls => Set<GachaPull>();

    public DbSet<GachaCollectionItem> GachaCollectionItems => Set<GachaCollectionItem>();

    public DbSet<GachaPityState> GachaPityStates => Set<GachaPityState>();

    public DbSet<ScrapbookPage> ScrapbookPages => Set<ScrapbookPage>();

    public DbSet<HangmanProgress> HangmanProgresses => Set<HangmanProgress>();

    public DbSet<BlackjackRound> BlackjackRounds => Set<BlackjackRound>();

    public DbSet<LoveBlocksRun> LoveBlocksRuns => Set<LoveBlocksRun>();

    public DbSet<LoveBlocksPlayerRecord> LoveBlocksPlayerRecords => Set<LoveBlocksPlayerRecord>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        var seedTimestamp = new DateTime(2026, 10, 5, 0, 0, 0, DateTimeKind.Utc);

        builder.Entity<MiniGame>()
            .HasIndex(game => game.Slug)
            .IsUnique();

        builder.Entity<PlayerProfile>()
            .HasIndex(player => player.ApplicationUserId)
            .IsUnique();

        builder.Entity<PlayerProfile>()
            .HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<PlayerProfile>(player => player.ApplicationUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<TokenRewardSetting>()
            .HasIndex(setting => setting.EventKey)
            .IsUnique();

        builder.Entity<PlayerGameCompletion>()
            .HasIndex(completion => new { completion.PlayerProfileId, completion.EventKey })
            .IsUnique();

        builder.Entity<TokenTransaction>()
            .HasOne<PlayerProfile>()
            .WithMany()
            .HasForeignKey(transaction => transaction.PlayerProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<PlayerGameCompletion>()
            .HasOne<PlayerProfile>()
            .WithMany()
            .HasForeignKey(completion => completion.PlayerProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<GameCompletionAttempt>()
            .HasIndex(attempt => attempt.AttemptToken)
            .IsUnique();

        builder.Entity<GameCompletionAttempt>()
            .HasIndex(attempt => new { attempt.PlayerProfileId, attempt.GameSlug, attempt.Status });

        builder.Entity<GameCompletionAttempt>()
            .HasOne<PlayerProfile>()
            .WithMany()
            .HasForeignKey(attempt => attempt.PlayerProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<GachaReward>()
            .HasOne(reward => reward.Media)
            .WithMany()
            .HasForeignKey(reward => reward.MediaId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<GachaReward>()
            .HasIndex(reward => reward.BuiltInCosmeticId)
            .IsUnique();

        builder.Entity<GachaPull>()
            .HasIndex(pull => pull.PullToken)
            .IsUnique();

        builder.Entity<GachaPullBatch>()
            .HasIndex(batch => new { batch.PlayerProfileId, batch.RequestToken })
            .IsUnique();

        builder.Entity<GachaPullBatch>()
            .HasIndex(batch => batch.BatchToken)
            .IsUnique();

        builder.Entity<GachaPullBatch>()
            .HasOne<PlayerProfile>()
            .WithMany()
            .HasForeignKey(batch => batch.PlayerProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<GachaPull>()
            .HasOne<PlayerProfile>()
            .WithMany()
            .HasForeignKey(pull => pull.PlayerProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<GachaPull>()
            .HasOne(pull => pull.Batch)
            .WithMany()
            .HasForeignKey(pull => pull.GachaPullBatchId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<GachaPull>()
            .HasOne(pull => pull.Reward)
            .WithMany()
            .HasForeignKey(pull => pull.GachaRewardId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<GachaPityState>()
            .HasIndex(state => state.PlayerProfileId)
            .IsUnique();

        builder.Entity<GachaPityState>()
            .HasOne<PlayerProfile>()
            .WithMany()
            .HasForeignKey(state => state.PlayerProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<GachaCollectionItem>()
            .HasIndex(item => new { item.PlayerProfileId, item.GachaRewardId })
            .IsUnique();

        builder.Entity<GachaCollectionItem>()
            .HasOne<PlayerProfile>()
            .WithMany()
            .HasForeignKey(item => item.PlayerProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<GachaCollectionItem>()
            .HasOne(item => item.Reward)
            .WithMany()
            .HasForeignKey(item => item.GachaRewardId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ScrapbookPage>()
            .HasIndex(page => new { page.PlayerProfileId, page.PageNumber })
            .IsUnique();

        builder.Entity<ScrapbookPage>()
            .HasOne<PlayerProfile>()
            .WithMany()
            .HasForeignKey(page => page.PlayerProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<HangmanProgress>()
            .HasIndex(progress => progress.PlayerProfileId)
            .IsUnique();

        builder.Entity<HangmanProgress>()
            .HasOne<PlayerProfile>()
            .WithMany()
            .HasForeignKey(progress => progress.PlayerProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<BlackjackRound>()
            .HasIndex(round => round.RoundToken)
            .IsUnique();

        builder.Entity<BlackjackRound>()
            .HasIndex(round => new { round.PlayerProfileId, round.Status });

        builder.Entity<BlackjackRound>()
            .HasOne<PlayerProfile>()
            .WithMany()
            .HasForeignKey(round => round.PlayerProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<LoveBlocksRun>().HasIndex(run => run.RunToken).IsUnique();
        builder.Entity<LoveBlocksRun>().HasIndex(run => new { run.PlayerProfileId, run.IsOver });
        builder.Entity<LoveBlocksPlayerRecord>().HasIndex(record => record.PlayerProfileId).IsUnique();
        builder.Entity<LoveBlocksRun>().HasOne<PlayerProfile>().WithMany().HasForeignKey(run => run.PlayerProfileId).OnDelete(DeleteBehavior.Cascade);
        builder.Entity<LoveBlocksPlayerRecord>().HasOne<PlayerProfile>().WithMany().HasForeignKey(record => record.PlayerProfileId).OnDelete(DeleteBehavior.Cascade);

        builder.Entity<UploadedMedia>()
            .HasOne<MiniGame>()
            .WithMany()
            .HasForeignKey(media => media.MiniGameId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<UploadedMedia>()
            .HasOne<UploadedMedia>()
            .WithMany()
            .HasForeignKey(media => media.AlbumCoverMediaId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Message>()
            .HasOne<MiniGame>()
            .WithMany()
            .HasForeignKey(message => message.MiniGameId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Message>()
            .HasOne<UploadedMedia>()
            .WithMany()
            .HasForeignKey(message => message.MediaId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<ResultRange>()
            .HasOne<MiniGame>()
            .WithMany()
            .HasForeignKey(range => range.MiniGameId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ResultRange>()
            .HasOne<UploadedMedia>()
            .WithMany()
            .HasForeignKey(range => range.MediaId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<DateOption>()
            .HasOne<DateCategory>()
            .WithMany()
            .HasForeignKey(option => option.DateCategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<DateOption>()
            .HasOne<UploadedMedia>()
            .WithMany()
            .HasForeignKey(option => option.MediaId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<CakeItem>()
            .HasOne<UploadedMedia>()
            .WithMany()
            .HasForeignKey(item => item.MediaId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<CakeRule>()
            .HasOne<CakeItem>()
            .WithMany()
            .HasForeignKey(rule => rule.BaseItemId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<CakeRule>()
            .HasOne<CakeItem>()
            .WithMany()
            .HasForeignKey(rule => rule.FrostingItemId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<CakeRule>()
            .HasOne<CakeItem>()
            .WithMany()
            .HasForeignKey(rule => rule.ToppingItemId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<CakeRule>()
            .HasOne<CakeItem>()
            .WithMany()
            .HasForeignKey(rule => rule.DecorationItemId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<QuizQuestion>()
            .HasOne<UploadedMedia>()
            .WithMany()
            .HasForeignKey(question => question.MediaId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<QuizAnswer>()
            .HasOne<QuizQuestion>()
            .WithMany()
            .HasForeignKey(answer => answer.QuizQuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<QuizAnswer>()
            .HasOne<UploadedMedia>()
            .WithMany()
            .HasForeignKey(answer => answer.MediaId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<MiniGame>().HasData(
            new MiniGame
            {
                Id = 1,
                Slug = "photobooth",
                Name = "Photobooth",
                Description = "Take a sweet photo strip together.",
                DisplayOrder = 1,
                IconLabel = "Photo",
                CreatedAt = seedTimestamp,
                UpdatedAt = seedTimestamp
            },
            new MiniGame
            {
                Id = 2,
                Slug = "cake",
                Name = "Bake a Cake",
                Description = "Build a cake and get a funny love rating.",
                DisplayOrder = 2,
                IconLabel = "Cake",
                CreatedAt = seedTimestamp,
                UpdatedAt = seedTimestamp
            },
            new MiniGame
            {
                Id = 3,
                Slug = "perfect-date",
                Name = "Perfect Date",
                Description = "Plan the ideal date from cozy choices.",
                DisplayOrder = 3,
                IconLabel = "Date",
                CreatedAt = seedTimestamp,
                UpdatedAt = seedTimestamp
            },
            new MiniGame
            {
                Id = 4,
                Slug = "quiz",
                Name = "How Well Do You Know Me?",
                Description = "Answer personal questions and unlock a result.",
                DisplayOrder = 4,
                IconLabel = "Quiz",
                CreatedAt = seedTimestamp,
                UpdatedAt = seedTimestamp
            },
            new MiniGame
            {
                Id = 5,
                Slug = "gacha",
                Name = "Girlfriend Gacha",
                Description = "Pull collectible dolls, letters, and posters.",
                DisplayOrder = 5,
                IconLabel = "Gacha",
                CreatedAt = seedTimestamp,
                UpdatedAt = seedTimestamp
            },
            new MiniGame
            {
                Id = 6,
                Slug = "love-blocks",
                Name = "Love Blocks",
                Description = "Place the pieces, clear the board, and beat your high score.",
                DisplayOrder = 6,
                IconLabel = "Blocks",
                CreatedAt = seedTimestamp,
                UpdatedAt = seedTimestamp
            },
            new MiniGame
            {
                Id = 7,
                Slug = "hangman",
                Name = "Hangman",
                Description = "Guess words across adventure and endless modes.",
                DisplayOrder = 7,
                IconLabel = "Word",
                CreatedAt = seedTimestamp,
                UpdatedAt = seedTimestamp
            },
            new MiniGame
            {
                Id = 8,
                Slug = "blackjack",
                Name = "Blackjack",
                Description = "Play cards using your shared token wallet.",
                DisplayOrder = 8,
                IconLabel = "Cards",
                CreatedAt = seedTimestamp,
                UpdatedAt = seedTimestamp
            });

        builder.Entity<TokenRewardSetting>().HasData(
            new TokenRewardSetting
            {
                Id = 1,
                EventKey = "complete:photobooth",
                DisplayName = "Complete Photobooth",
                TokenAmount = 20,
                CreatedAt = seedTimestamp,
                UpdatedAt = seedTimestamp
            },
            new TokenRewardSetting
            {
                Id = 2,
                EventKey = "complete:cake",
                DisplayName = "Complete Bake a Cake",
                TokenAmount = 25,
                CreatedAt = seedTimestamp,
                UpdatedAt = seedTimestamp
            },
            new TokenRewardSetting
            {
                Id = 3,
                EventKey = "complete:perfect-date",
                DisplayName = "Complete Perfect Date",
                TokenAmount = 35,
                CreatedAt = seedTimestamp,
                UpdatedAt = seedTimestamp
            },
            new TokenRewardSetting
            {
                Id = 4,
                EventKey = "complete:quiz",
                DisplayName = "Complete Quiz",
                TokenAmount = 30,
                CreatedAt = seedTimestamp,
                UpdatedAt = seedTimestamp
            });

        builder.Entity<GameSetting>().HasData(new GameSetting
        {
            Id = 1,
            GameTitle = "Casual Date",
            GirlfriendName = "Ali",
            PlayerName = "",
            NicknamesJson = "[]",
            IntroMessage = "Would you like to go on a date with me, {name}?",
            IntroSubtitle = "A tiny date night made of photos, cake, questions, and favorite memories.",
            MainMenuMessage = "Welcome to our little date.",
            PhotoboothCompletionHeading = "Game Completed!",
            PhotoboothCompletionSubheading = "Your smile is as beautiful as always.",
            YesButtonText = "YES",
            NoButtonText = "NO",
            NoButtonDisableAfterClicks = 8,
            YesButtonGrowthRate = 0.28m,
            FinalTitle = "You completed our date",
            FinalMessage = "This final message will be customized from the admin dashboard.",
            PrimaryColor = "#d84f6f",
            SecondaryColor = "#7bb8a5",
            GachaBannerMediaId = null,
            CreatedAt = seedTimestamp,
            UpdatedAt = seedTimestamp
        });
    }
}
