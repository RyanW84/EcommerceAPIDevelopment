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
    }

    public void SeedData()
    {
        // Seed Categories
        var electronics = new Category { CategoryId = 1, Name = "Electronics", Description = "Electronic devices and gadgets" };
        var clothing = new Category { CategoryId = 2, Name = "Clothing", Description = "Apparel and fashion items" };
        var books = new Category { CategoryId = 3, Name = "Books", Description = "Books and publications" };

        Categories.AddRange(electronics, clothing, books);

        // Seed Products
        Products.AddRange(
            new Product { ProductId = 1, Name = "Laptop", Description = "High-performance laptop", Price = 999.99m, Stock = 10, IsActive = true, CategoryId = 1 },
            new Product { ProductId = 2, Name = "Smartphone", Description = "Latest smartphone model", Price = 699.99m, Stock = 15, IsActive = true, CategoryId = 1 },
            new Product { ProductId = 3, Name = "T-Shirt", Description = "Cotton t-shirt", Price = 19.99m, Stock = 50, IsActive = true, CategoryId = 2 },
            new Product { ProductId = 4, Name = "Jeans", Description = "Denim jeans", Price = 49.99m, Stock = 30, IsActive = true, CategoryId = 2 },
            new Product { ProductId = 5, Name = "Programming Book", Description = "Learn C# programming", Price = 39.99m, Stock = 20, IsActive = true, CategoryId = 3 }
        );
    }
}
