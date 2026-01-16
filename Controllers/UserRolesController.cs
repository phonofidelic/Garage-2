using Garage_2.Models.Entities;
using Garage_2.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Garage_2.Controllers;

[Authorize(Roles = "Admin")]
public class UserRolesController : Controller
{
    private const string AdminRoleName = "Admin";
    private const string DefaultRoleName = "User";

    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserRolesController(
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var roleNames = _roleManager.Roles
            .OrderBy(r => r.Name)
            .Select(r => r.Name)
            .ToList();

        var roles = _roleManager.Roles
            .OrderBy(r => r.Name)
            .Select(r => new RoleItemViewModel
            {
                Id = r.Id,
                Name = r.Name!,
                IsProtected = string.Equals(r.Name, AdminRoleName, StringComparison.Ordinal) || string.Equals(r.Name, DefaultRoleName, StringComparison.Ordinal)
            })
            .ToList();

        var availableRoles = roleNames
            .Select(n => new SelectListItem { Value = n, Text = n })
            .ToList();

        var users = _userManager.Users
            .OrderBy(u => u.Email)
            .ToList();

        var userRows = new List<UserRoleRowViewModel>(users.Count);

        foreach (var user in users)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            var selectedRole = userRoles.FirstOrDefault();

            if (string.IsNullOrWhiteSpace(selectedRole))
            {
                selectedRole = roleNames.Contains(DefaultRoleName)
                    ? DefaultRoleName
                    : roleNames.FirstOrDefault() ?? string.Empty;
            }

            bool isProtectedAdmin = userRoles.Contains(AdminRoleName);

            userRows.Add(new UserRoleRowViewModel
            {
                UserId = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                SelectedRole = selectedRole,
                IsProtected = isProtectedAdmin
            });
        }

        var vm = new AdminAccessViewModel
        {
            Roles = roles,
            AvailableRoles = availableRoles,
            Users = userRows
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string name)
    {
        name = (name ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            TempData["AlertType"] = "warning";
            TempData["AlertMessage"] = "Role name is required.";
            return RedirectToAction(nameof(Index));
        }

        if (await _roleManager.RoleExistsAsync(name))
        {
            TempData["AlertType"] = "warning";
            TempData["AlertMessage"] = $"Role '{name}' already exists.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _roleManager.CreateAsync(new IdentityRole(name));
        if (!result.Succeeded)
        {
            TempData["AlertType"] = "danger";
            TempData["AlertMessage"] = string.Join(", ", result.Errors.Select(e => e.Description));
            return RedirectToAction(nameof(Index));
        }

        TempData["AlertType"] = "success";
        TempData["AlertMessage"] = $"Role '{name}' created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            TempData["AlertType"] = "warning";
            TempData["AlertMessage"] = "Role not found.";
            return RedirectToAction(nameof(Index));
        }

        var role = await _roleManager.FindByIdAsync(id);
        if (role is null)
        {
            TempData["AlertType"] = "warning";
            TempData["AlertMessage"] = "Role not found.";
            return RedirectToAction(nameof(Index));
        }

        if (string.Equals(role.Name, AdminRoleName, StringComparison.Ordinal))
        {
            TempData["AlertType"] = "warning";
            TempData["AlertMessage"] = "The Admin role cannot be deleted.";
            return RedirectToAction(nameof(Index));
        }

        if (string.Equals(role.Name, DefaultRoleName, StringComparison.Ordinal))
        {
            TempData["AlertType"] = "warning";
            TempData["AlertMessage"] = "The User role cannot be deleted.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _roleManager.DeleteAsync(role);
        if (!result.Succeeded)
        {
            TempData["AlertType"] = "danger";
            TempData["AlertMessage"] = string.Join(", ", result.Errors.Select(e => e.Description));
            return RedirectToAction(nameof(Index));
        }

        TempData["AlertType"] = "success";
        TempData["AlertMessage"] = $"Role '{role.Name}' deleted.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateUserRole(string userId, string roleName)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(roleName))
        {
            TempData["AlertType"] = "warning";
            TempData["AlertMessage"] = "Invalid role update request.";
            return RedirectToAction(nameof(Index));
        }

        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            TempData["AlertType"] = "warning";
            TempData["AlertMessage"] = "Role not found.";
            return RedirectToAction(nameof(Index));
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            TempData["AlertType"] = "warning";
            TempData["AlertMessage"] = "User not found.";
            return RedirectToAction(nameof(Index));
        }

        var currentRoles = await _userManager.GetRolesAsync(user);

        //bool isProtectedAdmin = currentRoles.Contains(AdminRoleName);

        //if (isProtectedAdmin && !string.Equals(roleName, AdminRoleName, StringComparison.Ordinal))
        //{
        //    TempData["AlertType"] = "warning";
        //    TempData["AlertMessage"] = "Admin users cannot be demoted.";
        //    return RedirectToAction(nameof(Index));
        //}

        if (currentRoles.Count > 0)
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                TempData["AlertType"] = "danger";
                TempData["AlertMessage"] = string.Join(", ", removeResult.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(Index));
            }
        }

        var addResult = await _userManager.AddToRoleAsync(user, roleName);
        if (!addResult.Succeeded)
        {
            TempData["AlertType"] = "danger";
            TempData["AlertMessage"] = string.Join(", ", addResult.Errors.Select(e => e.Description));
            return RedirectToAction(nameof(Index));
        }

        TempData["AlertType"] = "success";
        TempData["AlertMessage"] = $"Updated role for '{user.Email}' to '{roleName}'.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            TempData["AlertType"] = "warning";
            TempData["AlertMessage"] = "User not found.";
            return RedirectToAction(nameof(Index));
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            TempData["AlertType"] = "warning";
            TempData["AlertMessage"] = "User not found.";
            return RedirectToAction(nameof(Index));
        }

        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Contains(AdminRoleName))
        {
            TempData["AlertType"] = "warning";
            TempData["AlertMessage"] = "Admin users cannot be deleted.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            TempData["AlertType"] = "danger";
            TempData["AlertMessage"] = string.Join(", ", result.Errors.Select(e => e.Description));
            return RedirectToAction(nameof(Index));
        }

        TempData["AlertType"] = "success";
        TempData["AlertMessage"] = $"User '{user.Email}' deleted.";
        return RedirectToAction(nameof(Index));
    }
}