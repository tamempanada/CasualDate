using System.Diagnostics;
using CasualDate.Data;
using Microsoft.AspNetCore.Mvc;
using CasualDate.Models;
using CasualDate.ViewModels.Player;
using Microsoft.EntityFrameworkCore;

namespace CasualDate.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
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
                ActionName = game.Slug == "perfect-date"
                    ? "PerfectDate"
                    : game.Slug == "photobooth"
                        ? "Photobooth"
                        : game.Slug == "cake"
                            ? "Cake"
                            : "Quiz"
            })
            .ToListAsync();

        return View(new MenuViewModel
        {
            GameTitle = settings.GameTitle,
            WelcomeMessage = settings.MainMenuMessage,
            PrimaryColor = settings.PrimaryColor,
            SecondaryColor = settings.SecondaryColor,
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
}
