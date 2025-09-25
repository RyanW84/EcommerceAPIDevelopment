using System.ComponentModel.DataAnnotations;

namespace ECommerceApp.RyanW84.Data.DTO;

public class SaleQueryParameters
{
    private const int MaxPageSize = 100;

    private int _page = 1;
    private int _pageSize = 10;

    [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than or equal to 1")]
    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    [Range(1, MaxPageSize, ErrorMessage = "Page size must be between 1 and 100")]
    public int PageSize
    {
        get => _pageSize;
        set
        {
            if (value < 1) _pageSize = 10;
            else _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }
    }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    [StringLength(100, ErrorMessage = "Customer name filter cannot exceed 100 characters")]
    public string? CustomerName { get; set; }

    [EmailAddress(ErrorMessage = "Customer email filter must be a valid email address")]
    public string? CustomerEmail { get; set; }

    [StringLength(50, ErrorMessage = "Sort field cannot exceed 50 characters")]
    public string? SortBy { get; set; }

    [StringLength(4, ErrorMessage = "Sort direction must be 'asc' or 'desc'")]
    public string? SortDirection { get; set; }
}
