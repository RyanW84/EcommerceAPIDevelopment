namespace ECommerceApp.RyanW84.Data.Models;

public class Product : BaseModel
{
	public new int Id { get; set; } // 'new' keyword to hide the inherited Id property
    public required string Name { get; set; }
	public required string Description { get; set; }
	public required decimal Price { get; set; }
	public required int Quantity { get; set; }

	public required bool IsActive { get; set; }

	// Foreign key
	public required int CategoryId { get; set; }
	// Navigation property
	public required Category Category { get; set; }

}
