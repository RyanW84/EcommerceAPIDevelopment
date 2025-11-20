using System;
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

        public async Task<ApiResponseDto<Product>> AddAsync(
            Product entity,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                await _db.Products.AddAsync(entity, cancellationToken);
                await _db.SaveChangesAsync(cancellationToken);

                // Reload the product with related Category using explicit queries to ensure fresh data
                var createdProduct = await _db
                    .Products.Where(p => p.ProductId == entity.ProductId)
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(cancellationToken);

                // DEBUG: Log what we got back
                var categoryLoaded = createdProduct?.Category != null;
                var categoryName = createdProduct?.Category?.Name ?? "NULL";
                System.Diagnostics.Debug.WriteLine(
                    $"[AddAsync] Product {createdProduct?.ProductId}: CategoryId={createdProduct?.CategoryId}, Category Loaded={categoryLoaded}, Category Name={categoryName}"
                );

                return new ApiResponseDto<Product>
                {
                    RequestFailed = false,
                    ResponseCode = HttpStatusCode.Created,
                    ErrorMessage = string.Empty,
                    Data = createdProduct,
                };
            }
            catch (DbUpdateException ex)
            {
                // Handle constraint violations, foreign key errors, etc.
                if (
                    ex.InnerException?.Message.Contains("FOREIGN KEY") == true
                    || ex.InnerException?.Message.Contains("constraint") == true
                )
                {
                    return new ApiResponseDto<Product>
                    {
                        RequestFailed = true,
                        ResponseCode = HttpStatusCode.BadRequest,
                        ErrorMessage =
                            "Invalid data: Foreign key constraint violation or invalid reference.",
                        Data = null,
                    };
                }

                return new ApiResponseDto<Product>
                {
                    RequestFailed = true,
                    ResponseCode = HttpStatusCode.Conflict,
                    ErrorMessage = "Failed to save product due to data conflict.",
                    Data = null,
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<Product>
                {
                    RequestFailed = true,
                    ResponseCode = HttpStatusCode.InternalServerError,
                    ErrorMessage = $"Failed to create product: {ex.Message}",
                    Data = null,
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

        public async Task<PaginatedResponseDto<List<Product>>> GetAllProductsAsync(
            ProductQueryParameters parameters,
            CancellationToken cancellationToken = default
        )
        {
            parameters ??= new ProductQueryParameters();

            var page = Math.Max(parameters.Page, 1);
            var pageSize = Math.Clamp(parameters.PageSize, 1, 100);

            try
            {
                var query = _db
                    .Products.Include(p => p.Category)
                    .Where(p => !p.IsDeleted)
                    .AsNoTracking()
                    .AsQueryable();

                var search = parameters.Search?.Trim();
                if (!string.IsNullOrEmpty(search))
                {
                    var likePattern = $"%{search}%";
                    query = query.Where(p =>
                        EF.Functions.Like(p.Name, likePattern)
                        || EF.Functions.Like(p.Description, likePattern)
                    );
                }

                if (parameters.MinPrice is { } minPrice)
                {
                    query = query.Where(p => p.Price >= minPrice);
                }

                if (parameters.MaxPrice is { } maxPrice)
                {
                    query = query.Where(p => p.Price <= maxPrice);
                }

                if (parameters.CategoryId is { } categoryId)
                {
                    query = query.Where(p => p.CategoryId == categoryId);
                }

                var descending = string.Equals(
                    parameters.SortDirection,
                    "desc",
                    StringComparison.OrdinalIgnoreCase
                );
                var sortBy = parameters.SortBy?.Trim().ToLowerInvariant();

                var orderedQuery = ApplyProductSorting(query, sortBy, descending);

                var totalCount = await orderedQuery.CountAsync(cancellationToken);
                var products = await orderedQuery
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);

                // DEBUG: Log products returned
                foreach (var p in products)
                {
                    var categoryLoaded = p.Category != null;
                    var categoryName = p.Category?.Name ?? "NULL";
                    System.Diagnostics.Debug.WriteLine(
                        $"[GetAllProductsAsync] Product {p.ProductId}: CategoryId={p.CategoryId}, Category Loaded={categoryLoaded}, Category Name={categoryName}"
                    );
                }

                return new PaginatedResponseDto<List<Product>>
                {
                    RequestFailed = false,
                    ResponseCode = HttpStatusCode.OK,
                    ErrorMessage = string.Empty,
                    Data = products,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                };
            }
            catch (Exception ex)
            {
                return new PaginatedResponseDto<List<Product>>
                {
                    RequestFailed = true,
                    ResponseCode = HttpStatusCode.InternalServerError,
                    ErrorMessage = ex.Message,
                    Data = null,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalCount = 0,
                };
            }
        }

        private static IQueryable<Product> ApplyProductSorting(
            IQueryable<Product> query,
            string? sortBy,
            bool descending
        )
        {
            return sortBy switch
            {
                "name" => descending
                    ? query.OrderByDescending(p => p.Name)
                    : query.OrderBy(p => p.Name),
                "price" => descending
                    ? query.OrderByDescending(p => p.Price)
                    : query.OrderBy(p => p.Price),
                "createdat" => descending
                    ? query.OrderByDescending(p => p.CreatedAt)
                    : query.OrderBy(p => p.CreatedAt),
                "stock" => descending
                    ? query.OrderByDescending(p => p.Stock)
                    : query.OrderBy(p => p.Stock),
                "category" => descending
                    ? query.OrderByDescending(p => p.Category!.Name)
                    : query.OrderBy(p => p.Category!.Name),
                _ => descending
                    ? query.OrderByDescending(p => p.ProductId)
                    : query.OrderBy(p => p.ProductId),
            };
        }

        public async Task<ApiResponseDto<List<Product>>> GetProductsByCategoryIdAsync(
            int categoryId,
            CancellationToken cancellationToken = default
        )
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

        public async Task<ApiResponseDto<Product>> UpdateAsync(
            Product entity,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                var existing = await _db
                    .Products.Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.ProductId == entity.ProductId, cancellationToken);

                if (existing == null)
                {
                    return new ApiResponseDto<Product>
                    {
                        RequestFailed = true,
                        ResponseCode = HttpStatusCode.NotFound,
                        ErrorMessage = "Product not found",
                        Data = null,
                    };
                }

                // Update properties
                existing.Name = entity.Name;
                existing.Description = entity.Description;
                existing.Stock = entity.Stock;
                existing.IsActive = entity.IsActive;
                existing.CategoryId = entity.CategoryId;
                // Note: Price is preserved in the service layer

                await _db.SaveChangesAsync(cancellationToken);

                // Reload the product with related Category to return updated data
                var updatedProduct = await _db
                    .Products.Where(p => p.ProductId == existing.ProductId)
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(cancellationToken);

                return new ApiResponseDto<Product>
                {
                    RequestFailed = false,
                    ResponseCode = HttpStatusCode.OK,
                    ErrorMessage = string.Empty,
                    Data = updatedProduct,
                };
            }
            catch (DbUpdateConcurrencyException)
            {
                return new ApiResponseDto<Product>
                {
                    RequestFailed = true,
                    ResponseCode = HttpStatusCode.Conflict,
                    ErrorMessage =
                        "Product was modified by another user. Please refresh and try again.",
                    Data = null,
                };
            }
            catch (DbUpdateException ex)
            {
                if (
                    ex.InnerException?.Message.Contains("FOREIGN KEY") == true
                    || ex.InnerException?.Message.Contains("constraint") == true
                )
                {
                    return new ApiResponseDto<Product>
                    {
                        RequestFailed = true,
                        ResponseCode = HttpStatusCode.BadRequest,
                        ErrorMessage =
                            "Invalid data: Foreign key constraint violation or invalid reference.",
                        Data = null,
                    };
                }

                return new ApiResponseDto<Product>
                {
                    RequestFailed = true,
                    ResponseCode = HttpStatusCode.Conflict,
                    ErrorMessage = "Failed to update product due to data conflict.",
                    Data = null,
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<Product>
                {
                    RequestFailed = true,
                    ResponseCode = HttpStatusCode.InternalServerError,
                    ErrorMessage = $"Failed to update product: {ex.Message}",
                    Data = null,
                };
            }
        }

        public async Task<ApiResponseDto<bool>> DeleteAsync(
            int id,
            CancellationToken cancellationToken = default
        )
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

                // Soft delete: mark as deleted instead of removing
                product.IsDeleted = true;
                product.DeletedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync(cancellationToken);
                return new ApiResponseDto<bool>
                {
                    RequestFailed = false,
                    ResponseCode = HttpStatusCode.NoContent,
                    ErrorMessage = string.Empty,
                    Data = true,
                };
            }
            catch (DbUpdateException ex)
            {
                // Handle cases where deletion fails due to foreign key constraints
                if (
                    ex.InnerException?.Message.Contains("FOREIGN KEY") == true
                    || ex.InnerException?.Message.Contains("constraint") == true
                )
                {
                    return new ApiResponseDto<bool>
                    {
                        RequestFailed = true,
                        ResponseCode = HttpStatusCode.Conflict,
                        ErrorMessage =
                            "Cannot delete product as it is referenced by existing sales.",
                        Data = false,
                    };
                }

                return new ApiResponseDto<bool>
                {
                    RequestFailed = true,
                    ResponseCode = HttpStatusCode.InternalServerError,
                    ErrorMessage = "Failed to delete product due to database error.",
                    Data = false,
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDto<bool>
                {
                    RequestFailed = true,
                    ResponseCode = HttpStatusCode.InternalServerError,
                    ErrorMessage = $"Failed to delete product: {ex.Message}",
                    Data = false,
                };
            }
        }

        public async Task<ApiResponseDto<bool>> RestoreAsync(
            int id,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                var product = await _db
                    .Products.IgnoreQueryFilters() // Include soft-deleted items
                    .FirstOrDefaultAsync(p => p.ProductId == id, cancellationToken);

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

                if (!product.IsDeleted)
                {
                    return new ApiResponseDto<bool>
                    {
                        RequestFailed = true,
                        ResponseCode = HttpStatusCode.BadRequest,
                        ErrorMessage = "Product is not deleted",
                        Data = false,
                    };
                }

                // Restore the product
                product.IsDeleted = false;
                product.DeletedAt = null;

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

        public async Task<ApiResponseDto<List<Product>>> GetDeletedProductsAsync(
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                var deletedProducts = await _db
                    .Products.IgnoreQueryFilters() // Include soft-deleted items
                    .Where(p => p.IsDeleted)
                    .Include(p => p.Category)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                return new ApiResponseDto<List<Product>>
                {
                    RequestFailed = false,
                    ResponseCode = HttpStatusCode.OK,
                    ErrorMessage = string.Empty,
                    Data = deletedProducts,
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

        public async Task<ApiResponseDto<bool>> HardDeleteAsync(
            int id,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                var product = await _db
                    .Products.IgnoreQueryFilters() // Include soft-deleted items
                    .FirstOrDefaultAsync(p => p.ProductId == id, cancellationToken);

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
                    ResponseCode = HttpStatusCode.NoContent,
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
    }
}
