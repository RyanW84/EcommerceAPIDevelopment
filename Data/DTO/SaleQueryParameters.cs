namespace ECommerceApp.RyanW84.Data.DTO;

public class SaleQueryParameters
{
    private const int MaxPageSize = 100;

    private int _page = 1;
    private int _pageSize = 10;

    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }
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
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }
}
