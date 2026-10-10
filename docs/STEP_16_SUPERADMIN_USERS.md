# Step 16: SuperAdmin User Management

## What Was Added

- New `SuperAdmin` role.
- The seeded account is assigned to both `Admin` and `SuperAdmin`.
- Protected admin page at `/admin/users`.
- SuperAdmins can create new admin accounts.
- SuperAdmins can promote or demote other admins from `SuperAdmin`.
- A user cannot remove `SuperAdmin` from their own account.
- Dashboard link to Admin Users.

## Files Added

```txt
Controllers/Admin/UsersController.cs
ViewModels/Admin/AdminUserCreateViewModel.cs
ViewModels/Admin/AdminUserListItemViewModel.cs
Views/Admin/Users/Index.cshtml
Views/Admin/Users/Create.cshtml
docs/STEP_16_SUPERADMIN_USERS.md
```

## Files Updated

```txt
Data/SeedData.cs
Views/Admin/Dashboard/Index.cshtml
docs/NEXT_STEPS.md
```

## Notes

This patch does not require a migration because ASP.NET Identity roles and users already exist.

Restart the app once after this patch so seeding can add the `SuperAdmin` role to the seeded admin account.

## Recommended Later Improvements

- Add delete/deactivate admin account support.
- Add password reset from SuperAdmin.
- Add separate permissions for content editors versus full admins.
- Add player/friend accounts later, separate from admin accounts.
