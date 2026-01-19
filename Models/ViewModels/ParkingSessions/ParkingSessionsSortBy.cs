using System.ComponentModel.DataAnnotations;

namespace Garage_2.Models.ViewModels.ParkingSessions
{
    public enum ParkingSessionsSortBy
    {
        [Display(Name = "Vehicle type")]
        VehicleType,

        [Display(Name = "Vehicle owner")]
        VehicleOwner,

        [Display(Name = "Registration number")]
        RegistrationNumber,

        [Display(Name = "Start time")]
        ArrivalTime,

        [Display(Name = "Parked time")]
        ParkedTime,

        [Display(Name = "Checkout time")]
        DepartureTime,

        [Display(Name = "Cost")]
        CurrentCost
    }
}
