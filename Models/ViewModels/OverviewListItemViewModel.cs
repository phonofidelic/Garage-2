using System.ComponentModel.DataAnnotations;

namespace Garage_2.Models.ViewModels
{
    public class OverviewListItemViewModel
    {
        public int Id { get; set; }

        public string Type { get; set; } = default!;

        [Display(Name = "Registration number")]
        public string RegistrationNumber { get; set; } = default!;

        [Display(Name = "Arrival time")]
        public DateTime? ArrivalTime { get; set; }

        [Display(Name = "Parked time")]
        public TimeSpan? ParkedTime { get; set; }
    }
}
