using System.ComponentModel.DataAnnotations;

namespace Garage_2.Models.ViewModels
{
    public class OverviewViewModel
    {
        public OverviewListItemViewModel OverviewItem { get; set; } = default!;

        public IEnumerable<OverviewListItemViewModel> OverviewList { get; set; } = default!;

        public OverviewSortBy? SortBy { get; set; }

        public OverviewSortOrder? SortOrder { get; set; }

        public int Count { get; set; }

        public string? SearchString { get; set; }

        public string SearchField { get; set; } = string.Empty;

        public int TotalPages { get; set; }

        public int CurrentPage { get; set; }
    }
}
