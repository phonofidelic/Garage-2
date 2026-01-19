using Garage_2.Models.Entities;

namespace Garage_2.Models.ViewModels;

public class UserDetailsViewModel
{
    public string UserId { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string SSN { get; set; } = default!;
    public List<UserVehicleViewModel> Vehicles { get; set; } = new();
    public decimal TotalActiveCostNow { get; set; }
}
