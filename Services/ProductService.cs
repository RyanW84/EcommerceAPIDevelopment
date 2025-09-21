using System.Net;
using ECommerceApp.RyanW84.Data.DTO;
using ECommerceApp.RyanW84.Data.Models;
using ECommerceApp.RyanW84.Interfaces;

namespace ECommerceApp.RyanW84.Services
{
    public class ProductService(IProductRepository productRepository) : IProductService
    {
        private readonly IProductRepository _productRepository = productRepository;

        public async Task CreateProductAsync()
        {
            await Task.Delay(1000);
        }

        public async Task<ApiResponseDto<List<Product>>> GetProductsAsync()
        {
            return await _productRepository.GetAllProductsAsync();
        }

        public async Task GetProductFromSelectionPromptAsync(string prompt)
        {
            await Task.Delay(1000);
        }

        public async Task UpdateProductAsync()
        {
            await Task.Delay(1000);
        }

        public async Task DeleteProductAsync()
        {
            await Task.Delay(1000);
        }
    }
}
