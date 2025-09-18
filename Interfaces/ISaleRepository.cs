using ECommerceApp.RyanW84.Data.DTO;
using ECommerceApp.RyanW84.Data.Models;

namespace ECommerceApp.RyanW84.Interfaces;

public interface ISaleRepository
    {
  
    Task<ApiResponseDto<Sale?>> GetByIdAsync(
        int id ,
        CancellationToken cancellationToken = default
    );
    Task<ApiResponseDto<List<Sale>>> GetAllSalesAsync(CancellationToken cancellationToken = default);
    Task<ApiResponseDto<Sale>> AddAsync(
        Sale entity ,
        CancellationToken cancellationToken = default
    );
    Task<ApiResponseDto<Sale>> UpdateAsync(
        Sale entity ,
        CancellationToken cancellationToken = default
    );
    Task<ApiResponseDto<bool>> DeleteAsync(int id ,
        CancellationToken cancellationToken = default
    );
    }
