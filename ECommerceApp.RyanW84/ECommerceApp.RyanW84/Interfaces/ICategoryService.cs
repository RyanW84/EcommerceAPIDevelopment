namespace ECommerceApp.RyanW84.Interfaces;

public interface ICategoryService
{
    // Define methods for category management
    Task CreateCategoryAsync();
    Task GetCategoryAsync();
    Task GetCategoryFromSelectionPromptAsync(string prompt);
    Task UpdateCategoryAsync();
    Task DeleteCategoryAsync();
}
