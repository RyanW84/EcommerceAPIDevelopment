using ECommerceApp.RyanW84.Services.Dtos;

namespace ECommerceApp.RyanW84.Interfaces;

public interface ISalesSummaryService
{
    Task<List<SalesSummaryDto>> GetSalesSummaryAsync(CancellationToken cancellationToken = default);
}
