using Microsoft.Data.SqlClient;
using System.ComponentModel.DataAnnotations;

namespace Garage_2.Models.ViewModels.ParkingSessions
{
    public class ParkingSessionsIndexViewModel
    {
        public string PageTitle { get => "Parking History"; }
        public ParkingSessionsListParameters ListParameters { get; set; } = default!;
        public List<ParkingSessionsListItemViewModel> ParkingSessionsList { get; set; } = [];
    }
}

