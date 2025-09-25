namespace ECommerceApp.ConsoleClient.Models;

public class ApiResponse<T>
{
    public bool RequestFailed { get; set; }
    public int ResponseCode { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public T? Data { get; set; }
}

public class PaginatedResponse<T>
{
    public bool RequestFailed { get; set; }
    public int ResponseCode { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public T? Data { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
}

public class Product
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = new();
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public class Category
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public class Sale
{
    public int SaleId { get; set; }
    public DateTime SaleDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerAddress { get; set; } = string.Empty;
    public List<SaleItem> SaleItems { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
}

public class SaleItem
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public Product? Product { get; set; }
}

public record ProductQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    int? CategoryId = null,
    string? SortBy = null,
    string? SortDirection = null
);

public record CategoryQuery(
    string? Search = null,
    string? SortBy = null,
    string? SortDirection = null,
    bool IncludeDeleted = false
);

public record SaleQuery(
    int Page = 1,
    int PageSize = 10,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    string? CustomerName = null,
    string? CustomerEmail = null,
    string? SortBy = null,
    string? SortDirection = null
);

public class ApiRequest<T> where T : class
{
    public ApiRequest(T payload) => Payload = payload;

    public T Payload { get; set; }
}
