using ECommerceApp.RyanW84.Data.Models;
using ECommerceApp.RyanW84.Services;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.RyanW84.Data;

public class ECommerceDbContext(DbContextOptions<ECommerceDbContext> options) : DbContext(options)
{
    // DbSets for the entities
    public DbSet<Models.Product> Products { get; set; } = null!;
    public DbSet<Models.Category> Categories { get; set; } = null!;
    public DbSet<Models.Sale> Sales { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Models.Product>(entity =>
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
                .HasMany(p => p.Sales)
                .WithOne(s => s.Product)
                .HasForeignKey(s => s.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Models.Category>(entity =>
        {
            entity.HasKey(c => c.CategoryId);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
            entity.Property(c => c.Description).IsRequired().HasMaxLength(500);
            entity
                .HasMany(c => c.Products)
                .WithOne(p => p.Category)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
            entity
                .HasMany(c => c.Sales)
                .WithOne(s => s.Category)
                .HasForeignKey(s => s.CategoryId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Models.Sale>(entity =>
        {
            entity.HasKey(s => s.SaleId);
            entity.Property(s => s.SaleDate).IsRequired();
            entity.Property(s => s.TotalAmount).IsRequired().HasPrecision(18, 2);
            entity.Property(s => s.Quantity).IsRequired();
            entity.Property(s => s.CustomerName).IsRequired().HasMaxLength(100);
            entity.Property(s => s.CustomerEmail).IsRequired().HasMaxLength(100);
            entity.Property(s => s.CustomerAddress).IsRequired().HasMaxLength(200);
            entity.Property(s => s.ProductId).IsRequired();
            entity.Property(s => s.CategoryId).IsRequired();

            entity
                .HasOne(s => s.Product)
                .WithMany(p => p.Sales)
                .HasForeignKey(s => s.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasOne(s => s.Category)
                .WithMany(c => c.Sales)
                .HasForeignKey(s => s.CategoryId)
                .OnDelete(DeleteBehavior.NoAction);
        });
    }

    public void SeedData()
    {
        // Seed initial Categories
        Categories.RemoveRange(Categories);
        Products.RemoveRange(Products);
        Sales.RemoveRange(Sales);

        Category Electronics = new()
        {
            CategoryId = 1,
            Name = "Electronics",
            Description = "Electronic gadgets and devices",
        };
        Category Books = new()
        {
            CategoryId = 2,
            Name = "Books",
            Description = "Various kinds of books",
        };
        Category Clothing = new()
        {
            CategoryId = 3,
            Name = "Clothing",
            Description = "Apparel and accessories",
        };

        Categories.AddRange(Electronics, Books, Clothing);

        // Seed initial Products
        Product product1 = new()
        {
            ProductId = 1,
            Name = "Smartphone",
            Description = "Latest model smartphone",
            Price = 699.99m,
            Stock = 50,
            IsActive = true,
            CategoryId = 1,
        };
        Product product2 = new()
        {
            ProductId = 2,
            Name = "Laptop",
            Description = "High performance laptop",
            Price = 1299.99m,
            Stock = 30,
            IsActive = true,
            CategoryId = 1,
        };
        Product product3 = new()
        {
            ProductId = 3,
            Name = "Novel",
            Description = "Bestselling fiction novel",
            Price = 19.99m,
            Stock = 100,
            IsActive = true,
            CategoryId = 2,
        };
        Product product4 = new()
        {
            ProductId = 4,
            Name = "T-Shirt",
            Description = "100% cotton t-shirt",
            Price = 9.99m,
            Stock = 200,
            IsActive = true,
            CategoryId = 3,
        };

        Products.AddRange(product1, product2, product3, product4);
    }

    // Note: Sales are typically not seeded as they are transactional data


    public override int SaveChanges()
    {
        HandleSoftDelete();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        HandleSoftDelete();
        return base.SaveChangesAsync(cancellationToken);
    }

    // Soft-delete handler: checks for an "IsActive" property and optional "DeletedAt" and sets them instead of removing
    private async void HandleSoftDelete()
    {
        var softDeleteEntries = ChangeTracker
            .Entries()
            .Where(e => e.State == EntityState.Deleted && e.Entity is not null)
            .Where(e => e.Entity.GetType().GetProperty("IsActive") is not null)
            .ToList();

        foreach (var entry in softDeleteEntries)
        {
            entry.State = EntityState.Modified;
            var entity = entry.Entity;
            var isActiveProp = entity.GetType().GetProperty("IsActive");
            if (
                isActiveProp is not null
                && isActiveProp.CanWrite
                && isActiveProp.PropertyType == typeof(bool)
            )
            {
                isActiveProp.SetValue(entity, false);
            }

            var deletedAtProp = entity.GetType().GetProperty("DeletedAt");
            if (
                deletedAtProp is not null
                && deletedAtProp.CanWrite
                && (
                    deletedAtProp.PropertyType == typeof(DateTime)
                    || deletedAtProp.PropertyType == typeof(DateTime?)
                )
            )
            {
                deletedAtProp.SetValue(entity, DateTime.UtcNow);
            }
        }
    }
}
