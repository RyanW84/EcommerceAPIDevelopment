using ECommerceApp.RyanW84.Data.Configurations;
using ECommerceApp.RyanW84.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.RyanW84.Data;

public class ECommerceDbContext(DbContextOptions<ECommerceDbContext> options) : DbContext(options)
{
    // DbSets for the entities
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Sale> Sales { get; set; } = null!;
    public DbSet<SaleItem> SaleItems { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.ProductId);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Description).IsRequired().HasMaxLength(500);
            entity.Property(p => p.Price).IsRequired().HasPrecision(18, 2);
            entity.Property(p => p.Stock).IsRequired().HasDefaultValue(0);
            entity.Property(p => p.IsActive).IsRequired().HasDefaultValue(true);
            entity.Property(p => p.CategoryId).IsRequired();
            entity.HasIndex(p => p.Name).HasDatabaseName("IX_Products_Name");

            entity
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasMany(p => p.SaleItems)
                .WithOne(si => si.Product)
                .HasForeignKey(si => si.ProductId)
                .OnDelete(DeleteBehavior.NoAction); // choose behavior you want
        });

        modelBuilder.Entity<Category>()
      .HasMany(c => c.Products)
      .WithOne(p => p.Category)
      .HasForeignKey(p => p.CategoryId)
      .OnDelete(DeleteBehavior.Cascade);

        // configure many-to-many Category <-> Sale
        modelBuilder.Entity<Category>()
            .HasMany(c => c.Sales)
            .WithMany(s => s.Categories)
            .UsingEntity<Dictionary<string, object>>(
                "CategorySale",
                j => j.HasOne<Sale>().WithMany().HasForeignKey("SaleId").OnDelete(DeleteBehavior.Cascade),
                j => j.HasOne<Category>().WithMany().HasForeignKey("CategoryId").OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.HasKey("CategoryId", "SaleId");
                    j.ToTable("CategorySales");
                });

        // Sale mapping
        modelBuilder.Entity<Sale>(entity =>
        {
            entity.HasKey(s => s.SaleId);
            entity.Property(s => s.SaleDate).IsRequired();
            entity.Property(s => s.TotalAmount).IsRequired().HasPrecision(18, 2);
            entity.Property(s => s.CustomerName).IsRequired().HasMaxLength(100);
            entity.Property(s => s.CustomerEmail).IsRequired().HasMaxLength(100);
            entity.Property(s => s.CustomerAddress).IsRequired().HasMaxLength(200);

            entity
                .HasMany(s => s.SaleItems)
                .WithOne(si => si.Sale)
                .HasForeignKey(si => si.SaleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // SaleItem mapping (composite key)
        modelBuilder.Entity<SaleItem>(entity =>
        {
            entity.HasKey(si => new { si.SaleId, si.ProductId });

            entity.Property(si => si.Quantity).IsRequired();
            entity.Property(si => si.UnitPrice).IsRequired().HasPrecision(18, 2);
        });

        // Global query filters for soft delete
        modelBuilder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);
        modelBuilder.Entity<Category>().HasQueryFilter(c => !c.IsDeleted);
        // Filter out SaleItems that reference soft-deleted products
        modelBuilder.Entity<SaleItem>().HasQueryFilter(si => !si.Product.IsDeleted);
    }

    public void SeedData()
    {
        // Check if data already exists
        if (Categories.Any() || Products.Any() || Sales.Any())
        {
            // Data already exists, clear it first
            Sales.RemoveRange(Sales);
            SaleItems.RemoveRange(SaleItems);
            Products.RemoveRange(Products);
            Categories.RemoveRange(Categories);
            SaveChanges();
        }

        // Seed Categories - Let Entity Framework auto-generate IDs
        var electronics = new Category { Name = "Electronics", Description = "Electronic devices and gadgets" };
        var clothing = new Category { Name = "Clothing", Description = "Apparel and fashion items" };
        var books = new Category { Name = "Books", Description = "Books and publications" };
        var homeAndGarden = new Category { Name = "Home & Garden", Description = "Home improvement and gardening supplies" };
        var sports = new Category { Name = "Sports & Outdoors", Description = "Sports equipment and outdoor gear" };
        var beauty = new Category { Name = "Beauty & Personal Care", Description = "Cosmetics and personal care products" };
        var toys = new Category { Name = "Toys & Games", Description = "Toys, games, and entertainment" };
        var automotive = new Category { Name = "Automotive", Description = "Car parts and automotive accessories" };
        var health = new Category { Name = "Health & Household", Description = "Health products and household essentials" };
        var office = new Category { Name = "Office Products", Description = "Office supplies and equipment" };

        Categories.AddRange(electronics, clothing, books, homeAndGarden, sports, beauty, toys, automotive, health, office);

        // Save categories first to get their generated IDs
        SaveChanges();

        // Seed Products - Use the generated CategoryIds (55 products total)
        var products = new List<Product>
        {
            // Electronics (10 products)
            new Product { Name = "Laptop", Description = "High-performance laptop", Price = 999.99m, Stock = 10, IsActive = true, CategoryId = electronics.CategoryId },
            new Product { Name = "Smartphone", Description = "Latest smartphone model", Price = 699.99m, Stock = 15, IsActive = true, CategoryId = electronics.CategoryId },
            new Product { Name = "Wireless Headphones", Description = "Noise-cancelling wireless headphones", Price = 199.99m, Stock = 25, IsActive = true, CategoryId = electronics.CategoryId },
            new Product { Name = "Smart TV 55\"", Description = "4K Ultra HD Smart Television", Price = 799.99m, Stock = 8, IsActive = true, CategoryId = electronics.CategoryId },
            new Product { Name = "Gaming Console", Description = "Next-gen gaming console", Price = 499.99m, Stock = 12, IsActive = true, CategoryId = electronics.CategoryId },
            new Product { Name = "Tablet", Description = "10-inch Android tablet", Price = 299.99m, Stock = 20, IsActive = true, CategoryId = electronics.CategoryId },
            new Product { Name = "Bluetooth Speaker", Description = "Portable wireless speaker", Price = 79.99m, Stock = 35, IsActive = true, CategoryId = electronics.CategoryId },
            new Product { Name = "Smart Watch", Description = "Fitness tracking smartwatch", Price = 249.99m, Stock = 18, IsActive = true, CategoryId = electronics.CategoryId },
            new Product { Name = "External Hard Drive", Description = "2TB portable hard drive", Price = 89.99m, Stock = 22, IsActive = true, CategoryId = electronics.CategoryId },
            new Product { Name = "Wireless Router", Description = "High-speed WiFi router", Price = 129.99m, Stock = 14, IsActive = true, CategoryId = electronics.CategoryId },

            // Clothing (8 products)
            new Product { Name = "T-Shirt", Description = "Cotton t-shirt", Price = 19.99m, Stock = 50, IsActive = true, CategoryId = clothing.CategoryId },
            new Product { Name = "Jeans", Description = "Denim jeans", Price = 49.99m, Stock = 30, IsActive = true, CategoryId = clothing.CategoryId },
            new Product { Name = "Running Shoes", Description = "Comfortable athletic shoes", Price = 89.99m, Stock = 25, IsActive = true, CategoryId = clothing.CategoryId },
            new Product { Name = "Winter Jacket", Description = "Warm winter coat", Price = 129.99m, Stock = 15, IsActive = true, CategoryId = clothing.CategoryId },
            new Product { Name = "Dress Shirt", Description = "Formal dress shirt", Price = 39.99m, Stock = 40, IsActive = true, CategoryId = clothing.CategoryId },
            new Product { Name = "Yoga Pants", Description = "Comfortable yoga pants", Price = 34.99m, Stock = 28, IsActive = true, CategoryId = clothing.CategoryId },
            new Product { Name = "Baseball Cap", Description = "Adjustable baseball cap", Price = 16.99m, Stock = 60, IsActive = true, CategoryId = clothing.CategoryId },
            new Product { Name = "Leather Belt", Description = "Genuine leather belt", Price = 29.99m, Stock = 35, IsActive = true, CategoryId = clothing.CategoryId },

            // Books (6 products)
            new Product { Name = "Programming Book", Description = "Learn C# programming", Price = 39.99m, Stock = 20, IsActive = true, CategoryId = books.CategoryId },
            new Product { Name = "Science Fiction Novel", Description = "Bestselling sci-fi adventure", Price = 14.99m, Stock = 45, IsActive = true, CategoryId = books.CategoryId },
            new Product { Name = "Cookbook", Description = "Italian cuisine recipes", Price = 24.99m, Stock = 30, IsActive = true, CategoryId = books.CategoryId },
            new Product { Name = "Biography", Description = "Life story of a famous inventor", Price = 19.99m, Stock = 25, IsActive = true, CategoryId = books.CategoryId },
            new Product { Name = "Children's Book", Description = "Educational picture book", Price = 12.99m, Stock = 50, IsActive = true, CategoryId = books.CategoryId },
            new Product { Name = "Textbook", Description = "Advanced mathematics textbook", Price = 89.99m, Stock = 10, IsActive = true, CategoryId = books.CategoryId },

            // Home & Garden (6 products)
            new Product { Name = "Garden Hose", Description = "50ft expandable garden hose", Price = 34.99m, Stock = 20, IsActive = true, CategoryId = homeAndGarden.CategoryId },
            new Product { Name = "Lawn Mower", Description = "Electric lawn mower", Price = 249.99m, Stock = 8, IsActive = true, CategoryId = homeAndGarden.CategoryId },
            new Product { Name = "Paint Set", Description = "Interior paint set with brushes", Price = 79.99m, Stock = 15, IsActive = true, CategoryId = homeAndGarden.CategoryId },
            new Product { Name = "Tool Set", Description = "150-piece tool set", Price = 89.99m, Stock = 12, IsActive = true, CategoryId = homeAndGarden.CategoryId },
            new Product { Name = "Coffee Maker", Description = "12-cup coffee maker", Price = 59.99m, Stock = 18, IsActive = true, CategoryId = homeAndGarden.CategoryId },
            new Product { Name = "Blender", Description = "High-speed kitchen blender", Price = 69.99m, Stock = 22, IsActive = true, CategoryId = homeAndGarden.CategoryId },

            // Sports & Outdoors (6 products)
            new Product { Name = "Tennis Racket", Description = "Professional tennis racket", Price = 149.99m, Stock = 10, IsActive = true, CategoryId = sports.CategoryId },
            new Product { Name = "Yoga Mat", Description = "Non-slip yoga mat", Price = 29.99m, Stock = 40, IsActive = true, CategoryId = sports.CategoryId },
            new Product { Name = "Dumbbells Set", Description = "Adjustable dumbbells 5-50lbs", Price = 199.99m, Stock = 6, IsActive = true, CategoryId = sports.CategoryId },
            new Product { Name = "Camping Tent", Description = "4-person camping tent", Price = 119.99m, Stock = 12, IsActive = true, CategoryId = sports.CategoryId },
            new Product { Name = "Bicycle Helmet", Description = "Safety bicycle helmet", Price = 39.99m, Stock = 25, IsActive = true, CategoryId = sports.CategoryId },
            new Product { Name = "Swimming Goggles", Description = "Anti-fog swimming goggles", Price = 14.99m, Stock = 35, IsActive = true, CategoryId = sports.CategoryId },

            // Beauty & Personal Care (5 products)
            new Product { Name = "Shampoo", Description = "Moisturizing shampoo 16oz", Price = 8.99m, Stock = 60, IsActive = true, CategoryId = beauty.CategoryId },
            new Product { Name = "Face Cream", Description = "Anti-aging face cream", Price = 24.99m, Stock = 30, IsActive = true, CategoryId = beauty.CategoryId },
            new Product { Name = "Hair Dryer", Description = "Professional hair dryer", Price = 49.99m, Stock = 15, IsActive = true, CategoryId = beauty.CategoryId },
            new Product { Name = "Makeup Set", Description = "Complete makeup kit", Price = 79.99m, Stock = 20, IsActive = true, CategoryId = beauty.CategoryId },
            new Product { Name = "Perfume", Description = "Designer fragrance 3.4oz", Price = 89.99m, Stock = 12, IsActive = true, CategoryId = beauty.CategoryId },

            // Toys & Games (5 products)
            new Product { Name = "Board Game", Description = "Strategy board game for family", Price = 34.99m, Stock = 25, IsActive = true, CategoryId = toys.CategoryId },
            new Product { Name = "Building Blocks", Description = "Creative building block set", Price = 49.99m, Stock = 18, IsActive = true, CategoryId = toys.CategoryId },
            new Product { Name = "Puzzle 1000pc", Description = "1000-piece jigsaw puzzle", Price = 19.99m, Stock = 30, IsActive = true, CategoryId = toys.CategoryId },
            new Product { Name = "Stuffed Animal", Description = "Soft plush teddy bear", Price = 14.99m, Stock = 40, IsActive = true, CategoryId = toys.CategoryId },
            new Product { Name = "Remote Control Car", Description = "RC stunt car", Price = 39.99m, Stock = 15, IsActive = true, CategoryId = toys.CategoryId },

            // Automotive (4 products)
            new Product { Name = "Car Wash Kit", Description = "Complete car cleaning kit", Price = 29.99m, Stock = 20, IsActive = true, CategoryId = automotive.CategoryId },
            new Product { Name = "Tire Pressure Gauge", Description = "Digital tire pressure gauge", Price = 12.99m, Stock = 35, IsActive = true, CategoryId = automotive.CategoryId },
            new Product { Name = "Car Air Freshener", Description = "Long-lasting car air freshener", Price = 6.99m, Stock = 50, IsActive = true, CategoryId = automotive.CategoryId },
            new Product { Name = "Phone Mount", Description = "Dashboard phone holder", Price = 16.99m, Stock = 28, IsActive = true, CategoryId = automotive.CategoryId },

            // Health & Household (3 products)
            new Product { Name = "First Aid Kit", Description = "Complete first aid kit", Price = 24.99m, Stock = 15, IsActive = true, CategoryId = health.CategoryId },
            new Product { Name = "Vitamins", Description = "Multivitamin supplement", Price = 19.99m, Stock = 40, IsActive = true, CategoryId = health.CategoryId },
            new Product { Name = "Laundry Detergent", Description = "Concentrated laundry detergent", Price = 11.99m, Stock = 45, IsActive = true, CategoryId = health.CategoryId },

            // Office Products (2 products)
            new Product { Name = "Office Chair", Description = "Ergonomic office chair", Price = 199.99m, Stock = 8, IsActive = true, CategoryId = office.CategoryId },
            new Product { Name = "Printer Paper", Description = "500 sheets printer paper", Price = 7.99m, Stock = 100, IsActive = true, CategoryId = office.CategoryId }
        };

        Products.AddRange(products);
        SaveChanges();

        // Get all products for sales generation
        var allProducts = Products.ToList();

        // Create some sales data - mix of current and historical sales
        var sales = new List<Sale>();
        var random = new Random(42); // Fixed seed for reproducible data

        // Generate 50 sales with various dates
        for (int i = 0; i < 50; i++)
        {
            // Create sales from the last 2 years to test temporal filtering
            var saleDate = DateTime.UtcNow.AddDays(-random.Next(0, 730)); // 0-730 days ago

            // Select 1-5 random products for each sale
            var saleProducts = allProducts.OrderBy(x => random.Next()).Take(random.Next(1, 6)).ToList();

            var saleItems = new List<SaleItem>();
            decimal totalAmount = 0;

            foreach (var product in saleProducts)
            {
                var quantity = random.Next(1, 4); // 1-3 items
                var unitPrice = product.Price;
                totalAmount += unitPrice * quantity;

                saleItems.Add(new SaleItem
                {
                    ProductId = product.ProductId,
                    Quantity = quantity,
                    UnitPrice = unitPrice
                });
            }

            // Generate fake customer data
            var customerNames = new[] { "John Doe", "Jane Smith", "Bob Johnson", "Alice Brown", "Charlie Wilson", "Diana Davis", "Edward Miller", "Fiona Garcia", "George Rodriguez", "Helen Martinez" };
            var customerName = customerNames[random.Next(customerNames.Length)];
            var customerEmail = $"{customerName.ToLower().Replace(" ", ".")}@example.com";
            var addresses = new[] { "123 Main St", "456 Oak Ave", "789 Pine Rd", "321 Elm St", "654 Maple Dr", "987 Cedar Ln", "147 Birch Blvd", "258 Spruce Way", "369 Willow Ct", "741 Poplar Pl" };
            var customerAddress = $"{addresses[random.Next(addresses.Length)]}, Anytown, ST 12345";

            sales.Add(new Sale
            {
                SaleDate = saleDate,
                TotalAmount = totalAmount,
                CustomerName = customerName,
                CustomerEmail = customerEmail,
                CustomerAddress = customerAddress,
                SaleItems = saleItems
            });
        }

        // Add some products that will be soft-deleted to test temporal filtering
        // These products will be deleted after some sales but before others
        var productsToDelete = allProducts.Where(p => p.CategoryId == electronics.CategoryId).Take(3).ToList();
        var deletionDate = DateTimeOffset.UtcNow.AddDays(-30); // Deleted 30 days ago

        foreach (var product in productsToDelete)
        {
            product.SoftDelete();
            product.DeletedAt = deletionDate;
        }

        // Create some sales after the deletion date that shouldn't include deleted products
        for (int i = 0; i < 10; i++)
        {
            var saleDate = DateTime.UtcNow.AddDays(-random.Next(1, 29)); // 1-29 days ago (after deletion)

            // Only select products that are not deleted
            var availableProducts = allProducts.Where(p => !p.IsDeleted).ToList();
            var saleProducts = availableProducts.OrderBy(x => random.Next()).Take(random.Next(1, 4)).ToList();

            if (saleProducts.Count > 0)
            {
                var saleItems = new List<SaleItem>();
                decimal totalAmount = 0;

                foreach (var product in saleProducts)
                {
                    var quantity = random.Next(1, 3);
                    var unitPrice = product.Price;
                    totalAmount += unitPrice * quantity;

                    saleItems.Add(new SaleItem
                    {
                        ProductId = product.ProductId,
                        Quantity = quantity,
                        UnitPrice = unitPrice
                    });
                }

                // Generate fake customer data
                var customerNames = new[] { "John Doe", "Jane Smith", "Bob Johnson", "Alice Brown", "Charlie Wilson", "Diana Davis", "Edward Miller", "Fiona Garcia", "George Rodriguez", "Helen Martinez" };
                var customerName = customerNames[random.Next(customerNames.Length)];
                var customerEmail = $"{customerName.ToLower().Replace(" ", ".")}@example.com";
                var addresses = new[] { "123 Main St", "456 Oak Ave", "789 Pine Rd", "321 Elm St", "654 Maple Dr", "987 Cedar Ln", "147 Birch Blvd", "258 Spruce Way", "369 Willow Ct", "741 Poplar Pl" };
                var customerAddress = $"{addresses[random.Next(addresses.Length)]}, Anytown, ST 12345";

                sales.Add(new Sale
                {
                    SaleDate = saleDate,
                    TotalAmount = totalAmount,
                    CustomerName = customerName,
                    CustomerEmail = customerEmail,
                    CustomerAddress = customerAddress,
                    SaleItems = saleItems
                });
            }
        }

        Sales.AddRange(sales);
        SaveChanges();
    }
}
