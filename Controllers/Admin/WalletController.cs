using CasualDate.Data;
using CasualDate.Services;
using CasualDate.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CasualDate.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Route("admin/wallet")]
public class WalletController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly TokenWalletService _walletService;

    public WalletController(ApplicationDbContext context, TokenWalletService walletService)
    {
        _context = context;
        _walletService = walletService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        return View("~/Views/Admin/Wallet/Index.cshtml", await BuildViewModelAsync());
    }

    [HttpPost("adjust")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Adjust([Bind(Prefix = "Adjustment")] WalletAdjustmentViewModel adjustment)
    {
        if (adjustment.Amount == 0)
        {
            ModelState.AddModelError(nameof(adjustment.Amount), "Adjustment cannot be zero.");
        }

        if (!ModelState.IsValid)
        {
            return View("~/Views/Admin/Wallet/Index.cshtml", await BuildViewModelAsync(adjustment));
        }

        var player = await _context.PlayerProfiles.FindAsync(adjustment.PlayerProfileId);
        if (player is null)
        {
            return NotFound();
        }

        try
        {
            await _walletService.AdjustBalanceAsync(
                player,
                adjustment.Amount,
                "ManualAdjustment",
                $"manual:{DateTime.UtcNow:yyyyMMddHHmmss}",
                adjustment.Reason.Trim());
            await _context.SaveChangesAsync();
            TempData["StatusMessage"] = "Wallet adjusted.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["StatusMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<WalletAdminViewModel> BuildViewModelAsync(WalletAdjustmentViewModel? adjustment = null)
    {
        var players = await (
            from profile in _context.PlayerProfiles
            join user in _context.Users on profile.ApplicationUserId equals user.Id
            orderby profile.UpdatedAt descending
            select new WalletPlayerViewModel
            {
                PlayerProfileId = profile.Id,
                Email = user.Email ?? user.UserName ?? profile.DisplayName,
                DisplayName = profile.DisplayName,
                TokenBalance = profile.TokenBalance,
                UpdatedAt = profile.UpdatedAt
            })
            .ToListAsync();

        var transactions = await (
            from transaction in _context.TokenTransactions
            join profile in _context.PlayerProfiles on transaction.PlayerProfileId equals profile.Id
            join user in _context.Users on profile.ApplicationUserId equals user.Id
            orderby transaction.CreatedAt descending
            select new WalletTransactionViewModel
            {
                PlayerEmail = user.Email ?? user.UserName ?? profile.DisplayName,
                Amount = transaction.Amount,
                BalanceAfter = transaction.BalanceAfter,
                TransactionType = transaction.TransactionType,
                ReferenceKey = transaction.ReferenceKey,
                Description = transaction.Description,
                CreatedAt = transaction.CreatedAt
            })
            .Take(80)
            .ToListAsync();

        return new WalletAdminViewModel
        {
            Players = players,
            Transactions = transactions,
            Adjustment = adjustment ?? new WalletAdjustmentViewModel()
        };
    }
}
