using Microsoft.Data.SqlClient;

namespace Garage_2.Models.ViewModels.ParkingSessions
{
    public class ParkingSessionsListParameters : ParkingSessionsListItemViewModel
    {
        public ParkingSessionsSortBy SortBy { get; set; }

        public SortOrder SortOrder { get; set; }
    }
}
