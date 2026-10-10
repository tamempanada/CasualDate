using CasualDate.Models;
using Microsoft.AspNetCore.Identity;

namespace CasualDate.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        const string adminRole = "Admin";
        const string superAdminRole = "SuperAdmin";

        if (!await roleManager.RoleExistsAsync(adminRole))
        {
            await roleManager.CreateAsync(new IdentityRole(adminRole));
        }

        if (!await roleManager.RoleExistsAsync(superAdminRole))
        {
            await roleManager.CreateAsync(new IdentityRole(superAdminRole));
        }

        var email = configuration["SeedAdmin:Email"];
        var password = configuration["SeedAdmin:Password"];

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password) ||
            password == "CHANGE_ME")
        {
            return;
        }

        var existingUser = await userManager.FindByEmailAsync(email);

        if (existingUser is null)
        {
            existingUser = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(existingUser, password);

            if (!createResult.Succeeded)
            {
                var errors = string.Join("; ", createResult.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"Could not create seed admin user: {errors}");
            }
        }
        else
        {
            existingUser.UserName = email;
            existingUser.Email = email;
            existingUser.EmailConfirmed = true;

            var updateResult = await userManager.UpdateAsync(existingUser);

            if (!updateResult.Succeeded)
            {
                var errors = string.Join("; ", updateResult.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"Could not update seed admin user: {errors}");
            }
        }

        existingUser = await userManager.FindByEmailAsync(email)
            ?? throw new InvalidOperationException("Seed admin user could not be loaded after create/update.");

        if (!await userManager.CheckPasswordAsync(existingUser, password))
        {
            if (await userManager.HasPasswordAsync(existingUser))
            {
                var removeResult = await userManager.RemovePasswordAsync(existingUser);

                if (!removeResult.Succeeded)
                {
                    var errors = string.Join("; ", removeResult.Errors.Select(error => error.Description));
                    throw new InvalidOperationException($"Could not reset seed admin password: {errors}");
                }
            }

            var addPasswordResult = await userManager.AddPasswordAsync(existingUser, password);

            if (!addPasswordResult.Succeeded)
            {
                var errors = string.Join("; ", addPasswordResult.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"Could not set seed admin password: {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(existingUser, adminRole))
        {
            await userManager.AddToRoleAsync(existingUser, adminRole);
        }

        if (!await userManager.IsInRoleAsync(existingUser, superAdminRole))
        {
            await userManager.AddToRoleAsync(existingUser, superAdminRole);
        }
    }
}
