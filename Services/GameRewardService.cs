using CasualDate.Data;
using CasualDate.Models;
using Microsoft.EntityFrameworkCore;

namespace CasualDate.Services;

public class GameRewardService
{
    private readonly ApplicationDbContext _context;
    private readonly DatabaseSchemaService _schemaService;
    private readonly PlayerProfileService _playerProfileService;
    private readonly TokenWalletService _tokenWalletService;

    public GameRewardService(
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

    public async Task<GameAttemptStartResult> StartAttemptAsync(string gameSlug)
    {
        await EnsureSchemaAsync();
        if (gameSlug is not ("cake" or "perfect-date" or "photobooth"))
        {
            return GameAttemptStartResult.Invalid("This game does not use a reward attempt.");
        }

        var player = await _playerProfileService.GetOrCreateAsync();
        var attempt = new GameCompletionAttempt
        {
            PlayerProfileId = player.Id,
            GameSlug = gameSlug,
            AttemptToken = Guid.NewGuid().ToString("N")
        };
        _context.GameCompletionAttempts.Add(attempt);
        await _context.SaveChangesAsync();
        return GameAttemptStartResult.Success(attempt.AttemptToken);
    }

    public async Task<GameRewardCompletionResult> CompleteCakeAsync(CakeCompletionRequest request)
    {
        var selectedItemIds = new[]
        {
            request.BaseId,
            request.FrostingId,
            request.ToppingId,
            request.DecorationId
        };

        if (selectedItemIds.Any(id => id <= 0) || selectedItemIds.Distinct().Count() != selectedItemIds.Length)
        {
            return GameRewardCompletionResult.Invalid("Choose one available item from every cake category.");
        }

        // Cake configuration is intentionally small. Resolve it first, then validate the
        // player's four IDs in memory to avoid provider-specific collection parameter SQL.
        var enabledCakeItems = await _context.CakeItems
            .Where(item => item.IsEnabled)
            .ToListAsync();
        var selectedItems = enabledCakeItems
            .Where(item => selectedItemIds.Contains(item.Id))
            .ToList();

        var expectedTypes = new[] { CakeItemType.Base, CakeItemType.Frosting, CakeItemType.Topping, CakeItemType.Decoration };
        if (selectedItems.Count != 4 || expectedTypes.Any(type => selectedItems.Count(item => item.ItemType == type) != 1))
        {
            return GameRewardCompletionResult.Invalid("Choose one available item from every cake category.");
        }

        var score = Math.Min(100, selectedItems.Sum(item => item.Score));
        return await CompleteAttemptAsync("cake", request.AttemptToken, score);
    }

    public async Task<GameRewardCompletionResult> CompletePerfectDateAsync(PerfectDateCompletionRequest request)
    {
        var optionIds = request.OptionIds?.ToList() ?? [];
        if (optionIds.Count == 0 || optionIds.Distinct().Count() != optionIds.Count)
        {
            return GameRewardCompletionResult.Invalid("Choose one available option from every date category.");
        }

        var options = await _context.DateOptions
            .Where(option => option.IsEnabled && optionIds.Contains(option.Id))
            .ToListAsync();

        if (options.Count != optionIds.Count || options.Select(option => option.DateCategoryId).Distinct().Count() != options.Count)
        {
            return GameRewardCompletionResult.Invalid("Choose one available option from every date category.");
        }

        var enabledCategoryIds = await _context.DateCategories
            .Where(category => category.IsEnabled)
            .Select(category => category.Id)
            .ToListAsync();
        if (options.Count != enabledCategoryIds.Count || options.Any(option => !enabledCategoryIds.Contains(option.DateCategoryId)))
        {
            return GameRewardCompletionResult.Invalid("Choose one available option from every date category.");
        }

        return await CompleteAttemptAsync("perfect-date", request.AttemptToken, options.Sum(option => option.Score));
    }

    public async Task<PhotoboothCaptureResult> RegisterPhotoboothCaptureAsync(string attemptToken)
    {
        await EnsureSchemaAsync();
        if (string.IsNullOrWhiteSpace(attemptToken))
        {
            return PhotoboothCaptureResult.Invalid("Start the camera before taking photos.");
        }

        var player = await _playerProfileService.GetOrCreateAsync();
        var playerId = player.Id;
        var activeAttemptToken = attemptToken.Trim();
        await using var transaction = await _context.Database.BeginTransactionAsync();
        var attempt = await _context.GameCompletionAttempts.FirstOrDefaultAsync(item => item.AttemptToken == activeAttemptToken
            && item.PlayerProfileId == playerId
            && item.GameSlug == "photobooth");

        if (attempt is null || attempt.Status != GameCompletionAttemptStatus.Started)
        {
            return PhotoboothCaptureResult.Invalid("This camera session is no longer active. Start the camera again.");
        }

        if (attempt.CaptureCount < 4)
        {
            attempt.CaptureCount += 1;
            await _context.SaveChangesAsync();
        }

        await transaction.CommitAsync();
        return PhotoboothCaptureResult.Success(attempt.CaptureCount);
    }

    public async Task<GameRewardCompletionResult> CompletePhotoboothAsync(string attemptToken)
    {
        await EnsureSchemaAsync();
        if (string.IsNullOrWhiteSpace(attemptToken))
        {
            return GameRewardCompletionResult.Invalid("Take four camera photos before completing the Photobooth.");
        }

        var player = await _playerProfileService.GetOrCreateAsync();
        var playerId = player.Id;
        var activeAttemptToken = attemptToken.Trim();
        await using var transaction = await _context.Database.BeginTransactionAsync();
        var attempt = await _context.GameCompletionAttempts.FirstOrDefaultAsync(item => item.AttemptToken == activeAttemptToken
            && item.PlayerProfileId == playerId
            && item.GameSlug == "photobooth");

        if (attempt is null)
        {
            return GameRewardCompletionResult.Invalid("This Photobooth session could not be found. Start the camera and try again.");
        }

        if (attempt.Status == GameCompletionAttemptStatus.Completed)
        {
            await transaction.CommitAsync();
            return GameRewardCompletionResult.PreviouslyCompleted(player.TokenBalance);
        }

        if (attempt.CaptureCount < 4)
        {
            return GameRewardCompletionResult.Invalid("Only four camera captures can earn the Photobooth token reward. Placeholder photos do not earn tokens.");
        }

        await _tokenWalletService.EnsureRewardSettingAsync("complete:photobooth", "Complete Photobooth", 20);
        attempt.Status = GameCompletionAttemptStatus.Completed;
        attempt.CompletedAt = DateTime.UtcNow;

        var reward = await _tokenWalletService.AwardCompletionAsync(
            $"photobooth:attempt:{attempt.AttemptToken}",
            "complete:photobooth",
            transaction);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return GameRewardCompletionResult.Success(
            0,
            new GameResultDto("Photobooth complete", "Your camera captures were saved as a completed strip.", null),
            reward);
    }

    private async Task<GameRewardCompletionResult> CompleteAttemptAsync(string gameSlug, string attemptToken, int score)
    {
        await EnsureSchemaAsync();
        if (string.IsNullOrWhiteSpace(attemptToken))
        {
            return GameRewardCompletionResult.Invalid("Missing completion attempt.");
        }

        var player = await _playerProfileService.GetOrCreateAsync();
        var playerId = player.Id;
        var activeAttemptToken = attemptToken.Trim();
        var activeGameSlug = gameSlug;
        var completedScore = score;
        await using var transaction = await _context.Database.BeginTransactionAsync();

        var attempt = await _context.GameCompletionAttempts
            .FirstOrDefaultAsync(item => item.AttemptToken == activeAttemptToken
                && item.PlayerProfileId == playerId
                && item.GameSlug == activeGameSlug);
        if (attempt is null)
        {
            return GameRewardCompletionResult.Invalid("This game attempt could not be found. Reload the game and try again.");
        }

        if (attempt.Status == GameCompletionAttemptStatus.Completed)
        {
            await transaction.CommitAsync();
            return GameRewardCompletionResult.PreviouslyCompleted(player.TokenBalance);
        }

        var game = await _context.MiniGames.FirstOrDefaultAsync(item => item.Slug == activeGameSlug);
        if (game is null)
        {
            return GameRewardCompletionResult.Invalid("This game is not configured.");
        }

        var gameId = game.Id;
        var resultRange = await _context.ResultRanges
            .Where(range => range.MiniGameId == gameId && completedScore >= range.MinScore && completedScore <= range.MaxScore)
            .OrderBy(range => range.DisplayOrder)
            .ThenBy(range => range.MinScore)
            .FirstOrDefaultAsync();
        if (resultRange is null)
        {
            return GameRewardCompletionResult.Invalid("No result range matches this completed game.");
        }

        string? imagePath = null;
        if (resultRange.MediaId is int resultMediaId)
        {
            imagePath = await _context.UploadedMedia
                .Where(media => media.Id == resultMediaId
                    && (media.MediaType == MediaType.Image || EF.Functions.Like(media.ContentType, "image/%")))
                .Select(media => media.FilePath)
                .FirstOrDefaultAsync();
        }

        var resultRangeId = resultRange.Id;
        var rewardKey = $"{activeGameSlug}:result:{resultRangeId}";
        await _tokenWalletService.EnsureRewardSettingAsync(rewardKey, $"{game.Name}: {resultRange.Title}", activeGameSlug == "cake" ? 25 : 35);

        attempt.Status = GameCompletionAttemptStatus.Completed;
        attempt.Score = completedScore;
        attempt.ResultRangeId = resultRangeId;
        attempt.CompletedAt = DateTime.UtcNow;

        var reward = await _tokenWalletService.AwardCompletionAsync(
            $"{gameSlug}:attempt:{attempt.AttemptToken}",
            rewardKey,
            transaction);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return GameRewardCompletionResult.Success(
            completedScore,
            new GameResultDto(resultRange.Title, resultRange.Message, imagePath),
            reward);
    }

    private async Task EnsureSchemaAsync()
    {
        if (!await _schemaService.TableExistsAsync("GameCompletionAttempts"))
        {
            throw new PlayerDataUnavailableException("Game reward attempts are not available yet. Restart the app or run database migrations.");
        }
    }
}

public sealed class CakeCompletionRequest
{
    public string AttemptToken { get; set; } = "";
    public int BaseId { get; set; }
    public int FrostingId { get; set; }
    public int ToppingId { get; set; }
    public int DecorationId { get; set; }
}

public sealed class PerfectDateCompletionRequest
{
    public string AttemptToken { get; set; } = "";
    public List<int> OptionIds { get; set; } = [];
}

public sealed record PhotoboothCaptureResult(bool Succeeded, int CaptureCount, string? Message)
{
    public static PhotoboothCaptureResult Success(int captureCount) => new(true, captureCount, null);
    public static PhotoboothCaptureResult Invalid(string message) => new(false, 0, message);
}

public sealed record GameAttemptStartResult(bool Succeeded, string? AttemptToken, string? Message)
{
    public static GameAttemptStartResult Success(string attemptToken) => new(true, attemptToken, null);
    public static GameAttemptStartResult Invalid(string message) => new(false, null, message);
}

public sealed record GameResultDto(string Title, string Message, string? ImagePath);
public sealed record GameRewardCompletionResult(bool Succeeded, bool AlreadyCompleted, int Score, GameResultDto? Result, TokenAwardResult? Reward, string? Message)
{
    public static GameRewardCompletionResult Success(int score, GameResultDto result, TokenAwardResult reward) => new(true, false, score, result, reward, null);
    public static GameRewardCompletionResult PreviouslyCompleted(int balance) => new(true, true, 0, null, new TokenAwardResult(false, 0, balance, "Reward already claimed."), null);
    public static GameRewardCompletionResult Invalid(string message) => new(false, false, 0, null, null, message);
}
