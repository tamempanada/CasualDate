using CasualDate.Data;
using CasualDate.Models;
using CasualDate.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CasualDate.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Route("admin/messages")]
public class MessagesController : Controller
{
    private readonly ApplicationDbContext _context;

    public MessagesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var messages = await _context.Messages
            .OrderBy(message => message.DisplayOrder)
            .ThenBy(message => message.Key)
            .ToListAsync();

        return View("~/Views/Admin/Messages/Index.cshtml", messages);
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create()
    {
        await PopulateMiniGamesAsync();
        return View("~/Views/Admin/Messages/Edit.cshtml", new MessageEditViewModel());
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MessageEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateMiniGamesAsync();
            return View("~/Views/Admin/Messages/Edit.cshtml", model);
        }

        _context.Messages.Add(new Message
        {
            MiniGameId = model.MiniGameId,
            Key = model.Key.Trim(),
            Title = model.Title.Trim(),
            Body = model.Body.Trim(),
            ButtonText = model.ButtonText.Trim(),
            DisplayOrder = model.DisplayOrder
        });

        await _context.SaveChangesAsync();
        TempData["StatusMessage"] = "Message created.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var message = await _context.Messages.FindAsync(id);

        if (message is null)
        {
            return NotFound();
        }

        await PopulateMiniGamesAsync();
        return View("~/Views/Admin/Messages/Edit.cshtml", new MessageEditViewModel
        {
            Id = message.Id,
            MiniGameId = message.MiniGameId,
            Key = message.Key,
            Title = message.Title,
            Body = message.Body,
            ButtonText = message.ButtonText,
            DisplayOrder = message.DisplayOrder
        });
    }

    [HttpPost("edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, MessageEditViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await PopulateMiniGamesAsync();
            return View("~/Views/Admin/Messages/Edit.cshtml", model);
        }

        var message = await _context.Messages.FindAsync(id);

        if (message is null)
        {
            return NotFound();
        }

        message.MiniGameId = model.MiniGameId;
        message.Key = model.Key.Trim();
        message.Title = model.Title.Trim();
        message.Body = model.Body.Trim();
        message.ButtonText = model.ButtonText.Trim();
        message.DisplayOrder = model.DisplayOrder;
        message.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        TempData["StatusMessage"] = "Message saved.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var message = await _context.Messages.FindAsync(id);

        if (message is not null)
        {
            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();
            TempData["StatusMessage"] = "Message deleted.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateMiniGamesAsync()
    {
        var games = await _context.MiniGames
            .OrderBy(game => game.DisplayOrder)
            .Select(game => new SelectListItem(game.Name, game.Id.ToString()))
            .ToListAsync();

        games.Insert(0, new SelectListItem("General message", ""));
        ViewBag.MiniGames = games;
    }
}
