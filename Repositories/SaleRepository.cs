using System;
using System.Net;
using ECommerceApp.RyanW84.Data;
using ECommerceApp.RyanW84.Data.DTO;
using ECommerceApp.RyanW84.Data.Models;
using ECommerceApp.RyanW84.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.RyanW84.Repositories;

public class SaleRepository(ECommerceDbContext db) : ISaleRepository
{
    private readonly ECommerceDbContext _db = db;

    public async Task<ApiResponseDto<Sale?>> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var sale = await _db
                .Sales.AsNoTracking()
                .Include(s => s.SaleItems)
                .ThenInclude(si => si.Product)
                .Include(s => s.Categories)
                .FirstOrDefaultAsync(s => s.SaleId == id, cancellationToken);

            return new ApiResponseDto<Sale?>
            {
                RequestFailed = false,
                ResponseCode = HttpStatusCode.OK,
                ErrorMessage = string.Empty,
                Data = sale,
            };
        }
        catch (Exception ex)
        {
            return new ApiResponseDto<Sale?>
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.InternalServerError,
                ErrorMessage = ex.Message,
                Data = null,
            };
        }
    }

    private static IQueryable<Sale> ApplySaleSorting(
        IQueryable<Sale> query,
        string? sortBy,
        bool descending
    )
    {
        return sortBy switch
        {
            "customername" => descending
                ? query.OrderByDescending(s => s.CustomerName)
                : query.OrderBy(s => s.CustomerName),
            "totalamount" => descending
                ? query.OrderByDescending(s => s.TotalAmount)
                : query.OrderBy(s => s.TotalAmount),
            "saledate" => descending
                ? query.OrderByDescending(s => s.SaleDate)
                : query.OrderBy(s => s.SaleDate),
            _ => descending
                ? query.OrderByDescending(s => s.SaleDate)
                : query.OrderBy(s => s.SaleDate),
        };
    }

    public async Task<PaginatedResponseDto<List<Sale>>> GetAllSalesAsync(
        SaleQueryParameters parameters,
        CancellationToken cancellationToken = default
    )
    {
        parameters ??= new SaleQueryParameters();

        var page = Math.Max(parameters.Page, 1);
        var pageSize = Math.Clamp(parameters.PageSize, 1, 100);

        try
        {
            var query = _db
                .Sales.AsNoTracking()
                .Include(s => s.SaleItems)
                .ThenInclude(si => si.Product)
                .Include(s => s.Categories)
                .AsQueryable();

            if (parameters.StartDate is { } startDate)
            {
                query = query.Where(s => s.SaleDate >= startDate);
            }

            if (parameters.EndDate is { } endDate)
            {
                query = query.Where(s => s.SaleDate <= endDate);
            }

            var customerName = parameters.CustomerName?.Trim();
            if (!string.IsNullOrEmpty(customerName))
            {
                var likePattern = $"%{customerName}%";
                query = query.Where(s => EF.Functions.Like(s.CustomerName, likePattern));
            }

            var customerEmail = parameters.CustomerEmail?.Trim();
            if (!string.IsNullOrEmpty(customerEmail))
            {
                query = query.Where(s => s.CustomerEmail == customerEmail);
            }

            var descending = string.Equals(
                parameters.SortDirection,
                "desc",
                StringComparison.OrdinalIgnoreCase
            );
            var sortBy = parameters.SortBy?.Trim().ToLowerInvariant();
            var orderedQuery = ApplySaleSorting(query, sortBy, descending);

            var totalCount = await orderedQuery.CountAsync(cancellationToken);
            var sales = await orderedQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PaginatedResponseDto<List<Sale>>
            {
                RequestFailed = false,
                ResponseCode = HttpStatusCode.OK,
                ErrorMessage = string.Empty,
                Data = sales,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount,
            };
        }
        catch (Exception ex)
        {
            return new PaginatedResponseDto<List<Sale>>
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.InternalServerError,
                ErrorMessage = ex.Message,
                Data = [],
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = 0,
            };
        }
    }

    public async Task<ApiResponseDto<Sale>> AddAsync(
        Sale entity,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            await _db.Sales.AddAsync(entity, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            // Reload the sale with related SaleItems and Categories
            var createdSale = await _db
                .Sales.AsNoTracking()
                .Include(s => s.SaleItems)
                .ThenInclude(si => si.Product)
                .Include(s => s.Categories)
                .FirstOrDefaultAsync(s => s.SaleId == entity.SaleId, cancellationToken);

            return new ApiResponseDto<Sale>
            {
                RequestFailed = false,
                ResponseCode = HttpStatusCode.Created,
                ErrorMessage = string.Empty,
                Data = createdSale,
            };
        }
        catch (DbUpdateConcurrencyException)
        {
            return new ApiResponseDto<Sale>
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.Conflict,
                ErrorMessage = "Concurrency conflict occurred while adding the sale.",
                Data = null,
            };
        }
        catch (DbUpdateException)
        {
            return new ApiResponseDto<Sale>
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.BadRequest,
                ErrorMessage = "Failed to add sale. Please check the data and try again.",
                Data = null,
            };
        }
        catch (Exception)
        {
            return new ApiResponseDto<Sale>
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.InternalServerError,
                ErrorMessage = "An unexpected error occurred while adding the sale.",
                Data = null,
            };
        }
    }

    public async Task<ApiResponseDto<Sale>> UpdateAsync(
        Sale entity,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            _db.Sales.Update(entity);
            await _db.SaveChangesAsync(cancellationToken);

            // Reload the sale with related SaleItems and Categories
            var updatedSale = await _db
                .Sales.AsNoTracking()
                .Include(s => s.SaleItems)
                .ThenInclude(si => si.Product)
                .Include(s => s.Categories)
                .FirstOrDefaultAsync(s => s.SaleId == entity.SaleId, cancellationToken);

            return new ApiResponseDto<Sale>
            {
                RequestFailed = false,
                ResponseCode = HttpStatusCode.OK,
                ErrorMessage = string.Empty,
                Data = updatedSale,
            };
        }
        catch (DbUpdateConcurrencyException)
        {
            return new ApiResponseDto<Sale>
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.Conflict,
                ErrorMessage = "Concurrency conflict occurred while updating the sale.",
                Data = null,
            };
        }
        catch (DbUpdateException)
        {
            return new ApiResponseDto<Sale>
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.BadRequest,
                ErrorMessage = "Failed to update sale. Please check the data and try again.",
                Data = null,
            };
        }
        catch (Exception)
        {
            return new ApiResponseDto<Sale>
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.InternalServerError,
                ErrorMessage = "An unexpected error occurred while updating the sale.",
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
            var sale = await _db.Sales.FindAsync(new object[] { id }, cancellationToken);
            if (sale == null)
            {
                return new ApiResponseDto<bool>
                {
                    RequestFailed = true,
                    ResponseCode = HttpStatusCode.NotFound,
                    ErrorMessage = "Sale not found",
                    Data = false,
                };
            }
            _db.Sales.Remove(sale);
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
                ErrorMessage = "Concurrency conflict occurred while deleting the sale.",
                Data = false,
            };
        }
        catch (DbUpdateException)
        {
            return new ApiResponseDto<bool>
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.BadRequest,
                ErrorMessage = "Failed to delete sale. Please try again.",
                Data = false,
            };
        }
        catch (Exception)
        {
            return new ApiResponseDto<bool>
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.InternalServerError,
                ErrorMessage = "An unexpected error occurred while deleting the sale.",
                Data = false,
            };
        }
    }

    public async Task<ApiResponseDto<Sale?>> GetByIdWithHistoricalProductsAsync(
        int id,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var sale = await _db
                .Sales.AsNoTracking()
                .Include(s => s.SaleItems)
                .ThenInclude(si => si.Product)
                .Include(s => s.Categories)
                .IgnoreQueryFilters() // Include deleted products
                .FirstOrDefaultAsync(s => s.SaleId == id, cancellationToken);

            // Filter SaleItems to only include products that were available at the time of sale
            if (sale != null && sale.SaleItems != null)
            {
                var saleDate = sale.SaleDate;
                sale.SaleItems = sale
                    .SaleItems.Where(si =>
                        si.Product != null
                        && (!si.Product.IsDeleted || si.Product.DeletedAt > saleDate)
                    )
                    .ToList();
            }

            return new ApiResponseDto<Sale?>
            {
                RequestFailed = false,
                ResponseCode = HttpStatusCode.OK,
                ErrorMessage = string.Empty,
                Data = sale,
            };
        }
        catch (Exception ex)
        {
            return new ApiResponseDto<Sale?>
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.InternalServerError,
                ErrorMessage = ex.Message,
                Data = null,
            };
        }
    }

    public async Task<ApiResponseDto<List<Sale>>> GetHistoricalSalesAsync(
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var list = await _db
                .Sales.AsNoTracking()
                .Include(s => s.SaleItems)
                .ThenInclude(si => si.Product)
                .Include(s => s.Categories)
                .IgnoreQueryFilters() // Include deleted products
                .ToListAsync(cancellationToken);

            // Filter SaleItems for each sale to only include products that were available at the time of sale
            foreach (var sale in list)
            {
                if (sale.SaleItems != null)
                {
                    var saleDate = sale.SaleDate;
                    sale.SaleItems = sale
                        .SaleItems.Where(si =>
                            si.Product != null
                            && (!si.Product.IsDeleted || si.Product.DeletedAt > saleDate)
                        )
                        .ToList();
                }
            }

            return new ApiResponseDto<List<Sale>>
            {
                RequestFailed = false,
                ResponseCode = HttpStatusCode.OK,
                ErrorMessage = string.Empty,
                Data = list,
            };
        }
        catch (Exception ex)
        {
            return new ApiResponseDto<List<Sale>>
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.InternalServerError,
                ErrorMessage = ex.Message,
                Data = [],
            };
        }
    }
}
