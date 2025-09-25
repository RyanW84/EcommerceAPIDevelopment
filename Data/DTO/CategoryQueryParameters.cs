namespace ECommerceApp.RyanW84.Data.DTO;

public class CategoryQueryParameters
{
    public string? Search { get; set; }
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }

    public bool IncludeDeleted { get; set; }
}
