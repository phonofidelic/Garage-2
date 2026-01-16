using Microsoft.AspNetCore.Mvc.Rendering;

namespace Garage_2.Models.ViewModels;

public sealed class AdminAccessViewModel
{
    public IReadOnlyList<RoleItemViewModel> Roles { get; set; } = [];

    public IReadOnlyList<SelectListItem> AvailableRoles { get; set; } = [];

    public IReadOnlyList<UserRoleRowViewModel> Users { get; set; } = [];
}
public sealed class RoleItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsProtected { get; set; }
}

public sealed class UserRoleRowViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? UserName { get; set; }

    public string SelectedRole { get; set; } = string.Empty;

    public bool IsProtected { get; set; }
}