using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ECommerceApp.RyanW84.Data.DTO;
using ECommerceApp.RyanW84.Data.Models;

namespace ECommerceApp.RyanW84.Interfaces;

public interface ISaleService
{
    Task<ApiResponseDto<Sale>> CreateSaleAsync(ApiRequestDto<Sale> request, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<Sale>> GetSaleByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PaginatedResponseDto<List<Sale>>> GetSalesAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<Sale>> GetSaleByIdWithHistoricalProductsAsync(int id, CancellationToken cancellationToken = default);
    Task<ApiResponseDto<List<Sale>>> GetHistoricalSalesAsync(CancellationToken cancellationToken = default);
}
