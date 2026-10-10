using CasualDate.Data;
using CasualDate.Models;
using CasualDate.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CasualDate.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Route("admin/settings")]
public class SettingsController : Controller
{
    private readonly ApplicationDbContext _context;

    public SettingsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var settings = await GetOrCreateSettingsAsync();
        await PopulateMediaSelectorsAsync();

        return View("~/Views/Admin/Settings/Index.cshtml", new GameSettingEditViewModel
        {
            Id = settings.Id,
            GameTitle = settings.GameTitle,
            GirlfriendName = settings.GirlfriendName,
            PlayerName = settings.PlayerName,
            IntroMessage = settings.IntroMessage,
            IntroSubtitle = settings.IntroSubtitle,
            MainMenuMessage = settings.MainMenuMessage,
            PhotoboothCompletionHeading = settings.PhotoboothCompletionHeading,
            PhotoboothCompletionSubheading = settings.PhotoboothCompletionSubheading,
            YesButtonText = settings.YesButtonText,
            NoButtonText = settings.NoButtonText,
            NoButtonDisableAfterClicks = settings.NoButtonDisableAfterClicks,
            YesButtonGrowthRate = settings.YesButtonGrowthRate,
            FinalTitle = settings.FinalTitle,
            FinalMessage = settings.FinalMessage,
            PrimaryColor = settings.PrimaryColor,
            SecondaryColor = settings.SecondaryColor,
            BackgroundMediaId = settings.BackgroundMediaId,
            MusicMediaId = settings.MusicMediaId
        });
    }

    [HttpPost("")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(GameSettingEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateMediaSelectorsAsync();
            return View("~/Views/Admin/Settings/Index.cshtml", model);
        }

        var settings = await GetOrCreateSettingsAsync();
        settings.GameTitle = model.GameTitle.Trim();
        settings.GirlfriendName = model.GirlfriendName.Trim();
        settings.PlayerName = model.PlayerName.Trim();
        settings.IntroMessage = model.IntroMessage.Trim();
        settings.IntroSubtitle = model.IntroSubtitle.Trim();
        settings.MainMenuMessage = model.MainMenuMessage.Trim();
        settings.PhotoboothCompletionHeading = string.IsNullOrWhiteSpace(model.PhotoboothCompletionHeading)
            ? "Game Completed!"
            : model.PhotoboothCompletionHeading.Trim();
        settings.PhotoboothCompletionSubheading = string.IsNullOrWhiteSpace(model.PhotoboothCompletionSubheading)
            ? "Your smile is as beautiful as always."
            : model.PhotoboothCompletionSubheading.Trim();
        settings.YesButtonText = model.YesButtonText.Trim();
        settings.NoButtonText = model.NoButtonText.Trim();
        settings.NoButtonDisableAfterClicks = model.NoButtonDisableAfterClicks;
        settings.YesButtonGrowthRate = model.YesButtonGrowthRate;
        settings.FinalTitle = model.FinalTitle.Trim();
        settings.FinalMessage = model.FinalMessage.Trim();
        settings.PrimaryColor = model.PrimaryColor.Trim();
        settings.SecondaryColor = model.SecondaryColor.Trim();
        settings.BackgroundMediaId = model.BackgroundMediaId;
        settings.MusicMediaId = model.MusicMediaId;
        settings.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        TempData["StatusMessage"] = "Settings saved.";

        return RedirectToAction(nameof(Index));
    }

    private async Task<GameSetting> GetOrCreateSettingsAsync()
    {
        var settings = await _context.GameSettings
            .OrderBy(setting => setting.Id)
            .FirstOrDefaultAsync();

        if (settings is not null)
        {
            return settings;
        }

        settings = new GameSetting
        {
            Id = 1,
            IntroMessage = "Would you like to go on a date with me, {name}?"
        };

        _context.GameSettings.Add(settings);
        await _context.SaveChangesAsync();

        return settings;
    }

    private async Task PopulateMediaSelectorsAsync()
    {
        var images = await _context.UploadedMedia
            .Where(media => media.MediaType == MediaType.Image)
            .OrderBy(media => media.DisplayName)
            .Select(media => new SelectListItem(media.DisplayName, media.Id.ToString()))
            .ToListAsync();

        var audio = await _context.UploadedMedia
            .Where(media => media.MediaType == MediaType.Audio)
            .OrderBy(media => media.DisplayName)
            .Select(media => new SelectListItem(media.DisplayName, media.Id.ToString()))
            .ToListAsync();

        images.Insert(0, new SelectListItem("No background image", ""));
        audio.Insert(0, new SelectListItem("No background music", ""));
        ViewBag.BackgroundMedia = images;
        ViewBag.MusicMedia = audio;
    }
}
