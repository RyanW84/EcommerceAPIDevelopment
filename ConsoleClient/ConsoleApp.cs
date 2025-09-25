using ECommerceApp.ConsoleClient.Models;
using ECommerceApp.ConsoleClient.Options;
using ECommerceApp.ConsoleClient.Services;
using Spectre.Console;

namespace ECommerceApp.ConsoleClient;

public class ConsoleApp
{
    private static class Menu
    {
        public const string Products = "Products";
        public const string Categories = "Categories";
        public const string Sales = "Sales";
        public const string Settings = "Settings";
        public const string Exit = "Exit";
        public const string Browse = "Browse";
        public const string ViewDetails = "View details";
        public const string Create = "Create";
        public const string Update = "Update";
        public const string Delete = "Delete";
        public const string Back = "Back";
        public const string Ascending = "Ascending";
        public const string Descending = "Descending";
        public const string Items = "Items";
    }

    private readonly ECommerceApiClient _apiClient;
    private readonly ApiClientOptions _options;

    public ConsoleApp(ECommerceApiClient apiClient, ApiClientOptions options)
    {
        _apiClient = apiClient;
        _options = options;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        Console.Title = "ECommerce API Console";
        Console.CancelKeyPress += (_, args) => args.Cancel = true;

        ShowSplash();

        var keepRunning = true;
        while (keepRunning)
        {
            var choice = AnsiConsole.Prompt(new SelectionPrompt<string>()
                .Title("[cyan]Select an area to explore[/]")
                .HighlightStyle(Style.Parse("green bold"))
                .AddChoices(Menu.Products, Menu.Categories, Menu.Sales, Menu.Settings, Menu.Exit));

            switch (choice)
            {
                case Menu.Products:
                    await ProductsMenuAsync(cancellationToken);
                    break;
                case Menu.Categories:
                    await CategoriesMenuAsync(cancellationToken);
                    break;
                case Menu.Sales:
                    await SalesMenuAsync(cancellationToken);
                    break;
                case Menu.Settings:
                    ShowSettings();
                    break;
                case Menu.Exit:
                    keepRunning = false;
                    break;
            }
        }

        AnsiConsole.MarkupLine("\n[green]Thanks for using the console client. Goodbye![/]");
    }

