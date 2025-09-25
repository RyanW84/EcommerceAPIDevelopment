using System.Collections.Generic;

namespace ECommerceApp.RyanW84.Data.Models;

public class Category : BaseEntity
{
    public int CategoryId { get; set; }
    public required string Name { get; set; } = null!;
    public required string Description { get; set; } = null!;

    // Navigation property to Products
    public ICollection<Product> Products { get; set; } = new List<Product>();

    // Navigation property to Sales
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
}
