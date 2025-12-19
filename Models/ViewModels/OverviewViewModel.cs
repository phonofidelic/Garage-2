using System.ComponentModel.DataAnnotations;

namespace Garage_2.Models.ViewModels
{
    public class OverviewViewModel
    {
        public OverviewListItemViewModel OverviewItem { get; set; } = default!;

        public IEnumerable<OverviewListItemViewModel> OverviewList { get; set; } = default!;

        public OverviewSortBy? SortBy { get; set; }
    }

    public enum OverviewSortBy
    {
        RegistrationNumber,
        ArrivalTime,
        Type,
        ParkedTime
    }

    public class OverviewListItemViewModel
    {
        public int Id { get; set; }

        public VehicleType Type { get; set; }

        [Display(Name = "Registration number")]
        public string RegistrationNumber { get; set; } = default!;

        [Display(Name = "Arrival time")]
        public DateTime ArrivalTime { get; set; }

        [Display(Name = "Parked time")]
        public TimeSpan ParkedTime { get; set; }
    }
}
