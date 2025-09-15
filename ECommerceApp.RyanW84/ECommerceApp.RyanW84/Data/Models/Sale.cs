namespace ECommerceApp.RyanW84.Data.Models;

public class Sale: BaseModel
	{
	public new int Id { get; set; } // 'new' keyword to hide the inherited Id property
	public required int ProductId { get; set; }
	public required int QuantitySold { get; set; }
	public required decimal TotalPrice { get; set; }
	public required DateTime SaleDate { get; set; }

	// Navigation property
	public required Product Product { get; set; }
	}
