using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ECommerceApp.RyanW84.Data.Models;

public class Sale
{
    public int SaleId { get; set; }

    [Required(ErrorMessage = "Sale date is required")]
    public required DateTime SaleDate { get; set; }

    [Required(ErrorMessage = "Total amount is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Total amount must be greater than 0")]
    public required decimal TotalAmount { get; set; }

    [Required(ErrorMessage = "Customer name is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Customer name must be between 1 and 100 characters")]
    public required string CustomerName { get; set; } = null!;

    [Required(ErrorMessage = "Customer email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address format")]
    [StringLength(100, ErrorMessage = "Customer email cannot exceed 100 characters")]
    public required string CustomerEmail { get; set; } = null!;

    [Required(ErrorMessage = "Customer address is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Customer address must be between 1 and 200 characters")]
    public required string CustomerAddress { get; set; } = null!;

    // sale has many items (each item references a product)
    public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
    public ICollection<Category> Categories { get; set; } = new List<Category>();
}
