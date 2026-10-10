using CasualDate.Models;
using CasualDate.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CasualDate.Controllers.Admin;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("admin/login")]
    public IActionResult Login(string? returnUrl = null)
    {
        return View("~/Views/Admin/Account/Login.cshtml", new AdminLoginViewModel
        {
            ReturnUrl = returnUrl
        });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [Route("admin/login")]
    public async Task<IActionResult> Login(AdminLoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("~/Views/Admin/Account/Login.cshtml", model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);

        if (user is null || !await _userManager.IsInRoleAsync(user, "Admin"))
        {
            ModelState.AddModelError(string.Empty, "Invalid admin login.");
            return View("~/Views/Admin/Account/Login.cshtml", model);
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

            return Redirect("/admin");
        }

        ModelState.AddModelError(string.Empty, "Invalid admin login.");
        return View("~/Views/Admin/Account/Login.cshtml", model);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    [Route("admin/logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("admin/access-denied")]
    public IActionResult AccessDenied()
    {
        return View("~/Views/Admin/Account/AccessDenied.cshtml");
    }
}
