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
                .Include(s => s.SaleItems).ThenInclude(si => si.Product)
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

    public async Task<ApiResponseDto<List<Sale>>> GetAllSalesAsync(
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var list = await _db
                .Sales.AsNoTracking()
                .Include(s => s.SaleItems).ThenInclude(si => si.Product)
                .Include(s => s.Categories)
                .ToListAsync(cancellationToken);

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

    public async Task<ApiResponseDto<Sale>> AddAsync(
        Sale entity,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            await _db.Sales.AddAsync(entity, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            return new ApiResponseDto<Sale>
            {
                RequestFailed = false,
                ResponseCode = HttpStatusCode.Created,
                ErrorMessage = string.Empty,
                Data = entity,
            };
        }
        catch (Exception ex)
        {
            return new ApiResponseDto<Sale>
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.InternalServerError,
                ErrorMessage = ex.Message,
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

            return new ApiResponseDto<Sale>
            {
                RequestFailed = false,
                ResponseCode = HttpStatusCode.OK,
                ErrorMessage = string.Empty,
                Data = entity,
            };
        }
        catch (Exception ex)
        {
            return new ApiResponseDto<Sale>
            {
                RequestFailed = true,
                ResponseCode = HttpStatusCode.InternalServerError,
                ErrorMessage = ex.Message,
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

    public async Task<ApiResponseDto<Sale?>> GetByIdWithHistoricalProductsAsync(
        int id,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var sale = await _db
                .Sales.AsNoTracking()
                .Include(s => s.SaleItems).ThenInclude(si => si.Product)
                .Include(s => s.Categories)
                .IgnoreQueryFilters() // Include deleted products
                .FirstOrDefaultAsync(s => s.SaleId == id, cancellationToken);

            // Filter SaleItems to only include products that were available at the time of sale
            if (sale != null && sale.SaleItems != null)
            {
                var saleDate = sale.SaleDate;
                sale.SaleItems = sale.SaleItems
                    .Where(si => si.Product != null && (!si.Product.IsDeleted || si.Product.DeletedAt > saleDate))
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
                .Include(s => s.SaleItems).ThenInclude(si => si.Product)
                .Include(s => s.Categories)
                .IgnoreQueryFilters() // Include deleted products
                .ToListAsync(cancellationToken);

            // Filter SaleItems for each sale to only include products that were available at the time of sale
            foreach (var sale in list)
            {
                if (sale.SaleItems != null)
                {
                    var saleDate = sale.SaleDate;
                    sale.SaleItems = sale.SaleItems
                        .Where(si => si.Product != null && (!si.Product.IsDeleted || si.Product.DeletedAt > saleDate))
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
