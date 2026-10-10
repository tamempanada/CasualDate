using CasualDate.Data;
using CasualDate.Models;
using CasualDate.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CasualDate.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Route("admin/results")]
public class ResultsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ResultsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var ranges = await _context.ResultRanges
            .OrderBy(range => range.MiniGameId)
            .ThenBy(range => range.DisplayOrder)
            .ThenBy(range => range.MinScore)
            .ToListAsync();

        await PopulateMiniGamesAsync();
        return View("~/Views/Admin/Results/Index.cshtml", ranges);
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create()
    {
        await PopulateMiniGamesAsync();
        return View("~/Views/Admin/Results/Edit.cshtml", new ResultRangeEditViewModel());
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ResultRangeEditViewModel model)
    {
        ValidateScoreRange(model);

        if (!ModelState.IsValid)
        {
            await PopulateMiniGamesAsync();
            return View("~/Views/Admin/Results/Edit.cshtml", model);
        }

        var now = DateTime.UtcNow;
        _context.ResultRanges.Add(new ResultRange
        {
            MiniGameId = model.MiniGameId,
            MinScore = model.MinScore,
            MaxScore = model.MaxScore,
            Title = model.Title.Trim(),
            Message = model.Message.Trim(),
            MediaId = model.MediaId,
            DisplayOrder = model.DisplayOrder,
            CreatedAt = now,
            UpdatedAt = now
        });

        await _context.SaveChangesAsync();
        TempData["StatusMessage"] = "Result range created.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var range = await _context.ResultRanges.FindAsync(id);

        if (range is null)
        {
            return NotFound();
        }

        await PopulateMiniGamesAsync();
        return View("~/Views/Admin/Results/Edit.cshtml", new ResultRangeEditViewModel
        {
            Id = range.Id,
            MiniGameId = range.MiniGameId,
            MinScore = range.MinScore,
            MaxScore = range.MaxScore,
            Title = range.Title,
            Message = range.Message,
            MediaId = range.MediaId,
            DisplayOrder = range.DisplayOrder
        });
    }

    [HttpPost("edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ResultRangeEditViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        ValidateScoreRange(model);

        if (!ModelState.IsValid)
        {
            await PopulateMiniGamesAsync();
            return View("~/Views/Admin/Results/Edit.cshtml", model);
        }

        var range = await _context.ResultRanges.FindAsync(id);

        if (range is null)
        {
            return NotFound();
        }

        range.MiniGameId = model.MiniGameId;
        range.MinScore = model.MinScore;
        range.MaxScore = model.MaxScore;
        range.Title = model.Title.Trim();
        range.Message = model.Message.Trim();
        range.MediaId = model.MediaId;
        range.DisplayOrder = model.DisplayOrder;
        range.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        TempData["StatusMessage"] = "Result range saved.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var range = await _context.ResultRanges.FindAsync(id);

        if (range is not null)
        {
            _context.ResultRanges.Remove(range);
            await _context.SaveChangesAsync();
            TempData["StatusMessage"] = "Result range deleted.";
        }

        return RedirectToAction(nameof(Index));
    }

    private void ValidateScoreRange(ResultRangeEditViewModel model)
    {
        if (model.MaxScore < model.MinScore)
        {
            ModelState.AddModelError("", "Max score must be greater than or equal to min score.");
        }
    }

    private async Task PopulateMiniGamesAsync()
    {
        var games = await _context.MiniGames
            .OrderBy(game => game.DisplayOrder)
            .Select(game => new SelectListItem(game.Name, game.Id.ToString()))
            .ToListAsync();

        var images = await _context.UploadedMedia
            .Where(media => media.MediaType == MediaType.Image)
            .OrderBy(media => media.DisplayName)
            .Select(media => new SelectListItem(media.DisplayName, media.Id.ToString()))
            .ToListAsync();

        images.Insert(0, new SelectListItem("No result image", ""));

        ViewBag.MiniGames = games;
        ViewBag.ResultImages = images;
        ViewBag.MiniGameNames = games.ToDictionary(game => int.Parse(game.Value), game => game.Text);
    }
}
