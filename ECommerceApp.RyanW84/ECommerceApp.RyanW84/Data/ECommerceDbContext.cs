using ECommerceApp.RyanW84.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.RyanW84.Data;

public class ECommerceDbContext : DbContext
{
    public ECommerceDbContext(DbContextOptions<ECommerceDbContext> options)
        : base(options) { }

    // DbSets for the entities
    public DbSet<Models.Product> Products { get; set; } = null!;
    public DbSet<Models.Category> Categories { get; set; } = null!;
    public DbSet<Models.Sale> Sales { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Models.Product>(entity =>
        {
            entity.HasKey(p => p.Id);
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

            // Global filter to exclude inactive products
            entity.HasQueryFilter(p => p.IsActive);
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
                .OnDelete(DeleteBehavior.Cascade);
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
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Seed initial Categories
        modelBuilder
            .Entity<Models.Category>()
            .HasData(
                new Models.Category
                {
                    CategoryId = 1,
                    Name = "Electronics",
                    Description = "Electronic gadgets and devices",
                },
                new Models.Category
                {
                    CategoryId = 2,
                    Name = "Books",
                    Description = "Various kinds of books",
                },
                new Models.Category
                {
                    CategoryId = 3,
                    Name = "Clothing",
                    Description = "Apparel and accessories",
                }
            );

        // Seed initial Products
        modelBuilder
            .Entity<Models.Product>()
            .HasData(
                new Models.Product
                {
                    Id = 1,
                    Name = "Smartphone",
                    Description = "Latest model smartphone",
                    Price = 699.99m,
                    Stock = 50,
                    IsActive = true,
                    CategoryId = 1,
                },
                new Models.Product
                {
                    Id = 2,
                    Name = "Laptop",
                    Description = "High performance laptop",
                    Price = 1299.99m,
                    Stock = 30,
                    IsActive = true,
                    CategoryId = 1,
                },
                new Models.Product
                {
                    Id = 3,
                    Name = "Novel",
                    Description = "Bestselling fiction novel",
                    Price = 19.99m,
                    Stock = 100,
                    IsActive = true,
                    CategoryId = 2,
                },
                new Models.Product
                {
                    Id = 4,
                    Name = "T-Shirt",
                    Description = "100% cotton t-shirt",
                    Price = 9.99m,
                    Stock = 200,
                    IsActive = true,
                    CategoryId = 3,
                }
            );

        // Note: Sales are typically not seeded as they are transactional data
    }

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
    private void HandleSoftDelete()
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
                deletedAtProp != null
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
