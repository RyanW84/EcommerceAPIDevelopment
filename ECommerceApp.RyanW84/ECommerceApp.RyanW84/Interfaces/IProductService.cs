namespace ECommerceApp.RyanW84.Interfaces;

public interface IProductService
	{
	// Define methods for product management
	Task CreateProductAsync();
	Task GetProductAsync();
	Task GetProductFromSelectionPromptAsync(string prompt);
	Task UpdateProductAsync();
	Task DeleteProductAsync();
    }
