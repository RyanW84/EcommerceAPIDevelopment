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
    Task<ApiResponseDto<List<Sale>>> GetSalesAsync(CancellationToken cancellationToken = default);
}
