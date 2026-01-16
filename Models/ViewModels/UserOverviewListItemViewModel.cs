using System.ComponentModel.DataAnnotations;

namespace Garage_2.Models.ViewModels;

public class UserOverviewListItemViewModel
{
    public string UserId { get; set; } = default!;

    [Display(Name = "Name")]
    public string FullName { get; set; } = default!;
    public string? Email { get; set; }

    [Display(Name = "Registered vehicle(s)")]
    public int VehicleCount { get; set; }
    public decimal TotalActiveCostNow { get; set; }
}
