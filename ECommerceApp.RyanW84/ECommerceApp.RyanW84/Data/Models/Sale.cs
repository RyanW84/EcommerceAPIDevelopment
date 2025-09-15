namespace ECommerceApp.RyanW84.Data.Models;

public class Sale
{
    public int SaleId { get; set; }
    public required DateTime SaleDate { get; set; }
    public required decimal TotalAmount { get; set; }
    public required int Quantity { get; set; }
    public required string CustomerName { get; set; } = null!;
    public required string CustomerEmail { get; set; } = null!;
    public required string CustomerAddress { get; set; } = null!;

    // Foreign key to Product
    public int ProductId { get; set; }

    // Navigation property to Product
    public Product Product { get; set; } = null!;

    // Foreign key to Category
    public int CategoryId { get; set; }

    // Navigation property to Category
    public Category Category { get; set; } = null!;
}
