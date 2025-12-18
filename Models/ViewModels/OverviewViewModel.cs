using System.ComponentModel.DataAnnotations;

namespace Garage_2.Models.ViewModels
{
    public class OverviewViewModel
    {

        public int Id { get; set; }

        public VehicleType Type { get; set; }

        [Display(Name = "Registration number")]
        public string RegistrationNumber { get; set; }

        [Display(Name = "Arrival time")]
        public DateTime ArrivalTime { get; set; }

        [Display(Name = "Parked time")]
        public TimeSpan ParkedTime { get; set; }

    }
}
