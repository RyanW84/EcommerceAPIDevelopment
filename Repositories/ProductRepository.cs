using System.Net;
using ECommerceApp.RyanW84.Data;
using ECommerceApp.RyanW84.Data.DTO;
using ECommerceApp.RyanW84.Data.Models;
using ECommerceApp.RyanW84.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.RyanW84.Repositories
{
    public class ProductRepository(ECommerceDbContext db) : IProductRepository
    {
        private readonly ECommerceDbContext _db = db;

        public async Task<ApiResponseDto<Product>> AddAsync(Product entity, CancellationToken cancellationToken = default)
        {
            try
            {
                await _db.Products.AddAsync(entity, cancellationToken);
                await _db.SaveChangesAsync(cancellationToken);
                return new ApiResponseDto<Product>
                {
                    RequestFailed = false,
                    ResponseCode = HttpStatusCode.Created,
                    ErrorMessage = string.Empty,
                    Data = entity,
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<Product>
                {
                    RequestFailed = true,
                    ResponseCode = HttpStatusCode.InternalServerError,
                    ErrorMessage = ex.Message,
                    Data = null,
                };
            }
        }

        public async Task<ApiResponseDto<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var product = await _db.Products.FindAsync(new object[] { id }, cancellationToken);
                if (product == null)
                {
                    return new ApiResponseDto<bool>
                    {
                        RequestFailed = true,
                        ResponseCode = HttpStatusCode.NotFound,
                        ErrorMessage = "Product not found",
                        Data = false,
                    };
                }
                _db.Products.Remove(product);
                await _db.SaveChangesAsync(cancellationToken);
                return new ApiResponseDto<bool>
                {
                    RequestFailed = false,
                    ResponseCode = HttpStatusCode.OK,
                    ErrorMessage = string.Empty,
                    Data = true,
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<bool>
                {
                    RequestFailed = true,
                    ResponseCode = HttpStatusCode.InternalServerError,
                    ErrorMessage = ex.Message,
                    Data = false,
                };
            }
        }

        public async Task<ApiResponseDto<Product?>> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                var product = await _db
                    .Products.AsNoTracking()
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.ProductId == id, cancellationToken);
                return new ApiResponseDto<Product?>
                {
                    RequestFailed = false,
                    ResponseCode = HttpStatusCode.OK,
                    ErrorMessage = string.Empty,
                    Data = product,
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<Product?>
                {
                    RequestFailed = true,
                    ResponseCode = HttpStatusCode.InternalServerError,
                    ErrorMessage = ex.Message,
                    Data = null,
                };
            }
        }

        public async Task<ApiResponseDto<List<Product>>> GetAllProductsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var products = await _db
                    .Products.AsNoTracking()
                    .Include(p => p.Category)
                    .ToListAsync(cancellationToken);
                return new ApiResponseDto<List<Product>>
                {
                    RequestFailed = false,
                    ResponseCode = HttpStatusCode.OK,
                    ErrorMessage = string.Empty,
                    Data = products,
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<List<Product>>
                {
                    RequestFailed = true,
                    ResponseCode = HttpStatusCode.InternalServerError,
                    ErrorMessage = ex.Message,
                    Data = null,
                };
            }
        }

        public async Task<ApiResponseDto<List<Product>>> GetProductsByCategoryIdAsync(int categoryId, CancellationToken cancellationToken = default)
        {
            try
            {
                var products = await _db
                    .Products.AsNoTracking()
                    .Where(p => p.CategoryId == categoryId)
                    .Include(p => p.Category)
                    .ToListAsync(cancellationToken);
                return new ApiResponseDto<List<Product>>
                {
                    RequestFailed = false,
                    ResponseCode = HttpStatusCode.OK,
                    ErrorMessage = string.Empty,
                    Data = products,
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<List<Product>>
                {
                    RequestFailed = true,
                    ResponseCode = HttpStatusCode.InternalServerError,
                    ErrorMessage = ex.Message,
                    Data = null,
                };
            }
        }

        public async Task<ApiResponseDto<Product>> UpdateAsync(Product entity, CancellationToken cancellationToken = default)
        {
            try
            {
                _db.Products.Update(entity);
                await _db.SaveChangesAsync(cancellationToken);
                return new ApiResponseDto<Product>
                {
                    RequestFailed = false,
                    ResponseCode = HttpStatusCode.OK,
                    ErrorMessage = string.Empty,
                    Data = entity,
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<Product>
                {
                    RequestFailed = true,
                    ResponseCode = HttpStatusCode.InternalServerError,
                    ErrorMessage = ex.Message,
                    Data = null,
                };
            }
        }
    }
}
