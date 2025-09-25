using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ECommerceApp.RyanW84.Data.Models;

public class Category : BaseEntity
{
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Category name is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Category name must be between 1 and 100 characters")]
    public required string Name { get; set; } = null!;

    [Required(ErrorMessage = "Category description is required")]
    [StringLength(500, ErrorMessage = "Category description cannot exceed 500 characters")]
    public required string Description { get; set; } = null!;

    // Navigation property to Products
    public ICollection<Product> Products { get; set; } = new List<Product>();

    // Navigation property to Sales
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
}
