namespace ECommerceApp.RyanW84.Data.Models;

public class Product
	{
	public int ProductId { get; set; }
	public required string Name { get; set; } = null!;
	public required string Description { get; set; } = null!;
	public required decimal Price { get; set; }
	public required int Stock { get; set; }
	public required bool IsActive { get; set; }

	// Foreign key to Category
	public int CategoryId { get; set; }

	// Navigation property to Category (single)
	public Category Category { get; set; } = null!;

	// Navigation property to Sales
	public ICollection<Sale> Sales { get; set; } = new List<Sale>();
	}
