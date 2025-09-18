using ECommerceApp.RyanW84.Data.Models;

namespace ECommerceApp.RyanW84.Interfaces
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<List<Product>> GetActiveProductsWithCategoryAsync(
            CancellationToken cancellationToken = default
        );
        // Add more product-specific queries here as needed
    }
}
