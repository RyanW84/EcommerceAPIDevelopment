using ECommerceApp.RyanW84.Data;
using ECommerceApp.RyanW84.Interfaces;
using ECommerceApp.RyanW84.Services.Dtos;

using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.RyanW84.Services;

public class SalesSummaryService(ECommerceDbContext db): ISalesSummaryService
	{
	private readonly ECommerceDbContext _db = db;

	public async Task<List<SalesSummaryDto>> GetSalesSummaryAsync(CancellationToken cancellationToken = default)
		{
		return await _db.Sales
			.AsNoTracking()
			.Include(s => s.Product)
			.Include(s => s.Category)
			.GroupBy(s => new { ProductName = s.Product.Name , CategoryName = s.Category.Name })
			.Select(g => new SalesSummaryDto
				{
				ProductName = g.Key.ProductName ,
				CategoryName = g.Key.CategoryName ,
				TotalQuantitySold = g.Sum(x => x.Quantity) ,
				TotalRevenue = g.Sum(x => x.TotalAmount) ,
				LastSaleDate = g.Max(x => x.SaleDate)
				})
			.OrderByDescending(x => x.TotalRevenue)
			.ToListAsync(cancellationToken);
		}
	}
