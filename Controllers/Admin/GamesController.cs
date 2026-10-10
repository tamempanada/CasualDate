using CasualDate.Data;
using CasualDate.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CasualDate.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Route("admin/games")]
public class GamesController : Controller
{
    private readonly ApplicationDbContext _context;

    public GamesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var games = await _context.MiniGames
            .OrderBy(game => game.DisplayOrder)
            .ToListAsync();

        return View("~/Views/Admin/Games/Index.cshtml", games);
    }

    [HttpGet("edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var game = await _context.MiniGames.FindAsync(id);

        if (game is null)
        {
            return NotFound();
        }

        return View("~/Views/Admin/Games/Edit.cshtml", new MiniGameEditViewModel
        {
            Id = game.Id,
            Slug = game.Slug,
            Name = game.Name,
            Description = game.Description,
            DisplayOrder = game.DisplayOrder,
            IsEnabled = game.IsEnabled,
            IconLabel = game.IconLabel
        });
    }

    [HttpPost("edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, MiniGameEditViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View("~/Views/Admin/Games/Edit.cshtml", model);
        }

        var game = await _context.MiniGames.FindAsync(id);

        if (game is null)
        {
            return NotFound();
        }

        game.Name = model.Name.Trim();
        game.Description = model.Description.Trim();
        game.DisplayOrder = model.DisplayOrder;
        game.IsEnabled = model.IsEnabled;
        game.IconLabel = model.IconLabel.Trim();
        game.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        TempData["StatusMessage"] = "Game saved.";

        return RedirectToAction(nameof(Index));
    }
}
