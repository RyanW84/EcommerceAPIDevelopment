using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ECommerceApp.RyanW84.Data.Models;

public class Product : BaseEntity
{
    public int ProductId { get; set; }

    [Required(ErrorMessage = "Product name is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Product name must be between 1 and 100 characters")]
    public required string Name { get; set; } = null!;

    [Required(ErrorMessage = "Product description is required")]
    [StringLength(500, ErrorMessage = "Product description cannot exceed 500 characters")]
    public required string Description { get; set; } = null!;

    [Required(ErrorMessage = "Product price is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Product price must be greater than 0")]
    public required decimal Price { get; set; }

    [Required(ErrorMessage = "Stock quantity is required")]
    [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative")]
    public required int Stock { get; set; }

    public required bool IsActive { get; set; }

    [Required(ErrorMessage = "Category is required")]
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    // now hold sale items (join)
    public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
}
