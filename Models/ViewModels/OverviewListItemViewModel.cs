using System.ComponentModel.DataAnnotations;

namespace Garage_2.Models.ViewModels
{
    public class OverviewListItemViewModel
    {
        public int Id { get; set; }

        public string Type { get; set; } = default!;

        [Display(Name = "Registration number")]
        public string RegistrationNumber { get; set; } = default!;

        [Display(Name = "Parking start time")]
        public DateTime? ArrivalTime { get; set; }

        [Display(Name = "Parked time")]
        public TimeSpan? ParkedTime { get; set; }

        public string ParkingSpots { get; set; } = "-"; // Tex "12", "12, 13", "12–14"
    }
}
