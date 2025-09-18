using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ECommerceApp.RyanW84.Data;
using ECommerceApp.RyanW84.Data.DTO;
using ECommerceApp.RyanW84.Data.Models;
using ECommerceApp.RyanW84.Interfaces;

namespace ECommerceApp.RyanW84.Services;

public class SaleService(ECommerceDbContext db) : ISaleService
{
    private readonly ECommerceDbContext _db = db;

    public async Task<ApiResponseDto<Sale>> CreateSaleAsync(ApiRequestDto<Sale> request, CancellationToken cancellationToken = default)
    {
        var payload = request?.Payload;
        if (payload is null || payload.SaleItems is null || !payload.SaleItems.Any())
            return new ApiResponseDto<Sale>
            {
                RequestFailed = true,
                ResponseCode = System.Net.HttpStatusCode.BadRequest,
                ErrorMessage = "Invalid request or no items provided."
            };

        // Collect product ids and load products
        var productIds = payload.SaleItems.Select(si => si.ProductId).Distinct().ToList();
        var products = await _db.Products.Where(p => productIds.Contains(p.ProductId)).ToListAsync(cancellationToken);

        // Validate products and stock
        foreach (var item in payload.SaleItems)
        {
            var product = products.FirstOrDefault(p => p.ProductId == item.ProductId);
            if (product is null)
                return new ApiResponseDto<Sale>
                {
                    RequestFailed = true,
                    ResponseCode = System.Net.HttpStatusCode.BadRequest,
                    ErrorMessage = $"Product {item.ProductId} not found."
                };
            if (product.Stock < item.Quantity)
                return new ApiResponseDto<Sale>
                {
                    RequestFailed = true,
                    ResponseCode = System.Net.HttpStatusCode.Conflict,
                    ErrorMessage = $"Insufficient stock for product {product.ProductId}."
                };
        }

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // Create new Sale entity (do not attach incoming payload directly)
            var sale = new Sale
            {
                SaleDate = payload.SaleDate,
                CustomerName = payload.CustomerName,
                CustomerEmail = payload.CustomerEmail,
                CustomerAddress = payload.CustomerAddress,
                TotalAmount = 0m
            };

            _db.Sales.Add(sale);
            await _db.SaveChangesAsync(cancellationToken); // get SaleId

            decimal total = 0m;
            foreach (var item in payload.SaleItems)
            {
                var product = products.First(p => p.ProductId == item.ProductId);
                var unitPrice = product.Price;
                var lineTotal = unitPrice * item.Quantity;
                total += lineTotal;

                var saleItem = new SaleItem
                {
                    SaleId = sale.SaleId,
                    ProductId = product.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = unitPrice
                };
                _db.SaleItems.Add(saleItem);

                product.Stock -= item.Quantity;
                _db.Products.Update(product);
            }

            sale.TotalAmount = total;
            _db.Sales.Update(sale);

            await _db.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            // Load items for response
            await _db.Entry(sale).Collection(s => s.SaleItems).Query().Include(si => si.Product).LoadAsync(cancellationToken);

            return new ApiResponseDto<Sale>
            {
                RequestFailed = false,
                ResponseCode = System.Net.HttpStatusCode.Created,
                ErrorMessage = string.Empty,
                Data = sale
            };
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(cancellationToken);
            return new ApiResponseDto<Sale>
            {
                RequestFailed = true,
                ResponseCode = System.Net.HttpStatusCode.InternalServerError,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<ApiResponseDto<Sale>> GetSaleByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var sale = await _db.Sales
            .AsNoTracking()
            .Include(s => s.SaleItems)
                .ThenInclude(si => si.Product)
            .FirstOrDefaultAsync(s => s.SaleId == id, cancellationToken);

        if (sale is null)
            return new ApiResponseDto<Sale>
            {
                RequestFailed = true,
                ResponseCode = System.Net.HttpStatusCode.NotFound,
                ErrorMessage = $"Sale {id} not found."
            };

        return new ApiResponseDto<Sale>
        {
            RequestFailed = false,
            ResponseCode = System.Net.HttpStatusCode.OK,
            ErrorMessage = string.Empty,
            Data = sale
        };
    }

    public async Task<ApiResponseDto<System.Collections.Generic.List<Sale>>> GetSalesAsync(CancellationToken cancellationToken = default)
    {
        var list = await _db.Sales
            .AsNoTracking()
            .Include(s => s.SaleItems)
                .ThenInclude(si => si.Product)
            .ToListAsync(cancellationToken);

        return new ApiResponseDto<System.Collections.Generic.List<Sale>>
        {
            RequestFailed = false,
            ResponseCode = System.Net.HttpStatusCode.OK,
            ErrorMessage = string.Empty,
            Data = list
        };
    }
}
