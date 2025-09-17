using ECommerceApp.RyanW84.Data;
using ECommerceApp.RyanW84.Data.DTO;
using ECommerceApp.RyanW84.Data.Models;
using ECommerceApp.RyanW84.Interfaces;

namespace ECommerceApp.RyanW84.Services;

public class SaleService(ECommerceDbContext db) : ISaleService
{
    private readonly ECommerceDbContext _db = db;

    public async Task<ApiResponseDto<Sale>> CreateSaleAsync(Sale newSale)
    {
        await Task.Delay(1000);
        return new ApiResponseDto<Sale>
        {
            RequestFailed = false,
            ErrorMessage = "Sale created successfully",
            Data = newSale,
        };
    }

    public async Task GetSaleAsync()
    {
        await Task.Delay(1000);
    }

    public async Task GetSaleFromSelectionPromptAsync(string prompt)
    {
        await Task.Delay(1000);
    }

    public async Task UpdateSaleAsync()
    {
        await Task.Delay(1000);
    }

    public async Task DeleteSaleAsync()
    {
        await Task.Delay(1000);
    }
}
