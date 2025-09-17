using ECommerceApp.RyanW84.Data;
using ECommerceApp.RyanW84.Interfaces;

namespace ECommerceApp.RyanW84.Services;

public class CategoryService(ECommerceDbContext db) : ICategoryService
{
    private readonly ECommerceDbContext _db = db;

    public async Task CreateCategoryAsync()
    {
        await Task.Delay(1000);
    }

    public async Task GetCategoryAsync()
    {
        await Task.Delay(1000);
    }

    public async Task GetCategoryFromSelectionPromptAsync(string prompt)
    {
        await Task.Delay(1000);
    }

    public async Task UpdateCategoryAsync()
    {
        await Task.Delay(1000);
    }

    public async Task DeleteCategoryAsync()
    {
        await Task.Delay(1000);
    }
}
