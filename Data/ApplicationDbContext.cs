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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        var seedTimestamp = new DateTime(2026, 10, 5, 0, 0, 0, DateTimeKind.Utc);

        builder.Entity<MiniGame>()
            .HasIndex(game => game.Slug)
            .IsUnique();

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
            CreatedAt = seedTimestamp,
            UpdatedAt = seedTimestamp
        });
    }
}
