namespace Garage_2.Models.ViewModels.Vehicles
{
    public class VehiclesListViewModel
    {
        public IEnumerable<VehiclesListItem> VehiclesList { get; set; } = [];

        public VehiclesListSortBy? SortBy { get; set; }

        public OverviewSortOrder MyProperty { get; set; }

        public int Count { get; set; }

        public string? SearchString { get; set; }

        public string SearchField { get; set; } = string.Empty;

        public int TotalPages { get; set; }

        public int CurrentPage { get; set; }
    }
}
