using CasualDate.Data;
using CasualDate.Models;
using CasualDate.Services;
using CasualDate.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CasualDate.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Route("admin/gacha")]
public class GachaController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly BuiltInCosmeticCatalogService _catalogService;

    public GachaController(ApplicationDbContext context, BuiltInCosmeticCatalogService catalogService)
    {
        _context = context;
        _catalogService = catalogService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        await _catalogService.EnsureSeededAsync();
        var rewards = await _context.GachaRewards
            .Include(reward => reward.Media)
            .OrderBy(reward => reward.DisplayOrder)
            .ThenBy(reward => reward.Name)
            .ToListAsync();
        var settings = await GetOrCreateSettingsAsync();
        var banner = settings.GachaBannerMediaId.HasValue
            ? await _context.UploadedMedia.FirstOrDefaultAsync(media => media.Id == settings.GachaBannerMediaId.Value)
            : null;

        return View("~/Views/Admin/Gacha/Index.cshtml", new GachaAdminIndexViewModel
        {
            Rewards = rewards,
            GachaBannerMediaId = settings.GachaBannerMediaId,
            GachaBannerPath = banner?.FilePath,
            GachaBannerName = banner?.DisplayName ?? banner?.OriginalFileName,
            ImageMediaItems = await GetImageMediaItemsAsync("No banner")
        });
    }

    [HttpPost("banner")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveBanner(GachaBannerEditViewModel model)
    {
        if (model.GachaBannerMediaId.HasValue)
        {
            var isImage = await _context.UploadedMedia.AnyAsync(media => media.Id == model.GachaBannerMediaId.Value
                && (media.MediaType == MediaType.Image || EF.Functions.Like(media.ContentType, "image/%")));

            if (!isImage)
            {
                TempData["StatusMessage"] = "Choose an image from the media library for the Gacha banner.";
                return RedirectToAction(nameof(Index));
            }
        }

        var settings = await GetOrCreateSettingsAsync();
        settings.GachaBannerMediaId = model.GachaBannerMediaId;
        settings.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        TempData["StatusMessage"] = model.GachaBannerMediaId.HasValue
            ? "Gacha banner saved."
            : "Gacha banner deactivated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("banner/deactivate")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeactivateBanner()
    {
        var settings = await GetOrCreateSettingsAsync();
        settings.GachaBannerMediaId = null;
        settings.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        TempData["StatusMessage"] = "Gacha banner deactivated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("cosmetic/{id:int}/media")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateCosmeticMedia(int id, int? mediaId)
    {
        var cosmetic = await _context.GachaRewards.FirstOrDefaultAsync(reward => reward.Id == id && reward.BuiltInCosmeticId != null);
        if (cosmetic is null)
        {
            return NotFound();
        }

        if (mediaId.HasValue && !await _context.UploadedMedia.AnyAsync(media => media.Id == mediaId.Value && media.MediaType == MediaType.Image))
        {
            TempData["StatusMessage"] = "Choose an image from the media library for the cosmetic display image.";
            return RedirectToAction(nameof(Index));
        }

        cosmetic.MediaId = mediaId;
        cosmetic.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        TempData["StatusMessage"] = "Built-in cosmetic display image saved.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create()
    {
        await LoadImageMediaAsync();
        return View("~/Views/Admin/Gacha/Edit.cshtml", new GachaRewardEditViewModel { IsActive = true });
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(GachaRewardEditViewModel model)
    {
        Normalize(model);
        ValidateReward(model);

        if (!ModelState.IsValid)
        {
            await LoadImageMediaAsync();
            return View("~/Views/Admin/Gacha/Edit.cshtml", model);
        }

        var now = DateTime.UtcNow;
        _context.GachaRewards.Add(new GachaReward
        {
            RewardType = model.RewardType,
            Rarity = model.Rarity,
            MediaId = model.MediaId,
            Name = model.Name,
            Description = model.Description,
            LetterTitle = model.LetterTitle,
            LetterContent = model.LetterContent,
            IsActive = model.IsActive,
            DisplayOrder = model.DisplayOrder,
            CreatedAt = now,
            UpdatedAt = now
        });

        await _context.SaveChangesAsync();
        TempData["StatusMessage"] = "Gacha reward created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var reward = await _context.GachaRewards.FindAsync(id);

        if (reward is null)
        {
            return NotFound();
        }

        if (!string.IsNullOrWhiteSpace(reward.BuiltInCosmeticId))
        {
            TempData["StatusMessage"] = "Built-in cosmetics are fixed. You can only replace their display image from the Gacha Rewards list.";
            return RedirectToAction(nameof(Index));
        }

        await LoadImageMediaAsync();
        return View("~/Views/Admin/Gacha/Edit.cshtml", new GachaRewardEditViewModel
        {
            Id = reward.Id,
            RewardType = reward.RewardType,
            Rarity = reward.Rarity,
            MediaId = reward.MediaId,
            Name = reward.Name,
            Description = reward.Description,
            LetterTitle = reward.LetterTitle,
            LetterContent = reward.LetterContent,
            IsActive = reward.IsActive,
            DisplayOrder = reward.DisplayOrder
        });
    }

    [HttpPost("edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, GachaRewardEditViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        Normalize(model);
        ValidateReward(model);

        if (!ModelState.IsValid)
        {
            await LoadImageMediaAsync();
            return View("~/Views/Admin/Gacha/Edit.cshtml", model);
        }

        var reward = await _context.GachaRewards.FindAsync(id);

        if (reward is null)
        {
            return NotFound();
        }

        if (!string.IsNullOrWhiteSpace(reward.BuiltInCosmeticId))
        {
            TempData["StatusMessage"] = "Built-in cosmetics cannot be edited.";
            return RedirectToAction(nameof(Index));
        }

        reward.RewardType = model.RewardType;
        reward.Rarity = model.Rarity;
        reward.MediaId = model.MediaId;
        reward.Name = model.Name;
        reward.Description = model.Description;
        reward.LetterTitle = model.LetterTitle;
        reward.LetterContent = model.LetterContent;
        reward.IsActive = model.IsActive;
        reward.DisplayOrder = model.DisplayOrder;
        reward.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        TempData["StatusMessage"] = "Gacha reward saved.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var reward = await _context.GachaRewards.FindAsync(id);

        if (reward is null)
        {
            return RedirectToAction(nameof(Index));
        }

        if (!string.IsNullOrWhiteSpace(reward.BuiltInCosmeticId))
        {
            TempData["StatusMessage"] = "Built-in cosmetics cannot be deleted or deactivated.";
            return RedirectToAction(nameof(Index));
        }

        var hasPlayerHistory = await _context.GachaPulls.AnyAsync(pull => pull.GachaRewardId == id)
            || await _context.GachaCollectionItems.AnyAsync(item => item.GachaRewardId == id);

        if (hasPlayerHistory)
        {
            reward.IsActive = false;
            reward.UpdatedAt = DateTime.UtcNow;
            TempData["StatusMessage"] = "Reward has player history, so it was deactivated instead of deleted.";
        }
        else
        {
            _context.GachaRewards.Remove(reward);
            TempData["StatusMessage"] = "Gacha reward deleted.";
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private void ValidateReward(GachaRewardEditViewModel model)
    {
        if (model.RewardType == GachaRewardType.Cosmetic)
        {
            ModelState.AddModelError(nameof(model.RewardType), "Built-in cosmetics are managed by the application and cannot be created manually.");
        }
        if (model.RewardType is GachaRewardType.Doll or GachaRewardType.Poster)
        {
            if (!model.MediaId.HasValue)
            {
                ModelState.AddModelError(nameof(model.MediaId), "Choose artwork for Doll and Poster rewards.");
            }
        }

        if (model.RewardType == GachaRewardType.Letter && string.IsNullOrWhiteSpace(model.LetterContent))
        {
            ModelState.AddModelError(nameof(model.LetterContent), "Write the letter content.");
        }
    }

    private static void Normalize(GachaRewardEditViewModel model)
    {
        model.Name = model.Name?.Trim() ?? "";
        model.Description = model.Description?.Trim() ?? "";
        model.LetterTitle = model.LetterTitle?.Trim() ?? "";
        model.LetterContent = model.LetterContent?.Trim() ?? "";

        if (model.RewardType == GachaRewardType.Letter)
        {
            model.MediaId = null;
        }
        else
        {
            model.LetterTitle = "";
            model.LetterContent = "";
        }
    }

    private async Task LoadImageMediaAsync()
    {
        ViewBag.ImageMediaItems = await GetImageMediaItemsAsync("No artwork");
    }

    private async Task<List<SelectListItem>> GetImageMediaItemsAsync(string emptyText)
    {
        var images = await _context.UploadedMedia
            .Where(media => media.MediaType == MediaType.Image || EF.Functions.Like(media.ContentType, "image/%"))
            .OrderBy(media => media.DisplayName)
            .ThenBy(media => media.OriginalFileName)
            .Select(media => new SelectListItem
            {
                Value = media.Id.ToString(),
                Text = string.IsNullOrWhiteSpace(media.DisplayName)
                    ? media.OriginalFileName
                    : media.DisplayName
            })
            .ToListAsync();

        images.Insert(0, new SelectListItem { Value = "", Text = emptyText });
        return images;
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

        settings = new GameSetting();
        _context.GameSettings.Add(settings);
        await _context.SaveChangesAsync();
        return settings;
    }
}
