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
        StartTime,
        
        [Display(Name = "Parked time")]
        Duration,

        [Display(Name = "Checkout time")]
        CheckoutTime,

        [Display(Name = "Cost")]
        CurrentCost
    }
}
