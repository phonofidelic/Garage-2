using System.ComponentModel.DataAnnotations;

namespace Garage_2.Models.ViewModels.Vehicles
{
    public enum VehiclesListSortBy
    {
        [Display(Name = "Vehicle Registration")]
        RegistrationNumber,
        Type
    }
}