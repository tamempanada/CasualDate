using System.Diagnostics;
using System.Data.Common;
using CasualDate.Data;
using Microsoft.AspNetCore.Mvc;
using CasualDate.Models;
using CasualDate.Services;
using CasualDate.ViewModels.Player;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CasualDate.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly TokenWalletService _tokenWalletService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly PlayerCosmeticService _playerCosmeticService;

    public HomeController(
        ApplicationDbContext context,
        TokenWalletService tokenWalletService,
        UserManager<ApplicationUser> userManager,
        PlayerCosmeticService playerCosmeticService)
    {
        _context = context;
        _tokenWalletService = tokenWalletService;
        _userManager = userManager;
        _playerCosmeticService = playerCosmeticService;
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> Index()
    {
        var settings = await GetSettingsAsync();

        return View(new IntroViewModel
        {
            GameTitle = settings.GameTitle,
            GirlfriendName = settings.GirlfriendName,
            IntroMessage = settings.IntroMessage.Replace("{name}", settings.GirlfriendName),
            IntroSubtitle = settings.IntroSubtitle,
            YesButtonText = settings.YesButtonText,
            NoButtonText = settings.NoButtonText,
            NoButtonDisableAfterClicks = settings.NoButtonDisableAfterClicks,
            YesButtonGrowthRate = settings.YesButtonGrowthRate,
            PrimaryColor = settings.PrimaryColor,
            SecondaryColor = settings.SecondaryColor
        });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> Menu()
    {
        var settings = await GetSettingsAsync();
        var games = await _context.MiniGames
            .Where(game => game.IsEnabled)
            .OrderBy(game => game.DisplayOrder)
            .Select(game => new MenuGameViewModel
            {
                Slug = game.Slug,
                Name = game.Name,
                Description = game.Description,
                IconLabel = game.IconLabel,
                ActionName = GetGameActionName(game.Slug),
                IsCompletionGame = IsCompletionGame(game.Slug)
            })
            .ToListAsync();
        var signedInUser = await _userManager.GetUserAsync(User);
        var isPlayerSignedIn = signedInUser is not null && await _userManager.IsInRoleAsync(signedInUser, "Player");
        var wallet = new TokenWalletSnapshot(0);
        PlayerCompanionDto? companion = null;

        if (isPlayerSignedIn)
        {
            try
            {
                wallet = await _tokenWalletService.GetSnapshotAsync();
            }
            catch (DbException ex)
            {
                HttpContext.RequestServices
                    .GetRequiredService<ILogger<HomeController>>()
                    .LogWarning(ex, "Player wallet is unavailable. Run database migrations to enable wallet features.");
            }
            catch (PlayerDataUnavailableException ex)
            {
                HttpContext.RequestServices
                    .GetRequiredService<ILogger<HomeController>>()
                    .LogWarning(ex, "Player wallet is unavailable. Run database migrations to enable wallet features.");
            }

            try
            {
                companion = await _playerCosmeticService.GetEquippedCompanionAsync();
            }
            catch (PlayerDataUnavailableException)
            {
                // Cosmetic preferences are optional for existing player data during an upgrade.
            }
        }

        return View(new MenuViewModel
        {
            GameTitle = settings.GameTitle,
            WelcomeMessage = settings.MainMenuMessage,
            PrimaryColor = settings.PrimaryColor,
            SecondaryColor = settings.SecondaryColor,
            TokenBalance = wallet.Balance,
            IsPlayerSignedIn = isPlayerSignedIn,
            CompanionName = companion?.Name,
            CompanionImagePath = companion?.ImagePath,
            CompanionIsLegendary = companion?.Rarity == GachaRarity.Legendary.ToString(),
            Games = games
        });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private async Task<GameSetting> GetSettingsAsync()
    {
        return await _context.GameSettings
            .OrderBy(setting => setting.Id)
            .FirstOrDefaultAsync()
            ?? new GameSetting();
    }

    private static string GetGameActionName(string slug)
    {
        return slug switch
        {
            "perfect-date" => "PerfectDate",
            "photobooth" => "Photobooth",
            "cake" => "Cake",
            "quiz" => "Quiz",
            "gacha" => "Gacha",
            "love-blocks" => "LoveBlocks",
            "hangman" => "Hangman",
            "blackjack" => "Blackjack",
            _ => "Quiz"
        };
    }

    private static bool IsCompletionGame(string slug)
    {
        return slug is "photobooth" or "cake" or "perfect-date" or "quiz";
    }
}
