using ECommerceApp.RyanW84.Data.DTO;
using ECommerceApp.RyanW84.Data.Models;

namespace ECommerceApp.RyanW84.Interfaces;

public interface ISaleService
{
    // Define methods for sale management
    Task<ApiResponseDto<Sale>> CreateSaleAsync(Sale newSale);
    Task GetSaleAsync();
    Task GetSaleFromSelectionPromptAsync(string prompt);
    Task UpdateSaleAsync();
    Task DeleteSaleAsync();
}
