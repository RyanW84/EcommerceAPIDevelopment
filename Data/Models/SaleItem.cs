namespace ECommerceApp.RyanW84.Data.Models;

public class SaleItem
{
    public int SaleId { get; set; }
    public Sale Sale { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public decimal LineTotal => UnitPrice * Quantity;
}
