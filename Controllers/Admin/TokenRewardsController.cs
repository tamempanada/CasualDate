using CasualDate.Data;
using CasualDate.Models;
using CasualDate.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CasualDate.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Route("admin/token-rewards")]
public class TokenRewardsController : Controller
{
    private static readonly IReadOnlyList<TokenRewardSeed> BaseRewards = CreateBaseRewards();

    private readonly ApplicationDbContext _context;

    public TokenRewardsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var rewards = await GetSupportedRewardsAsync();
        await EnsureDefaultRewardsAsync(rewards);
        return View("~/Views/Admin/TokenRewards/Index.cshtml", await BuildViewModelAsync(rewards));
    }

    [HttpPost("")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(TokenRewardsEditViewModel model)
    {
        var rewards = await GetSupportedRewardsAsync();
        await EnsureDefaultRewardsAsync(rewards);

        if (!ModelState.IsValid)
        {
            return View("~/Views/Admin/TokenRewards/Index.cshtml", model);
        }

        var allowedKeys = rewards.Select(reward => reward.EventKey).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (model.Rewards.Count != rewards.Count
            || model.Rewards.Any(reward => !allowedKeys.Contains(reward.EventKey))
            || model.Rewards.Select(reward => reward.EventKey).Distinct(StringComparer.OrdinalIgnoreCase).Count() != model.Rewards.Count)
        {
            ModelState.AddModelError(string.Empty, "The submitted reward settings were invalid. Reload the page and try again.");
            return View("~/Views/Admin/TokenRewards/Index.cshtml", await BuildViewModelAsync(rewards));
        }

        var settings = await _context.TokenRewardSettings
            .Where(reward => allowedKeys.Contains(reward.EventKey))
            .ToListAsync();
        var submittedRewards = model.Rewards.ToDictionary(reward => reward.EventKey, StringComparer.OrdinalIgnoreCase);

        foreach (var setting in settings)
        {
            if (!submittedRewards.TryGetValue(setting.EventKey, out var submittedReward))
            {
                continue;
            }

            setting.TokenAmount = submittedReward.TokenAmount;
            setting.TriggerInterval = submittedReward.TriggerInterval;
            setting.IsEnabled = submittedReward.IsEnabled;
            setting.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        TempData["StatusMessage"] = "Token reward settings saved.";

        return RedirectToAction(nameof(Index));
    }

    private async Task<TokenRewardsEditViewModel> BuildViewModelAsync(IReadOnlyList<TokenRewardSeed> supportedRewards)
    {
        var rewardSettings = await _context.TokenRewardSettings
            .OrderBy(reward => reward.Id)
            .ToListAsync();
        var settingsByKey = rewardSettings.ToDictionary(reward => reward.EventKey, StringComparer.OrdinalIgnoreCase);

        return new TokenRewardsEditViewModel
        {
            Rewards = supportedRewards
                .Select(defaultReward =>
                {
                    var setting = settingsByKey[defaultReward.EventKey];

                    return new TokenRewardEditItemViewModel
                    {
                        Id = setting.Id,
                        EventKey = setting.EventKey,
                        DisplayName = setting.DisplayName,
                        GroupName = defaultReward.GroupName,
                        TokenAmount = setting.TokenAmount,
                        TriggerInterval = setting.TriggerInterval,
                        IsEnabled = setting.IsEnabled
                    };
                })
                .ToList()
        };
    }

    private async Task EnsureDefaultRewardsAsync(IReadOnlyList<TokenRewardSeed> supportedRewards)
    {
        var existingKeys = await _context.TokenRewardSettings
            .Select(reward => reward.EventKey)
            .ToListAsync();
        var existingKeySet = existingKeys.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var now = DateTime.UtcNow;

        foreach (var defaultReward in supportedRewards.Where(reward => !existingKeySet.Contains(reward.EventKey)))
        {
            _context.TokenRewardSettings.Add(new TokenRewardSetting
            {
                EventKey = defaultReward.EventKey,
                DisplayName = defaultReward.DisplayName,
                TokenAmount = defaultReward.TokenAmount,
                TriggerInterval = defaultReward.TriggerInterval,
                IsEnabled = true,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        await _context.SaveChangesAsync();
    }

    private async Task<IReadOnlyList<TokenRewardSeed>> GetSupportedRewardsAsync()
    {
        var quizQuestions = await _context.QuizQuestions
            .Where(question => question.IsEnabled)
            .OrderBy(question => question.DisplayOrder)
            .ThenBy(question => question.Id)
            .Select(question => new { question.Id, question.QuestionText })
            .ToListAsync();

        var resultGames = await _context.MiniGames
            .Where(game => game.Slug == "cake" || game.Slug == "perfect-date")
            .Select(game => new { game.Id, game.Slug, game.Name })
            .ToListAsync();
        var resultGameIds = resultGames.Select(game => game.Id).ToList();
        var resultRanges = await _context.ResultRanges
            .Where(range => resultGameIds.Contains(range.MiniGameId))
            .OrderBy(range => range.DisplayOrder)
            .ThenBy(range => range.MinScore)
            .ToListAsync();
        var gamesById = resultGames.ToDictionary(game => game.Id);

        return BaseRewards
            .Concat(resultRanges.Select(range =>
            {
                var game = gamesById[range.MiniGameId];
                return new TokenRewardSeed(
                    $"{game.Slug}:result:{range.Id}",
                    $"{range.Title} ({range.MinScore}-{range.MaxScore})",
                    game.Name,
                    game.Slug == "cake" ? 25 : 35);
            }))
            .Concat(quizQuestions.Select(question => new TokenRewardSeed(
                $"quiz:question:{question.Id}",
                $"Question: {question.QuestionText}",
                "How Well Do You Know Me?",
                1)))
            .ToList();
    }

    private static IReadOnlyList<TokenRewardSeed> CreateBaseRewards()
    {
        var rewards = new List<TokenRewardSeed>
        {
            new("complete:photobooth", "Complete Photobooth", "Photobooth", 20),
            new("hangman:endless:word", "Endless: each completed word", "Hangman", 1),
            new("love-blocks:score", "Score milestone reward", "Love Blocks", 5, 500),
            new("love-blocks:lines", "Line-clear reward", "Love Blocks", 5, 5),
            new("love-blocks:personal-best", "New personal-best reward", "Love Blocks", 20)
        };

        rewards.AddRange(Enumerable.Range(1, 15)
            .Select(level => new TokenRewardSeed(
                $"hangman:adventure:level:{level:D2}",
                $"Adventure level {level}",
                "Hangman",
                2)));

        return rewards;
    }

    private sealed record TokenRewardSeed(string EventKey, string DisplayName, string GroupName, int TokenAmount, int TriggerInterval = 1);
}
