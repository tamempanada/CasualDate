using CasualDate.Data;
using CasualDate.Models;
using Microsoft.EntityFrameworkCore;

namespace CasualDate.Services;

public class HangmanService
{
    private readonly ApplicationDbContext _context;
    private readonly DatabaseSchemaService _schemaService;
    private readonly PlayerProfileService _playerProfileService;
    private readonly TokenWalletService _tokenWalletService;

    public HangmanService(
        ApplicationDbContext context,
        DatabaseSchemaService schemaService,
        PlayerProfileService playerProfileService,
        TokenWalletService tokenWalletService)
    {
        _context = context;
        _schemaService = schemaService;
        _playerProfileService = playerProfileService;
        _tokenWalletService = tokenWalletService;
    }

    public async Task<HangmanProgressDto> GetProgressAsync()
    {
        await EnsureSchemaAsync();
        return ToDto(await GetOrCreateProgressAsync());
    }

    public async Task<HangmanCompletionResult> CompleteAdventureLevelAsync(int level)
    {
        await EnsureSchemaAsync();
        var progress = await GetOrCreateProgressAsync();
        if (level is < 1 or > 15)
        {
            return new HangmanCompletionResult(
                ToDto(progress),
                new TokenAwardResult(false, 0, (await _playerProfileService.GetOrCreateAsync()).TokenBalance, "That Adventure level is not available."));
        }

        if (level > progress.AdventureUnlockedLevel)
        {
            return new HangmanCompletionResult(
                ToDto(progress),
                new TokenAwardResult(false, 0, (await _playerProfileService.GetOrCreateAsync()).TokenBalance, "Complete the earlier Adventure levels first."));
        }

        var safeLevel = level;
        var completed = ParseLevels(progress.AdventureCompletedLevels);
        var completedForFirstTime = completed.Add(safeLevel);

        progress.AdventureCompletedLevels = string.Join(",", completed);
        progress.AdventureUnlockedLevel = Math.Max(progress.AdventureUnlockedLevel, Math.Min(15, safeLevel + 1));
        progress.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var rewardKey = $"hangman:adventure:level:{safeLevel:D2}";
        if (completedForFirstTime)
        {
            await _tokenWalletService.EnsureRewardSettingAsync(rewardKey, $"Adventure level {safeLevel}", 2);
        }

        var reward = completedForFirstTime
            ? await _tokenWalletService.AwardCompletionAsync(rewardKey)
            : new TokenAwardResult(false, 0, (await _playerProfileService.GetOrCreateAsync()).TokenBalance, "Level was already completed.");

        return new HangmanCompletionResult(ToDto(progress), reward);
    }

    public async Task<HangmanCompletionResult> RecordEndlessWinAsync(int streak)
    {
        await EnsureSchemaAsync();
        var progress = await GetOrCreateProgressAsync();
        progress.EndlessTotalWins += 1;
        progress.EndlessBestStreak = Math.Max(progress.EndlessBestStreak, Math.Max(0, streak));
        progress.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        const string rewardSettingKey = "hangman:endless:word";
        await _tokenWalletService.EnsureRewardSettingAsync(rewardSettingKey, "Endless: each completed word", 1);
        var reward = await _tokenWalletService.AwardCompletionAsync(
            $"hangman:endless:word:{progress.EndlessTotalWins}",
            rewardSettingKey);

        return new HangmanCompletionResult(ToDto(progress), reward);
    }

    private async Task EnsureSchemaAsync()
    {
        if (!await _schemaService.TableExistsAsync("HangmanProgresses"))
        {
            throw new PlayerDataUnavailableException("Hangman progress tables are not available yet. Restart the app or run database migrations.");
        }
    }

    private async Task<HangmanProgress> GetOrCreateProgressAsync()
    {
        var player = await _playerProfileService.GetOrCreateAsync();
        var progress = await _context.HangmanProgresses
            .FirstOrDefaultAsync(item => item.PlayerProfileId == player.Id);

        if (progress is not null)
        {
            return progress;
        }

        progress = new HangmanProgress
        {
            PlayerProfileId = player.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.HangmanProgresses.Add(progress);
        await _context.SaveChangesAsync();
        return progress;
    }

    private static SortedSet<int> ParseLevels(string value)
    {
        return new SortedSet<int>(value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(item => int.TryParse(item, out var level) ? level : 0)
            .Where(level => level is >= 1 and <= 15));
    }

    private static HangmanProgressDto ToDto(HangmanProgress progress)
    {
        return new HangmanProgressDto(
            Math.Clamp(progress.AdventureUnlockedLevel, 1, 15),
            ParseLevels(progress.AdventureCompletedLevels).ToArray(),
            progress.EndlessTotalWins,
            progress.EndlessBestStreak);
    }
}

public sealed record HangmanProgressDto(
    int AdventureUnlockedLevel,
    int[] AdventureCompletedLevels,
    int EndlessTotalWins,
    int EndlessBestStreak);

public sealed record HangmanCompletionResult(HangmanProgressDto Progress, TokenAwardResult Reward);

public sealed class HangmanLevelRequest
{
    public int Level { get; set; }
}

public sealed class HangmanEndlessWinRequest
{
    public int Streak { get; set; }
}