    private static void ShowSplash()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new FigletText("E-Commerce").Color(Color.Aqua));

        var panel = new Panel(@"Interact with the ECommerce API.
- Browse products with rich filtering
- Manage categories and inventory
- Inspect sales and drill into order detail")
        {
            Header = new PanelHeader("Welcome", Justify.Center),
            Border = BoxBorder.Rounded,
            Expand = true,
            Padding = new Padding(1)
        };

        panel.BorderStyle = new Style(Color.LightGreen);
        AnsiConsole.Write(panel);
        AnsiConsole.MarkupLine("[grey]Tip: Press Ctrl+C at any prompt to cancel.[/]");
        AnsiConsole.WriteLine();
    }

    private void ShowSettings()
    {
        var table = new Table().Border(TableBorder.Rounded).Title("[yellow]Current API Settings[/]");
        table.AddColumn("Setting");
        table.AddColumn("Value");
        table.AddRow("Base Address", _options.BaseAddress);
        table.AddRow("Timeout", _options.Timeout.ToString());
        table.AddRow("Ignore SSL Errors", _options.IgnoreCertificateErrors ? "Yes" : "No");
        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    #region Products

    private async Task ProductsMenuAsync(CancellationToken cancellationToken)
    {
        var keepGoing = true;
        while (keepGoing)
        {
            var choice = AnsiConsole.Prompt(new SelectionPrompt<string>()
                .Title("[green]Products[/]")
                .AddChoices(Menu.Browse, Menu.ViewDetails, Menu.Create, Menu.Update, Menu.Delete, Menu.Back));

            switch (choice)
            {
                case Menu.Browse:
                    await BrowseProductsAsync(cancellationToken);
                    break;
                case Menu.ViewDetails:
                    await ViewProductDetailsAsync(cancellationToken);
                    break;
                case Menu.Create:
                    await CreateProductAsync(cancellationToken);
                    break;
                case Menu.Update:
                    await UpdateProductAsync(cancellationToken);
                    break;
                case Menu.Delete:
                    await DeleteProductAsync(cancellationToken);
                    break;
                case Menu.Back:
                    keepGoing = false;
                    break;
            }
        }
    }

    private async Task BrowseProductsAsync(CancellationToken cancellationToken)
    {
        var query = PromptForProductQuery();

        while (true)
        {
            var result = await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync("Loading products...", async _ => await _apiClient.GetProductsAsync(query, cancellationToken));

            if (!result.Success)
            {
                RenderError(result.Message);
                return;
            }

            if (result.Data?.Data is null || result.Data.Data.Count == 0)
            {
                RenderWarning("No products matched the current filters.");
                return;
            }

            RenderProductTable(result.Data.Data, result.Data);

            // Show pagination menu
            var newQuery = await ShowProductPaginationMenuAsync(result.Data, query, (q) => Task.CompletedTask);
            if (newQuery == null)
            {
                // User chose to go back
                return;
            }

            query = newQuery;
        }
    }

    private async Task ViewProductDetailsAsync(CancellationToken cancellationToken)
    {
        var id = AnsiConsole.Ask<int>("Enter the [green]product ID[/]:");
        var result = await _apiClient.GetProductByIdAsync(id, cancellationToken);
        if (!result.Success || result.Data is null)
        {
            RenderError(result.Message.Length == 0 ? $"Product {id} not found." : result.Message);
            return;
        }

        RenderProductDetails(result.Data);
    }

    private async Task CreateProductAsync(CancellationToken cancellationToken)
    {
        var product = await PromptForProductAsync(cancellationToken);
        if (product is null)
        {
            return;
        }

        var result = await _apiClient.CreateProductAsync(product, cancellationToken);
        if (!result.Success)
        {
            RenderError(result.Message);
            return;
        }

        RenderSuccess($"Product '{result.Data?.Name}' created successfully with ID {result.Data?.ProductId}.");
    }

    private async Task UpdateProductAsync(CancellationToken cancellationToken)
    {
        var id = AnsiConsole.Ask<int>("Enter the [green]product ID[/] to update:");
        var existing = await _apiClient.GetProductByIdAsync(id, cancellationToken);
        if (!existing.Success || existing.Data is null)
        {
            RenderError(existing.Message.Length == 0 ? "Product not found." : existing.Message);
            return;
        }

        var updated = await PromptForProductAsync(cancellationToken, existing.Data);
        if (updated is null)
        {
            return;
        }

        var result = await _apiClient.UpdateProductAsync(id, updated, cancellationToken);
        if (!result.Success)
        {
            RenderError(result.Message);
            return;
        }

        RenderSuccess($"Product '{result.Data?.Name}' updated successfully.");
    }

    private async Task DeleteProductAsync(CancellationToken cancellationToken)
    {
        var id = AnsiConsole.Ask<int>("Enter the [green]product ID[/] to delete:");
        if (!AnsiConsole.Confirm("Are you sure you want to delete this product?", false))
        {
            return;
        }

        var result = await _apiClient.DeleteProductAsync(id, cancellationToken);
        if (!result.Success)
        {
            RenderError(result.Message);
            return;
        }

        RenderSuccess("Product deleted successfully.");
    }

    private static ProductQuery PromptForProductQuery()
    {
        var applyFilters = AnsiConsole.Confirm("Do you want to apply filters?", false);

        if (!applyFilters)
        {
            return new ProductQuery();
        }

        var search = PromptOptionalString("Search term (blank to skip):");
        var categoryId = PromptOptionalInt("Category ID (blank to skip):");
        var minPrice = PromptOptionalDecimal("Minimum price:");
        var maxPrice = PromptOptionalDecimal("Maximum price:");
        var pageSize = PromptOptionalInt("Page size (default 10):") ?? 10;

        var sort = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title("Sort by")
            .AddChoices("Default", "Name", "Price", "Stock", "CreatedOn"));

        var direction = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title("Sort direction")
            .AddChoices(Menu.Ascending, Menu.Descending));

        return new ProductQuery(
            Page: 1,
            PageSize: pageSize,
            Search: search,
            MinPrice: minPrice,
            MaxPrice: maxPrice,
            CategoryId: categoryId,
            SortBy: sort == "Default" ? null : sort.ToLowerInvariant(),
            SortDirection: direction == Menu.Ascending ? "asc" : "desc");
    }

    private async Task<Product?> PromptForProductAsync(CancellationToken cancellationToken, Product? existing = null)
    {
        var namePrompt = new TextPrompt<string>("Product name:")
            .DefaultValue(existing?.Name ?? string.Empty)
            .Validate(value => string.IsNullOrWhiteSpace(value)
                ? ValidationResult.Error("Name is required.")
                : ValidationResult.Success());

        var descriptionPrompt = new TextPrompt<string>("Description:")
            .DefaultValue(existing?.Description ?? string.Empty)
            .Validate(value => string.IsNullOrWhiteSpace(value)
                ? ValidationResult.Error("Description is required.")
                : ValidationResult.Success());

        var pricePrompt = new TextPrompt<decimal>("Price:")
            .DefaultValue(existing?.Price ?? 0m)
            .Validate(value => value > 0 ? ValidationResult.Success() : ValidationResult.Error("Price must be positive."));

        var stockPrompt = new TextPrompt<int>("Stock quantity:")
            .DefaultValue(existing?.Stock ?? 0)
            .Validate(value => value >= 0 ? ValidationResult.Success() : ValidationResult.Error("Stock cannot be negative."));

        var categories = await LoadCategoriesAsync(cancellationToken);
        if (categories.Length == 0)
        {
            RenderError("No categories available. Create a category first.");
            return null;
        }

        var categoryPrompt = new SelectionPrompt<Category>()
            .Title("Select category")
            .UseConverter(c => $"{c.CategoryId}: {c.Name}")
            .AddChoices(categories)
            .PageSize(10);

        var name = AnsiConsole.Prompt(namePrompt);
        var description = AnsiConsole.Prompt(descriptionPrompt);
        var price = AnsiConsole.Prompt(pricePrompt);
        var stock = AnsiConsole.Prompt(stockPrompt);
        var category = AnsiConsole.Prompt(categoryPrompt);
        var isActive = AnsiConsole.Confirm("Is the product active?", existing?.IsActive ?? true);

        return new Product
        {
            Name = name,
            Description = description,
            Price = price,
            Stock = stock,
            CategoryId = category.CategoryId,
            Category = category,
            IsActive = isActive
        };
    }

    private async Task<Category[]> LoadCategoriesAsync(CancellationToken cancellationToken)
    {
        var result = await _apiClient.GetCategoriesAsync(new CategoryQuery(), cancellationToken);
        if (!result.Success || result.Data is null)
        {
            RenderError(result.Message.Length == 0 ? "Unable to load categories." : result.Message);
            return Array.Empty<Category>();
        }

        return result.Data.OrderBy(c => c.Name, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static void RenderProductTable(IReadOnlyCollection<Product> products, PaginatedResponse<List<Product>>? metadata)
    {
        var table = new Table().Border(TableBorder.Rounded).Title("[aqua]Products[/]");
        table.AddColumn("ID");
        table.AddColumn("Name");
        table.AddColumn("Category");
        table.AddColumn("Price");
        table.AddColumn("Stock");
        table.AddColumn("Active");

        foreach (var product in products)
        {
            table.AddRow(
                product.ProductId.ToString(),
                Markup.Escape(product.Name),
                Markup.Escape(product.Category?.Name ?? "-"),
                product.Price.ToString("C"),
                product.Stock.ToString(),
                product.IsActive ? "[green]Yes[/]" : "[red]No[/]");
        }

        if (metadata is not null)
        {
            table.Caption($"Page {metadata.CurrentPage} of {metadata.TotalPages} (Total {metadata.TotalCount})");
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    private static void RenderProductDetails(Product product)
    {
        var grid = new Grid();
        grid.AddColumn(new GridColumn().NoWrap());
        grid.AddColumn(new GridColumn());

        grid.AddRow("ID", product.ProductId.ToString());
        grid.AddRow("Name", Markup.Escape(product.Name));
        grid.AddRow("Description", Markup.Escape(product.Description));
        grid.AddRow("Category", Markup.Escape(product.Category?.Name ?? "-"));
        grid.AddRow("Price", product.Price.ToString("C"));
        grid.AddRow("Stock", product.Stock.ToString());
        grid.AddRow("Active", product.IsActive ? "Yes" : "No");
        grid.AddRow("Created", product.CreatedAt.ToString("u"));
        if (product.UpdatedAt is not null)
        {
            grid.AddRow("Updated", product.UpdatedAt.Value.ToString("u"));
        }

        var panel = new Panel(grid)
        {
            Header = new PanelHeader($"Product {product.ProductId}", Justify.Center),
            Border = BoxBorder.Rounded,
            Padding = new Padding(1)
        };

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
    }

    #endregion

    #region Categories

    private async Task CategoriesMenuAsync(CancellationToken cancellationToken)
    {
        var keepGoing = true;
        while (keepGoing)
        {
            var choice = AnsiConsole.Prompt(new SelectionPrompt<string>()
                .Title("[green]Categories[/]")
                .AddChoices(Menu.Browse, Menu.Create, Menu.Update, Menu.Delete, Menu.Back));

            switch (choice)
            {
                case Menu.Browse:
                    await BrowseCategoriesAsync(cancellationToken);
                    break;
                case Menu.Create:
                    await CreateCategoryAsync(cancellationToken);
                    break;
                case Menu.Update:
                    await UpdateCategoryAsync(cancellationToken);
                    break;
                case Menu.Delete:
                    await DeleteCategoryAsync(cancellationToken);
                    break;
                case Menu.Back:
                    keepGoing = false;
                    break;
            }
        }
    }

    private async Task BrowseCategoriesAsync(CancellationToken cancellationToken)
    {
        var applyFilters = AnsiConsole.Confirm("Do you want to apply filters?", false);

        CategoryQuery query;
        if (!applyFilters)
        {
            query = new CategoryQuery();
        }
        else
        {
            var search = PromptOptionalString("Search term (blank to skip):");
            var includeDeleted = AnsiConsole.Confirm("Include deleted categories?", false);
            var sort = AnsiConsole.Prompt(new SelectionPrompt<string>()
                .Title("Sort by")
                .AddChoices("Name", "CreatedOn"));
            var direction = AnsiConsole.Prompt(new SelectionPrompt<string>()
                .Title("Sort direction")
                .AddChoices(Menu.Ascending, Menu.Descending));

            query = new CategoryQuery(
                Page: 1,
                PageSize: 10,
                Search: search,
                SortBy: sort.ToLowerInvariant(),
                SortDirection: direction == Menu.Ascending ? "asc" : "desc",
                IncludeDeleted: includeDeleted);
        }

        // For now, fetch all categories since server doesn't support pagination
        var result = await _apiClient.GetCategoriesAsync(query, cancellationToken);
        if (!result.Success || result.Data is null || result.Data.Count == 0)
        {
            RenderWarning(result.Message.Length == 0 ? "No categories found." : result.Message);
            return;
        }

        // Client-side pagination
        var allCategories = result.Data;
        var totalCount = allCategories.Count;
        var totalPages = (int)Math.Ceiling((double)totalCount / query.PageSize);

        while (true)
        {
            var startIndex = (query.Page - 1) * query.PageSize;
            var endIndex = Math.Min(startIndex + query.PageSize, totalCount);
            var pageCategories = allCategories.Skip(startIndex).Take(query.PageSize).ToList();

            var table = new Table().Border(TableBorder.Rounded).Title("[aqua]Categories[/]");
            table.AddColumn("ID");
            table.AddColumn("Name");
            table.AddColumn("Description");
            table.AddColumn("Deleted");
            table.AddColumn("Created");

            foreach (var category in pageCategories)
            {
                table.AddRow(
                    category.CategoryId.ToString(),
                    Markup.Escape(category.Name),
                    Markup.Escape(category.Description),
                    category.IsDeleted ? "[red]Yes[/]" : "[green]No[/]",
                    category.CreatedAt.ToString("yyyy-MM-dd"));
            }

            table.Caption($"Page {query.Page} of {totalPages} (Total {totalCount})");
            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();

            // Show pagination menu
            var newQuery = await ShowCategoryPaginationMenuAsync(query.Page, totalPages, totalCount, query, (q) => Task.CompletedTask);
            if (newQuery == null)
            {
                // User chose to go back
                return;
            }

            query = newQuery;
        }
    }

    private async Task CreateCategoryAsync(CancellationToken cancellationToken)
    {
        var category = PromptForCategory();
        if (category is null)
        {
            return;
        }

        var result = await _apiClient.CreateCategoryAsync(category, cancellationToken);
        if (!result.Success)
        {
            RenderError(result.Message);
            return;
        }

        RenderSuccess($"Category '{result.Data?.Name}' created successfully.");
    }

    private async Task UpdateCategoryAsync(CancellationToken cancellationToken)
    {
        var id = AnsiConsole.Ask<int>("Enter the [green]category ID[/] to update:");
        var categories = await _apiClient.GetCategoriesAsync(new CategoryQuery(IncludeDeleted: true), cancellationToken);
        var existing = categories.Data?.FirstOrDefault(c => c.CategoryId == id);
        if (!categories.Success || existing is null)
        {
            RenderError("Category not found.");
            return;
        }

        var updated = PromptForCategory(existing);
        if (updated is null)
        {
            return;
        }

        var result = await _apiClient.UpdateCategoryAsync(id, updated, cancellationToken);
        if (!result.Success)
        {
            RenderError(result.Message);
            return;
        }

        RenderSuccess($"Category '{result.Data?.Name}' updated successfully.");
    }

    private async Task DeleteCategoryAsync(CancellationToken cancellationToken)
    {
        var id = AnsiConsole.Ask<int>("Enter the [green]category ID[/] to delete:");
        if (!AnsiConsole.Confirm("Delete this category?", false))
        {
            return;
        }

        var result = await _apiClient.DeleteCategoryAsync(id, cancellationToken);
        if (!result.Success)
        {
            RenderError(result.Message);
            return;
        }

        RenderSuccess("Category deleted successfully.");
    }

    private static Category? PromptForCategory(Category? existing = null)
    {
        var namePrompt = new TextPrompt<string>("Category name:")
            .DefaultValue(existing?.Name ?? string.Empty)
            .Validate(value => string.IsNullOrWhiteSpace(value)
                ? ValidationResult.Error("Name is required.")
                : ValidationResult.Success());

        var descriptionPrompt = new TextPrompt<string>("Description:")
            .DefaultValue(existing?.Description ?? string.Empty)
            .Validate(value => string.IsNullOrWhiteSpace(value)
                ? ValidationResult.Error("Description is required.")
                : ValidationResult.Success());

        var name = AnsiConsole.Prompt(namePrompt);
        var description = AnsiConsole.Prompt(descriptionPrompt);

        return new Category
        {
            Name = name,
            Description = description
        };
    }

    #endregion

    #region Sales

    private async Task SalesMenuAsync(CancellationToken cancellationToken)
    {
        var keepGoing = true;
        while (keepGoing)
        {
            var choice = AnsiConsole.Prompt(new SelectionPrompt<string>()
                .Title("[green]Sales[/]")
                .AddChoices(Menu.Browse, Menu.ViewDetails, Menu.Back));

            switch (choice)
            {
                case Menu.Browse:
                    await BrowseSalesAsync(cancellationToken);
                    break;
                case Menu.ViewDetails:
                    await ViewSaleDetailsAsync(cancellationToken);
                    break;
                case Menu.Back:
                    keepGoing = false;
                    break;
            }
        }
    }

    private async Task BrowseSalesAsync(CancellationToken cancellationToken)
    {
        var applyFilters = AnsiConsole.Confirm("Do you want to apply filters?", false);

        SaleQuery query;
        if (!applyFilters)
        {
            query = new SaleQuery();
        }
        else
        {
            var pageSize = PromptOptionalInt("Page size (default 10):") ?? 10;
            var startDate = PromptOptionalDate("Start date (yyyy-MM-dd):");
            var endDate = PromptOptionalDate("End date (yyyy-MM-dd):");
            var customer = PromptOptionalString("Customer name contains:");
            var sort = AnsiConsole.Prompt(new SelectionPrompt<string>()
                .Title("Sort by")
                .AddChoices("SaleDate", "TotalAmount", "CustomerName"));
            var direction = AnsiConsole.Prompt(new SelectionPrompt<string>()
                .Title("Sort direction")
                .AddChoices(Menu.Ascending, Menu.Descending));

            query = new SaleQuery(
                Page: 1,
                PageSize: pageSize,
                StartDate: startDate,
                EndDate: endDate,
                CustomerName: customer,
                CustomerEmail: null,
                SortBy: sort.ToLowerInvariant(),
                SortDirection: direction == Menu.Ascending ? "asc" : "desc");
        }

        while (true)
        {
            var result = await _apiClient.GetSalesAsync(query, cancellationToken);
            if (!result.Success || result.Data?.Data is null || result.Data.Data.Count == 0)
            {
                RenderWarning(result.Message.Length == 0 ? "No sales found." : result.Message);
                return;
            }

            var table = new Table().Border(TableBorder.DoubleEdge).Title("[aqua]Sales[/]");
            table.AddColumn("Sale ID");
            table.AddColumn("Date");
            table.AddColumn("Customer");
            table.AddColumn("Total");
            table.AddColumn(Menu.Items);

            foreach (var sale in result.Data.Data)
            {
                table.AddRow(
                    sale.SaleId.ToString(),
                    sale.SaleDate.ToString("yyyy-MM-dd"),
                    Markup.Escape(sale.CustomerName),
                    sale.TotalAmount.ToString("C"),
                    sale.SaleItems.Count.ToString());
            }

            table.Caption($"Page {result.Data.CurrentPage} of {result.Data.TotalPages} (Total {result.Data.TotalCount})");
            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();

            // Show pagination menu
            var newQuery = await ShowSalePaginationMenuAsync(result.Data, query, (q) => Task.CompletedTask);
            if (newQuery == null)
            {
                // User chose to go back
                return;
            }

            query = newQuery;
        }
    }

    private async Task ViewSaleDetailsAsync(CancellationToken cancellationToken)
    {
        var id = AnsiConsole.Ask<int>("Enter the [green]sale ID[/]:");
        var result = await _apiClient.GetSaleByIdAsync(id, cancellationToken);
        if (!result.Success || result.Data is null)
        {
            RenderError(result.Message.Length == 0 ? "Sale not found." : result.Message);
            return;
        }

        RenderSaleDetails(result.Data);
    }

    private static void RenderSaleDetails(Sale sale)
    {
        var summary = new Grid();
        summary.AddColumn(new GridColumn().NoWrap());
        summary.AddColumn(new GridColumn());
        summary.AddRow("ID", sale.SaleId.ToString());
        summary.AddRow("Date", sale.SaleDate.ToString("yyyy-MM-dd HH:mm"));
        summary.AddRow("Customer", Markup.Escape(sale.CustomerName));
        summary.AddRow("Email", Markup.Escape(sale.CustomerEmail));
        summary.AddRow("Address", Markup.Escape(sale.CustomerAddress));
        summary.AddRow("Total", sale.TotalAmount.ToString("C"));

        var items = new Table().Border(TableBorder.HeavyHead).Title("[yellow]Items[/]");
        items.AddColumn("Product");
        items.AddColumn("Qty");
        items.AddColumn("Unit Price");
        items.AddColumn("Line Total");

        foreach (var item in sale.SaleItems)
        {
            var lineTotal = item.LineTotal == 0 ? item.UnitPrice * item.Quantity : item.LineTotal;
            items.AddRow(
                Markup.Escape(item.Product?.Name ?? item.ProductName),
                item.Quantity.ToString(),
                item.UnitPrice.ToString("C"),
                lineTotal.ToString("C"));
        }

        var layout = new Layout("root")
            .SplitRows(
                new Layout("summary") { Size = 6 },
                new Layout("items"));

        layout["summary"].Update(new Panel(summary)
        {
            Header = new PanelHeader($"Sale {sale.SaleId}", Justify.Center),
            Border = BoxBorder.Rounded
        });

        layout["items"].Update(new Panel(items)
        {
            Border = BoxBorder.Square,
            Padding = new Padding(1)
        });

        AnsiConsole.Write(layout);
        AnsiConsole.WriteLine();
    }

    #endregion

    #region Helpers

    private static string? PromptOptionalString(string message)
    {
        var value = AnsiConsole.Prompt(new TextPrompt<string>(message).AllowEmpty());
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static int? PromptOptionalInt(string message)
    {
        var value = AnsiConsole.Prompt(new TextPrompt<string>(message).AllowEmpty());
        return int.TryParse(value, out var parsed) ? parsed : null;
    }

    private static decimal? PromptOptionalDecimal(string message)
    {
        var value = AnsiConsole.Prompt(new TextPrompt<string>(message).AllowEmpty());
        return decimal.TryParse(value, out var parsed) ? parsed : null;
    }

    private static DateTime? PromptOptionalDate(string message)
    {
        var value = AnsiConsole.Prompt(new TextPrompt<string>(message).AllowEmpty());
        return DateTime.TryParse(value, out var parsed) ? parsed : null;
    }

    private static void RenderSuccess(string message)
    {
        AnsiConsole.MarkupLine($"[green]{Markup.Escape(message)}[/]");
        AnsiConsole.WriteLine();
    }

    private static void RenderError(string message)
    {
        var output = string.IsNullOrWhiteSpace(message) ? "An unexpected error occurred." : message;
        AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(output)}");
        AnsiConsole.WriteLine();
    }

    private static void RenderWarning(string message)
    {
        var output = string.IsNullOrWhiteSpace(message) ? "Nothing to show." : message;
        AnsiConsole.MarkupLine($"[yellow]{Markup.Escape(output)}[/]");
        AnsiConsole.WriteLine();
    }

    private static async Task<ProductQuery?> ShowProductPaginationMenuAsync(
        PaginatedResponse<List<Product>> metadata,
        ProductQuery currentQuery,
        Func<ProductQuery, Task> onPageChange)
    {
        while (true)
        {
            AnsiConsole.WriteLine();
            var choices = new List<string>();

            if (metadata.HasPreviousPage)
            {
                choices.Add("Previous Page");
            }

            if (metadata.HasNextPage)
            {
                choices.Add("Next Page");
            }

            choices.Add("Go to Page");
            choices.Add("Change Page Size");
            choices.Add("Back to Menu");

            var choice = AnsiConsole.Prompt(new SelectionPrompt<string>()
                .Title($"[green]Page {metadata.CurrentPage} of {metadata.TotalPages} (Total {metadata.TotalCount})[/]")
                .AddChoices(choices));

            switch (choice)
            {
                case "Previous Page":
                    var prevQuery = currentQuery with { Page = metadata.CurrentPage - 1 };
                    await onPageChange(prevQuery);
                    return prevQuery;

                case "Next Page":
                    var nextQuery = currentQuery with { Page = metadata.CurrentPage + 1 };
                    await onPageChange(nextQuery);
                    return nextQuery;

                case "Go to Page":
                    var page = AnsiConsole.Ask<int>($"Enter page number (1-{metadata.TotalPages}):");
                    if (page < 1 || page > metadata.TotalPages)
                    {
                        RenderWarning($"Invalid page number. Please enter a number between 1 and {metadata.TotalPages}.");
                        continue;
                    }
                    var pageQuery = currentQuery with { Page = page };
                    await onPageChange(pageQuery);
                    return pageQuery;

                case "Change Page Size":
                    var size = AnsiConsole.Ask<int>("Enter page size (1-32):");
                    if (size < 1 || size > 32)
                    {
                        RenderWarning("Invalid page size. Please enter a number between 1 and 32.");
                        continue;
                    }
                    var sizeQuery = currentQuery with { PageSize = size, Page = 1 };
                    await onPageChange(sizeQuery);
                    return sizeQuery;

                case "Back to Menu":
                    return null;
            }
        }
    }

    private static async Task<SaleQuery?> ShowSalePaginationMenuAsync(
        PaginatedResponse<List<Sale>> metadata,
        SaleQuery currentQuery,
        Func<SaleQuery, Task> onPageChange)
    {
        while (true)
        {
            AnsiConsole.WriteLine();
            var choices = new List<string>();

            if (metadata.HasPreviousPage)
            {
                choices.Add("Previous Page");
            }

            if (metadata.HasNextPage)
            {
                choices.Add("Next Page");
            }

            choices.Add("Go to Page");
            choices.Add("Change Page Size");
            choices.Add("Back to Menu");

            var choice = AnsiConsole.Prompt(new SelectionPrompt<string>()
                .Title($"[green]Page {metadata.CurrentPage} of {metadata.TotalPages} (Total {metadata.TotalCount})[/]")
                .AddChoices(choices));

            switch (choice)
            {
                case "Previous Page":
                    var prevQuery = currentQuery with { Page = metadata.CurrentPage - 1 };
                    await onPageChange(prevQuery);
                    return prevQuery;

                case "Next Page":
                    var nextQuery = currentQuery with { Page = metadata.CurrentPage + 1 };
                    await onPageChange(nextQuery);
                    return nextQuery;

                case "Go to Page":
                    var page = AnsiConsole.Ask<int>($"Enter page number (1-{metadata.TotalPages}):");
                    if (page < 1 || page > metadata.TotalPages)
                    {
                        RenderWarning($"Invalid page number. Please enter a number between 1 and {metadata.TotalPages}.");
                        continue;
                    }
                    var pageQuery = currentQuery with { Page = page };
                    await onPageChange(pageQuery);
                    return pageQuery;

                case "Change Page Size":
                    var size = AnsiConsole.Ask<int>("Enter page size (1-32):");
                    if (size < 1 || size > 32)
                    {
                        RenderWarning("Invalid page size. Please enter a number between 1 and 32.");
                        continue;
                    }
                    var sizeQuery = currentQuery with { PageSize = size, Page = 1 };
                    await onPageChange(sizeQuery);
                    return sizeQuery;

                case "Back to Menu":
                    return null;
            }
        }
    }

    private static async Task<CategoryQuery?> ShowCategoryPaginationMenuAsync(
        int currentPage,
        int totalPages,
        int totalCount,
        CategoryQuery currentQuery,
        Func<CategoryQuery, Task> onPageChange)
    {
        while (true)
        {
            AnsiConsole.WriteLine();
            var choices = new List<string>();

            if (currentPage > 1)
            {
                choices.Add("Previous Page");
            }

            if (currentPage < totalPages)
            {
                choices.Add("Next Page");
            }

            choices.Add("Go to Page");
            choices.Add("Back to Menu");

            var choice = AnsiConsole.Prompt(new SelectionPrompt<string>()
                .Title($"[green]Page {currentPage} of {totalPages} (Total {totalCount})[/]")
                .AddChoices(choices));

            switch (choice)
            {
                case "Previous Page":
                    var prevQuery = currentQuery with { Page = currentPage - 1 };
                    await onPageChange(prevQuery);
                    return prevQuery;

                case "Next Page":
                    var nextQuery = currentQuery with { Page = currentPage + 1 };
                    await onPageChange(nextQuery);
                    return nextQuery;

                case "Go to Page":
                    var page = AnsiConsole.Ask<int>($"Enter page number (1-{totalPages}):");
                    if (page < 1 || page > totalPages)
                    {
                        RenderWarning($"Invalid page number. Please enter a number between 1 and {totalPages}.");
                        continue;
                    }
                    var pageQuery = currentQuery with { Page = page };
                    await onPageChange(pageQuery);
                    return pageQuery;

                case "Back to Menu":
                    return null;
            }
        }
    }

    #endregion
}
