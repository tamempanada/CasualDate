using CasualDate.Models;
using CasualDate.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CasualDate.Controllers.Admin;

[Authorize(Roles = "SuperAdmin")]
[Route("admin/users")]
public class UsersController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UsersController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
        var playerUsers = await _userManager.GetUsersInRoleAsync("Player");
        var model = new List<AdminUserListItemViewModel>();

        foreach (var user in adminUsers.OrderBy(user => user.Email))
        {
            model.Add(new AdminUserListItemViewModel
            {
                Id = user.Id,
                Email = user.Email ?? user.UserName ?? "",
                AccountType = "Admin",
                IsSuperAdmin = await _userManager.IsInRoleAsync(user, "SuperAdmin")
            });
        }

        foreach (var user in playerUsers.OrderBy(user => user.Email))
        {
            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                continue;
            }

            model.Add(new AdminUserListItemViewModel
            {
                Id = user.Id,
                Email = user.Email ?? user.UserName ?? "",
                AccountType = "Player",
                IsSuperAdmin = false
            });
        }

        return View("~/Views/Admin/Users/Index.cshtml", model);
    }

    [HttpGet("create")]
    public IActionResult Create()
    {
        return View("~/Views/Admin/Users/Create.cshtml", new AdminUserCreateViewModel());
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminUserCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("~/Views/Admin/Users/Create.cshtml", model);
        }

        await EnsureRoleAsync("Player");
        await EnsureRoleAsync("Admin");
        await EnsureRoleAsync("SuperAdmin");

        var existingUser = await _userManager.FindByEmailAsync(model.Email);

        if (existingUser is not null)
        {
            ModelState.AddModelError("", "That email already has an account.");
            return View("~/Views/Admin/Users/Create.cshtml", model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email.Trim(),
            Email = model.Email.Trim(),
            EmailConfirmed = true
        };

        var createResult = await _userManager.CreateAsync(user, model.Password);

        if (!createResult.Succeeded)
        {
            AddIdentityErrors(createResult);
            return View("~/Views/Admin/Users/Create.cshtml", model);
        }

        if (model.AccountType == "Admin")
        {
            await _userManager.AddToRoleAsync(user, "Admin");

            if (model.IsSuperAdmin)
            {
                await _userManager.AddToRoleAsync(user, "SuperAdmin");
            }
        }
        else
        {
            await _userManager.AddToRoleAsync(user, "Player");
        }

        TempData["StatusMessage"] = model.AccountType == "Admin"
            ? "Admin account created."
            : "Player account created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("toggle-superadmin/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleSuperAdmin(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user is null)
        {
            return NotFound();
        }

        var currentUserId = _userManager.GetUserId(User);

        if (user.Id == currentUserId)
        {
            TempData["StatusMessage"] = "You cannot remove SuperAdmin from your own account.";
            return RedirectToAction(nameof(Index));
        }

        if (await _userManager.IsInRoleAsync(user, "SuperAdmin"))
        {
            await _userManager.RemoveFromRoleAsync(user, "SuperAdmin");
            TempData["StatusMessage"] = "SuperAdmin removed.";
        }
        else
        {
            await EnsureRoleAsync("SuperAdmin");
            await _userManager.AddToRoleAsync(user, "SuperAdmin");
            TempData["StatusMessage"] = "SuperAdmin added.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task EnsureRoleAsync(string roleName)
    {
        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            await _roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    private void AddIdentityErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }
    }
}
