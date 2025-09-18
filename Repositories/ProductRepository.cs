using ECommerceApp.RyanW84.Data;
using ECommerceApp.RyanW84.Data.Models;
using ECommerceApp.RyanW84.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.RyanW84.Repositories
{
    public class ProductRepository(ECommerceDbContext db): IProductRepository
    {
        private readonly ECommerceDbContext _db = db;

		public async Task AddAsync(Product entity, CancellationToken cancellationToken = default)
        {
            await _db.Products.AddAsync(entity, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Product entity, CancellationToken cancellationToken = default)
        {
            _db.Products.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task<Product?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default
        )
        {
            return await _db
                .Products.AsNoTracking()
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == id, cancellationToken);
        }

        public async Task<List<Product>> ListAsync(CancellationToken cancellationToken = default)
        {
            return await _db
                .Products.AsNoTracking()
                .Include(p => p.Category)
                .ToListAsync(cancellationToken);
        }

        public async Task UpdateAsync(Product entity, CancellationToken cancellationToken = default)
        {
            _db.Products.Update(entity);
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<Product>> GetActiveProductsWithCategoryAsync(
            CancellationToken cancellationToken = default
        )
        {
            return await _db
                .Products.AsNoTracking()
                .Where(p => p.IsActive)
                .Include(p => p.Category)
                .ToListAsync(cancellationToken);
        }
    }
}
