namespace Garage_2.Models.ViewModels;

public class UserOverviewViewModel
{
    public UserOverviewListItemViewModel OverviewItem { get; set; } = new();

    public IEnumerable<UserOverviewListItemViewModel> OverviewList { get; set; }
        = Enumerable.Empty<UserOverviewListItemViewModel>();

    public UserOverviewSortBy? SortBy { get; set; }
    public UserOverviewSortOrder? SortOrder { get; set; }

    public int Count { get; set; }

    public string? SearchString { get; set; }
    public string SearchField { get; set; } = string.Empty;

    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
}
