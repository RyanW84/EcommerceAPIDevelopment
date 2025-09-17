using ECommerceApp.RyanW84.Data;
using ECommerceApp.RyanW84.Data.DTO;
using ECommerceApp.RyanW84.Data.Models;
using ECommerceApp.RyanW84.Interfaces;
using System.Net;

using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.RyanW84.Services;

public class ProductService(ECommerceDbContext db) : IProductService
{
    private readonly ECommerceDbContext _dbcontext = db;

    public async Task CreateProductAsync()
    {
        await Task.Delay(1000);
    }

    public async Task<ApiResponseDto<List<Product>>> GetProductsAsync()
    {
        var query = await _dbcontext.Products
            .Where(p => p.IsActive)
            .Select(p => new Product
            {
                ProductId = p.ProductId,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock,
                IsActive = p.IsActive,
                CategoryId = p.CategoryId,
                Category = new Category
                {
                    CategoryId = p.Category.CategoryId,
                    Name = p.Category.Name,
                    Description = p.Category.Description
                }
                // Note: Explicitly NOT including Sales collection
            })
            .ToListAsync();

        List<Product> products = query;

        return new ApiResponseDto<List<Product>>()
        {
            RequestFailed = false,
            ResponseCode = HttpStatusCode.OK,
            ErrorMessage = "No Errors",
            Data = products
        };
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
