using System.ComponentModel.DataAnnotations;

namespace Garage_2.Models.ViewModels.ParkingSessions
{
    public class ParkingSessionsIndexViewModel
    {
        public string PageTitle { get => "Parking History"; }

        public bool IsAdmin { get; set; } = false;

        public ParkingSessionsListParameters ListParameters { get; set; } = default!;

        public IEnumerable<ParkingSessionsListItemViewModel> ParkingSessionsList { get; set; } = [];

        [DataType(DataType.Currency)]
        public decimal CurrentTotal { get; set; }
    }
}

