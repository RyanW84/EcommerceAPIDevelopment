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
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Name).IsRequired().HasMaxLength(100);

            entity.Property(p => p.Description).IsRequired().HasMaxLength(500);

            // Use HasPrecision for money values (preferred over raw column type)
            entity.Property(p => p.Price).IsRequired().HasPrecision(18, 2);

            // Defaults and requiredness for inventory/flags
            entity.Property(p => p.Stock).IsRequired().HasDefaultValue(0);

            entity.Property(p => p.IsActive).IsRequired().HasDefaultValue(true);

            entity.Property(p => p.CategoryId).IsRequired();

            // Index for common lookups
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
    }
}
