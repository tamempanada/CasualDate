using CasualDate.Data;
using CasualDate.Models;
using CasualDate.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CasualDate.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Route("admin/perfect-date")]
public class PerfectDateController : Controller
{
    private readonly ApplicationDbContext _context;

    public PerfectDateController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var categories = await _context.DateCategories
            .OrderBy(category => category.DisplayOrder)
            .ThenBy(category => category.Id)
            .ToListAsync();

        return View("~/Views/Admin/PerfectDate/Index.cshtml", categories);
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create()
    {
        await LoadImageMediaAsync();
        return View("~/Views/Admin/PerfectDate/Edit.cshtml", CreateDefaultModel());
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DateCategoryEditViewModel model)
    {
        NormalizeOptions(model);
        ValidateOptions(model);

        if (!ModelState.IsValid)
        {
            EnsureOptionRows(model);
            await LoadImageMediaAsync();
            await PopulateOptionImagePathsAsync(model);
            return View("~/Views/Admin/PerfectDate/Edit.cshtml", model);
        }

        var now = DateTime.UtcNow;
        var category = new DateCategory
        {
            Name = model.Name?.Trim() ?? "",
            Description = model.Description?.Trim() ?? "",
            DisplayOrder = model.DisplayOrder,
            IsEnabled = model.IsEnabled,
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.DateCategories.Add(category);
        await _context.SaveChangesAsync();

        AddOptions(category.Id, model, now);
        await _context.SaveChangesAsync();

        TempData["StatusMessage"] = "Date category created.";
        return RedirectToAction(nameof(Edit), new { id = category.Id });
    }

    [HttpGet("edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var category = await _context.DateCategories.FindAsync(id);

        if (category is null)
        {
            return NotFound();
        }

        var options = await _context.DateOptions
            .Where(option => option.DateCategoryId == id)
            .OrderBy(option => option.DisplayOrder)
            .ThenBy(option => option.Id)
            .ToListAsync();

        var model = new DateCategoryEditViewModel
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            DisplayOrder = category.DisplayOrder,
            IsEnabled = category.IsEnabled,
            Options = options.Select(option => new DateOptionEditViewModel
            {
                Id = option.Id,
                Name = option.Name ?? "",
                Description = option.Description ?? "",
                Score = option.Score,
                MediaId = option.MediaId,
                DisplayOrder = option.DisplayOrder,
                IsEnabled = option.IsEnabled
            }).ToList()
        };

        EnsureOptionRows(model);
        await LoadImageMediaAsync();
        await PopulateOptionImagePathsAsync(model);
        return View("~/Views/Admin/PerfectDate/Edit.cshtml", model);
    }

    [HttpPost("edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, DateCategoryEditViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        NormalizeOptions(model);
        ValidateOptions(model);

        if (!ModelState.IsValid)
        {
            EnsureOptionRows(model);
            await LoadImageMediaAsync();
            await PopulateOptionImagePathsAsync(model);
            return View("~/Views/Admin/PerfectDate/Edit.cshtml", model);
        }

        var category = await _context.DateCategories.FindAsync(id);

        if (category is null)
        {
            return NotFound();
        }

        category.Name = model.Name?.Trim() ?? "";
        category.Description = model.Description?.Trim() ?? "";
        category.DisplayOrder = model.DisplayOrder;
        category.IsEnabled = model.IsEnabled;
        category.UpdatedAt = DateTime.UtcNow;

        var existingOptions = await _context.DateOptions
            .Where(option => option.DateCategoryId == id)
            .ToListAsync();

        SyncOptions(id, model, existingOptions);
        await _context.SaveChangesAsync();

        TempData["StatusMessage"] = "Date category saved.";
        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpPost("delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _context.DateCategories.FindAsync(id);

        if (category is not null)
        {
            _context.DateCategories.Remove(category);
            await _context.SaveChangesAsync();
            TempData["StatusMessage"] = "Date category deleted.";
        }

        return RedirectToAction(nameof(Index));
    }

    private static DateCategoryEditViewModel CreateDefaultModel()
    {
        var model = new DateCategoryEditViewModel
        {
            IsEnabled = true
        };

        EnsureOptionRows(model);
        return model;
    }

    private static void EnsureOptionRows(DateCategoryEditViewModel model)
    {
        model.Options ??= new List<DateOptionEditViewModel>();

        while (model.Options.Count < 6)
        {
            model.Options.Add(new DateOptionEditViewModel
            {
                DisplayOrder = model.Options.Count + 1,
                IsEnabled = true,
                Score = 20
            });
        }
    }

    private static void NormalizeOptions(DateCategoryEditViewModel model)
    {
        EnsureOptionRows(model);

        for (var index = 0; index < model.Options.Count; index++)
        {
            model.Options[index].Name = model.Options[index].Name?.Trim() ?? "";
            model.Options[index].Description = model.Options[index].Description?.Trim() ?? "";
            model.Options[index].DisplayOrder = index + 1;
        }
    }

    private void ValidateOptions(DateCategoryEditViewModel model)
    {
        var filledOptions = model.Options
            .Where(option => !string.IsNullOrWhiteSpace(option.Name))
            .ToList();

        if (filledOptions.Count < 2)
        {
            ModelState.AddModelError("", "Add at least two date options.");
        }
    }

    private void AddOptions(int categoryId, DateCategoryEditViewModel model, DateTime now)
    {
        var filledOptions = model.Options
            .Where(option => !string.IsNullOrWhiteSpace(option.Name));

        foreach (var option in filledOptions)
        {
            _context.DateOptions.Add(new DateOption
            {
                DateCategoryId = categoryId,
                Name = option.Name ?? "",
                Description = option.Description ?? "",
                Score = option.Score,
                MediaId = option.MediaId,
                DisplayOrder = option.DisplayOrder,
                IsEnabled = option.IsEnabled,
                CreatedAt = now,
                UpdatedAt = now
            });
        }
    }

    private void SyncOptions(int categoryId, DateCategoryEditViewModel model, List<DateOption> existingOptions)
    {
        var now = DateTime.UtcNow;

        foreach (var optionModel in model.Options)
        {
            var existingOption = existingOptions.FirstOrDefault(option => option.Id == optionModel.Id);

            if (string.IsNullOrWhiteSpace(optionModel.Name))
            {
                if (existingOption is not null)
                {
                    _context.DateOptions.Remove(existingOption);
                }

                continue;
            }

            if (existingOption is null)
            {
                _context.DateOptions.Add(new DateOption
                {
                    DateCategoryId = categoryId,
                    Name = optionModel.Name ?? "",
                    Description = optionModel.Description ?? "",
                    Score = optionModel.Score,
                    MediaId = optionModel.MediaId,
                    DisplayOrder = optionModel.DisplayOrder,
                    IsEnabled = optionModel.IsEnabled,
                    CreatedAt = now,
                    UpdatedAt = now
                });

                continue;
            }

            existingOption.Name = optionModel.Name ?? "";
            existingOption.Description = optionModel.Description ?? "";
            existingOption.Score = optionModel.Score;
            existingOption.MediaId = optionModel.MediaId;
            existingOption.DisplayOrder = optionModel.DisplayOrder;
            existingOption.IsEnabled = optionModel.IsEnabled;
            existingOption.UpdatedAt = now;
        }
    }

    private async Task LoadImageMediaAsync()
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

        images.Insert(0, new SelectListItem { Value = "", Text = "No image" });
        ViewBag.ImageMediaItems = images;
    }

    private async Task PopulateOptionImagePathsAsync(DateCategoryEditViewModel model)
    {
        var mediaIds = model.Options
            .Select(option => option.MediaId)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        if (mediaIds.Count == 0)
        {
            foreach (var option in model.Options)
            {
                option.ImagePath = null;
            }

            return;
        }

        var mediaPaths = await _context.UploadedMedia
            .Where(media => mediaIds.Contains(media.Id)
                && (media.MediaType == MediaType.Image || EF.Functions.Like(media.ContentType, "image/%")))
            .ToDictionaryAsync(media => media.Id, media => media.FilePath);

        foreach (var option in model.Options)
        {
            option.ImagePath = option.MediaId.HasValue && mediaPaths.TryGetValue(option.MediaId.Value, out var imagePath)
                ? imagePath
                : null;
        }
    }
}
