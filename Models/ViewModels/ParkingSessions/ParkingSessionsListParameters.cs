using Microsoft.Data.SqlClient;

namespace Garage_2.Models.ViewModels.ParkingSessions
{
    // ToDo: Reverse order of inheritance? 
    // (so that different kinds of list views can share helper methods currently defined in ParkingSessionsExtensions)
    public class ParkingSessionsListParameters : ParkingSessionsListItemViewModel
    {
        public ParkingSessionsSortBy SortBy { get; set; }

        public SortOrder SortOrder { get; set; }
        
        public int PageLimit { get; set; }

        public int CurrentPage { get; set; }

        public int TotalPages { get; set; }

        public int ItemsCount { get; set; }

        public int FilteredItemsCount { get; set; }
    }
}
