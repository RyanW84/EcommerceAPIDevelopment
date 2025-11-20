using ECommerceApp.RyanW84.Data;
using ECommerceApp.RyanW84.Interfaces;
using ECommerceApp.RyanW84.Services.Dtos;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.RyanW84.Services;

public class SalesSummaryService(ECommerceDbContext db) : ISalesSummaryService
{
    private readonly ECommerceDbContext _db = db;

    public async Task<List<SalesSummaryDto>> GetSalesSummaryAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _db
            .Sales.AsNoTracking()
            .Include(s => s.SaleItems)
            .ThenInclude(si => si.Product)
            .ThenInclude(p => p.Category)
            .SelectMany(s => s.SaleItems, (sale, item) => new { Sale = sale, Item = item })
            .GroupBy(x => new
            {
                ProductName = x.Item.Product.Name,
                CategoryName = x.Item.Product.Category.Name,
            })
            .Select(g => new SalesSummaryDto
            {
                ProductName = g.Key.ProductName,
                CategoryName = g.Key.CategoryName,
                TotalQuantitySold = g.Sum(x => x.Item.Quantity),
                TotalRevenue = g.Sum(x => x.Item.LineTotal),
                LastSaleDate = g.Max(x => x.Sale.SaleDate),
            })
            .OrderByDescending(x => x.TotalRevenue)
            .ToListAsync(cancellationToken);
    }
}
