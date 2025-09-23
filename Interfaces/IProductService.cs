using ECommerceApp.RyanW84.Data.DTO;
using ECommerceApp.RyanW84.Data.Models;
using ECommerceApp.RyanW84.Services;


namespace ECommerceApp.RyanW84.Interfaces;

public interface IProductService
{
    // Define methods for product management
    Task CreateProductAsync();
    Task<ApiResponseDto<List<Product>>> GetProductsAsync();
    Task GetProductFromSelectionPromptAsync(string prompt);
    Task UpdateProductAsync();
    Task DeleteProductAsync();
}
