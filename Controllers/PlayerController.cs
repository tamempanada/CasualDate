using System.Data.Common;
using CasualDate.Data;
using CasualDate.Models;
using CasualDate.Services;
using CasualDate.ViewModels.Player;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CasualDate.Controllers;

[Route("player")]
public class PlayerController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly PlayerProfileService _playerProfileService;
    private readonly TokenWalletService _tokenWalletService;

    public PlayerController(
        ApplicationDbContext context,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        PlayerProfileService playerProfileService,
        TokenWalletService tokenWalletService)
    {
        _context = context;
        _signInManager = signInManager;
        _userManager = userManager;
        _playerProfileService = playerProfileService;
        _tokenWalletService = tokenWalletService;
    }

    [HttpGet("login")]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        return View("~/Views/Player/Login.cshtml", new PlayerLoginViewModel
        {
            ReturnUrl = returnUrl
        });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(PlayerLoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("~/Views/Player/Login.cshtml", model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);

        if (user is null || !await _userManager.IsInRoleAsync(user, "Player"))
        {
            ModelState.AddModelError(string.Empty, "Invalid player login.");
            return View("~/Views/Player/Login.cshtml", model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            user,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: false);

        if (result.Succeeded)
        {
            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Menu", "Home");
        }

        ModelState.AddModelError(string.Empty, "Invalid player login.");
        return View("~/Views/Player/Login.cshtml", model);
    }

    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet("wallet")]
    public async Task<IActionResult> Wallet()
    {
        try
        {
            var player = await _playerProfileService.GetOrCreateAsync();
            var transactions = await _context.TokenTransactions
                .Where(transaction => transaction.PlayerProfileId == player.Id)
                .OrderByDescending(transaction => transaction.CreatedAt)
                .Take(40)
                .Select(transaction => new PlayerWalletTransactionViewModel
                {
                    Amount = transaction.Amount,
                    BalanceAfter = transaction.BalanceAfter,
                    TransactionType = transaction.TransactionType,
                    Description = transaction.Description,
                    CreatedAt = transaction.CreatedAt
                })
                .ToListAsync();

            return View("~/Views/Player/Wallet.cshtml", new PlayerWalletViewModel
            {
                DisplayName = player.DisplayName,
                TokenBalance = player.TokenBalance,
                Transactions = transactions
            });
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToAction(nameof(Login), new { returnUrl = "/player/wallet" });
        }
    }

    [HttpGet("wallet-summary")]
    public async Task<IActionResult> WalletSummary()
    {
        try
        {
            var snapshot = await _tokenWalletService.GetSnapshotAsync();
            return Json(new { balance = snapshot.Balance, signedIn = true });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { balance = 0, signedIn = false, message = "Player login required." });
        }
        catch (DbException)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                balance = 0,
                signedIn = true,
                message = "Database migration required for player wallet."
            });
        }
        catch (PlayerDataUnavailableException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                balance = 0,
                signedIn = true,
                message = ex.Message
            });
        }
    }

}
