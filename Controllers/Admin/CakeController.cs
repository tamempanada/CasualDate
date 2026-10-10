using CasualDate.Data;
using CasualDate.Models;
using CasualDate.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CasualDate.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Route("admin/cake")]
public class CakeController : Controller
{
    private readonly ApplicationDbContext _context;

    public CakeController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var items = await _context.CakeItems
            .OrderBy(item => item.ItemType)
            .ThenBy(item => item.DisplayOrder)
            .ThenBy(item => item.Name)
            .ToListAsync();

        return View("~/Views/Admin/Cake/Index.cshtml", items);
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create()
    {
        await LoadImageMediaAsync();
        return View("~/Views/Admin/Cake/Edit.cshtml", new CakeItemEditViewModel
        {
            IsEnabled = true,
            Score = 20
        });
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CakeItemEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadImageMediaAsync();
            return View("~/Views/Admin/Cake/Edit.cshtml", model);
        }

        var now = DateTime.UtcNow;
        _context.CakeItems.Add(new CakeItem
        {
            ItemType = model.ItemType,
            Name = model.Name?.Trim() ?? "",
            Description = model.Description?.Trim() ?? "",
            Score = model.Score,
            MediaId = model.MediaId,
            DisplayOrder = model.DisplayOrder,
            IsEnabled = model.IsEnabled,
            CreatedAt = now,
            UpdatedAt = now
        });

        await _context.SaveChangesAsync();
        TempData["StatusMessage"] = "Cake item created.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var item = await _context.CakeItems.FindAsync(id);

        if (item is null)
        {
            return NotFound();
        }

        await LoadImageMediaAsync();
        return View("~/Views/Admin/Cake/Edit.cshtml", new CakeItemEditViewModel
        {
            Id = item.Id,
            ItemType = item.ItemType,
            Name = item.Name,
            Description = item.Description,
            Score = item.Score,
            MediaId = item.MediaId,
            DisplayOrder = item.DisplayOrder,
            IsEnabled = item.IsEnabled
        });
    }

    [HttpPost("edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CakeItemEditViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await LoadImageMediaAsync();
            return View("~/Views/Admin/Cake/Edit.cshtml", model);
        }

        var item = await _context.CakeItems.FindAsync(id);

        if (item is null)
        {
            return NotFound();
        }

        item.ItemType = model.ItemType;
        item.Name = model.Name?.Trim() ?? "";
        item.Description = model.Description?.Trim() ?? "";
        item.Score = model.Score;
        item.MediaId = model.MediaId;
        item.DisplayOrder = model.DisplayOrder;
        item.IsEnabled = model.IsEnabled;
        item.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        TempData["StatusMessage"] = "Cake item saved.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _context.CakeItems.FindAsync(id);

        if (item is not null)
        {
            _context.CakeItems.Remove(item);
            await _context.SaveChangesAsync();
            TempData["StatusMessage"] = "Cake item deleted.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadImageMediaAsync()
    {
        var images = await _context.UploadedMedia
            .Where(media => media.MediaType == MediaType.Image)
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
}
