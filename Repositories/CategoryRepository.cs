using System;
using System.Net;
using ECommerceApp.RyanW84.Data;
using ECommerceApp.RyanW84.Data.DTO;
using ECommerceApp.RyanW84.Data.Models;
using ECommerceApp.RyanW84.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.RyanW84.Repositories;

public class CategoryRepository(ECommerceDbContext db) : ICategoryRepository
{
    private readonly ECommerceDbContext _db = db;

    public async Task<bool> CategoryExistsAsync(
        string categoryName,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(categoryName))
            return false;
        return await _db
            .Categories.AsNoTracking()
            .AnyAsync(c => c.Name == categoryName, cancellationToken);
    }

    public async Task<ApiResponseDto<Category?>> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var category = await _db
                .Categories.AsNoTracking()
                .Include(c => c.Products)
                .Include(c => c.Sales)
                .FirstOrDefaultAsync(c => c.CategoryId == id, cancellationToken);

            return new ApiResponseDto<Category?>
            {
                RequestFailed = false,
                ResponseCode = HttpStatusCode.OK,
                ErrorMessage = string.Empty,
                Data = category,
            };
        }
        catch (Exception ex)
        {
            return new ApiResponseDto<Category?>
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.InternalServerError,
                ErrorMessage = ex.Message,
                Data = null,
            };
        }
    }

    public async Task<ApiResponseDto<Category?>> GetByNameAsync(
        string name,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var category = await _db
                .Categories.AsNoTracking()
                .Include(c => c.Products)
                .Include(c => c.Sales)
                .FirstOrDefaultAsync(c => c.Name == name, cancellationToken);

            return new ApiResponseDto<Category?>
            {
                RequestFailed = false,
                ResponseCode = HttpStatusCode.OK,
                ErrorMessage = string.Empty,
                Data = category,
            };
        }
        catch (Exception ex)
        {
            return new ApiResponseDto<Category?>
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.InternalServerError,
                ErrorMessage = ex.Message,
                Data = null,
            };
        }
    }

    public async Task<ApiResponseDto<List<Category>>> GetAllCategoriesAsync(
        CategoryQueryParameters parameters,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            parameters ??= new CategoryQueryParameters();

            var query = _db
                .Categories.AsNoTracking()
                .Include(c => c.Products)
                .AsQueryable();

            if (parameters.IncludeDeleted)
            {
                query = query.IgnoreQueryFilters();
            }

            var search = parameters.Search?.Trim();
            if (!string.IsNullOrEmpty(search))
            {
                var likePattern = $"%{search}%";
                query = query.Where(c =>
                    EF.Functions.Like(c.Name, likePattern) ||
                    EF.Functions.Like(c.Description, likePattern));
            }

            var descending = string.Equals(parameters.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            var sortBy = parameters.SortBy?.Trim().ToLowerInvariant();
            query = ApplyCategorySorting(query, sortBy, descending);

            var list = await query.ToListAsync(cancellationToken);

            return new ApiResponseDto<List<Category>>
            {
                RequestFailed = false,
                ResponseCode = HttpStatusCode.OK,
                ErrorMessage = string.Empty,
                Data = list,
            };
        }
        catch (Exception ex)
        {
            return new ApiResponseDto<List<Category>>
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.InternalServerError,
                ErrorMessage = ex.Message,
                Data = [],
            };
        }
    }

    private static IQueryable<Category> ApplyCategorySorting(IQueryable<Category> query, string? sortBy, bool descending)
    {
        return sortBy switch
        {
            "name" => descending ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
            "createdat" => descending ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt),
            _ => descending ? query.OrderByDescending(c => c.CategoryId) : query.OrderBy(c => c.CategoryId)
        };
    }

    public async Task<ApiResponseDto<Category>> AddAsync(
        Category entity,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            await _db.Categories.AddAsync(entity, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            return new ApiResponseDto<Category>
            {
                RequestFailed = false,
                ResponseCode = HttpStatusCode.Created,
                ErrorMessage = string.Empty,
                Data = entity,
            };
        }
        catch (DbUpdateConcurrencyException)
        {
            return new ApiResponseDto<Category>
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.Conflict,
                ErrorMessage = "Concurrency conflict occurred while adding the category.",
                Data = entity,
            };
        }
        catch (DbUpdateException)
        {
            return new ApiResponseDto<Category>
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.BadRequest,
                ErrorMessage = "Failed to add category. Please check the data and try again.",
                Data = entity,
            };
        }
        catch (Exception)
        {
            return new ApiResponseDto<Category>
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.InternalServerError,
                ErrorMessage = "An unexpected error occurred while adding the category.",
                Data = entity,
            };
        }
    }

    public async Task<ApiResponseDto<Category>> UpdateAsync(
        Category entity,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            _db.Categories.Update(entity);
            await _db.SaveChangesAsync(cancellationToken);

            return new ApiResponseDto<Category>
            {
                RequestFailed = false,
                ResponseCode = HttpStatusCode.OK,
                ErrorMessage = string.Empty,
                Data = entity,
            };
        }
        catch (DbUpdateConcurrencyException)
        {
            return new ApiResponseDto<Category>
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.Conflict,
                ErrorMessage = "Concurrency conflict occurred while updating the category.",
                Data = entity,
            };
        }
        catch (DbUpdateException)
        {
            return new ApiResponseDto<Category>
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.BadRequest,
                ErrorMessage = "Failed to update category. Please check the data and try again.",
                Data = entity,
            };
        }
        catch (Exception)
        {
            return new ApiResponseDto<Category>
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.InternalServerError,
                ErrorMessage = "An unexpected error occurred while updating the category.",
                Data = entity,
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
            var category = await _db.Categories.FindAsync(new object[] { id }, cancellationToken);
            if (category == null)
            {
                return new ApiResponseDto<bool>
                {
                    RequestFailed = true,
                    ResponseCode = HttpStatusCode.NotFound,
                    ErrorMessage = "Category not found",
                    Data = false,
                };
            }

            // Soft delete: mark as deleted instead of removing
            category.IsDeleted = true;
            category.DeletedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);
            return new ApiResponseDto<bool>
            {
                RequestFailed = false,
                ResponseCode = HttpStatusCode.NoContent,
                ErrorMessage = string.Empty,
                Data = true,
            };
        }
        catch (DbUpdateConcurrencyException)
        {
            return new ApiResponseDto<bool>
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.Conflict,
                ErrorMessage = "Concurrency conflict occurred while deleting the category.",
                Data = false,
            };
        }
        catch (DbUpdateException)
        {
            return new ApiResponseDto<bool>
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.BadRequest,
                ErrorMessage = "Failed to delete category. Please try again.",
                Data = false,
            };
        }
        catch (Exception)
        {
            return new ApiResponseDto<bool>
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.InternalServerError,
                ErrorMessage = "An unexpected error occurred while deleting the category.",
                Data = false,
            };
        }
    }

    public async Task<ApiResponseDto<bool>> RestoreAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var category = await _db.Categories
                .IgnoreQueryFilters() // Include soft-deleted items
                .FirstOrDefaultAsync(c => c.CategoryId == id, cancellationToken);

            if (category == null)
            {
                return new ApiResponseDto<bool>
                {
                    RequestFailed = true,
                    ResponseCode = HttpStatusCode.NotFound,
                    ErrorMessage = "Category not found",
                    Data = false,
                };
            }

            if (!category.IsDeleted)
            {
                return new ApiResponseDto<bool>
                {
                    RequestFailed = true,
                    ResponseCode = HttpStatusCode.BadRequest,
                    ErrorMessage = "Category is not deleted",
                    Data = false,
                };
            }

            // Restore the category
            category.IsDeleted = false;
            category.DeletedAt = null;

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

    public async Task<ApiResponseDto<List<Category>>> GetDeletedCategoriesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var deletedCategories = await _db
                .Categories
                .IgnoreQueryFilters() // Include soft-deleted items
                .Where(c => c.IsDeleted)
                .Include(c => c.Products)
                .Include(c => c.Sales)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return new ApiResponseDto<List<Category>>
            {
                RequestFailed = false,
                ResponseCode = HttpStatusCode.OK,
                ErrorMessage = string.Empty,
                Data = deletedCategories,
            };
        }
        catch (Exception ex)
        {
            return new ApiResponseDto<List<Category>>
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.InternalServerError,
                ErrorMessage = ex.Message,
                Data = null,
            };
        }
    }

    public async Task<ApiResponseDto<bool>> HardDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var category = await _db.Categories
                .IgnoreQueryFilters() // Include soft-deleted items
                .FirstOrDefaultAsync(c => c.CategoryId == id, cancellationToken);

            if (category == null)
            {
                return new ApiResponseDto<bool>
                {
                    RequestFailed = true,
                    ResponseCode = HttpStatusCode.NotFound,
                    ErrorMessage = "Category not found",
                    Data = false,
                };
            }

            _db.Categories.Remove(category);
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
