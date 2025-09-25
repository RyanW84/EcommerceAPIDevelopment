using System.ComponentModel.DataAnnotations;

namespace ECommerceApp.RyanW84.Data.DTO;

public class CategoryQueryParameters
{
    [StringLength(100, ErrorMessage = "Search term cannot exceed 100 characters")]
    public string? Search { get; set; }

    [StringLength(50, ErrorMessage = "Sort field cannot exceed 50 characters")]
    public string? SortBy { get; set; }

    [StringLength(4, ErrorMessage = "Sort direction must be 'asc' or 'desc'")]
    public string? SortDirection { get; set; }

    public bool IncludeDeleted { get; set; }
}
