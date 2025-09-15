using ECommerceApp.RyanW84.Data.Models;

namespace ECommerceApp.RyanW84.Data
{
	public class DbContext
	{

		private readonly List<Product> _products = new();
		private readonly List<Category> _categories = new();
		public List<Product> Products => _products;
		public List<Category> Categories => _categories;

		public DbContext()
		{
			_products = [];
			_categories = [];

			// Seed some initial data
			var Electronics = new Category
			{
				CategoryId = 1,
				Name = "Electronics",
				Description = "Electronic gadgets and devices"
			};
			var Clothing = new Category
			{
				CategoryId = 2,
				Name = "Clothing",
				Description = "Apparel and accessories"
			};
			var Books = new Category
			{
				CategoryId = 3,
				Name = "Books",
				Description = "Fiction and non-fiction books"
			};
			var HomeAppliances = new Category
            {
				CategoryId = 4,
				Name = "Home Appliances",
				Description = "Appliances for home use"
				};
            _categories.Add(Electronics);
            _categories.Add(Clothing);
			_categories.Add(Books);

            // Products
            var product1 = new Product
			{
				Id = 1,
				Name = "Smartphone",
				Description = "Latest model smartphone",
				Price = 699.99m,
				Quantity = 50,
				IsActive = true,
				CategoryId = Electronics.CategoryId,
				Category = Electronics
			};
			var product2 = new Product()
			{
				Id = 2,
				Name = "Jeans",
				Description = "Comfortable blue jeans",
				Price = 49.99m,
				Quantity = 100,
				IsActive = true,
				CategoryId = Clothing.CategoryId,
				Category = Clothing

			};
			var product3 = new Product()
			{
				Id = 3,
				Name = "Science Fiction Novel",
				Description = "A thrilling sci-fi adventure",
				Price = 19.99m,
				Quantity = 200,
				IsActive = true,
				CategoryId = Books.CategoryId,
				Category = Books
			};	
          

			_products.Add(product1);
			_products.Add(product2);
			_products.Add(product3);
		}
	}
}

