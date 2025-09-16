namespace ECommerceApp.RyanW84.Interfaces;

public interface ISaleService
{
    // Define methods for sale management
    Task CreateSaleAsync();
    Task GetSaleAsync();
    Task GetSaleFromSelectionPromptAsync(string prompt);
    Task UpdateSaleAsync();
    Task DeleteSaleAsync();
}
