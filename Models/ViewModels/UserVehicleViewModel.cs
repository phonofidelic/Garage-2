using Garage_2.Models.Entities;

namespace Garage_2.Models.ViewModels;

public class UserVehicleViewModel
{
    public ApplicationUser User { get; }
    public IReadOnlyList<Vehicle> Vehicles { get; }

    public UserVehicleViewModel(ApplicationUser user, IReadOnlyList<Vehicle> vehicles)
    {
        User = user;
        Vehicles = vehicles;
    }
}
