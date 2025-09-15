namespace ECommerceApp.RyanW84.Data.Models;

public class Products
	{
	public int Id {  get; set; }
	public string Name { get; set; }
	public string Description { get; set; }
	public decimal Price { get; set; }
	public int Quantity { get; set; }

	public bool IsActive { get; set; }

		}
