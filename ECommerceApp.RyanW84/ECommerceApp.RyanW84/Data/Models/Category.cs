namespace ECommerceApp.RyanW84.Data.Models
{
	public class Category : BaseModel
	{
		public int CategoryId { get; set; }
		public required string Name { get; set; }
		public required string Description { get; set; }


		// Navigation property
		public Product? product { get; set; }

	}
}
