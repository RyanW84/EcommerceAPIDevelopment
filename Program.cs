using ECommerceApp.RyanW84.Data;
using ECommerceApp.RyanW84.Interfaces;
using ECommerceApp.RyanW84.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

namespace ECommerceApp.RyanW84;

public class Program
{
    // Entry point of the application --test
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder
            .Services.AddControllers()
            .AddJsonOptions(options =>
            {
                // Handle circular references
                options.JsonSerializerOptions.ReferenceHandler = System
                    .Text
                    .Json
                    .Serialization
                    .ReferenceHandler
                    .Preserve;
                // Optional: Make JSON more readable
                options.JsonSerializerOptions.WriteIndented = true;
            });

        // Add Scalar for API documentation
        builder.Services.AddOpenApi();

        // Configure database context
        builder.Services.AddDbContext<Data.ECommerceDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
        );
        // Register application services
        builder.Services.AddScoped<IProductService, ProductService>();
        builder.Services.AddScoped<ICategoryService, CategoryService>();
        builder.Services.AddScoped<ISaleService, SaleService>();
        builder.Services.AddScoped<ISalesSummaryService, SalesSummaryService>();
        builder.Services.AddScoped<IProductRepository, Repositories.ProductRepository>();
        builder.Services.AddScoped<ICategoryRepository, Repositories.CategoryRepository>();

        var app = builder.Build();

        // Ensure database migrations are applied and optionally seed data
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ECommerceDbContext>();
            try
            {
                // Applies any pending migrations; creates database if it doesn't exist
                db.Database.Migrate();

                // Seed data in Development environment if database is empty
                if (app.Environment.IsDevelopment())
                {
                    // Check if data already exists to avoid re-seeding
                    if (!db.Categories.Any())
                    {
                        db.SeedData();
                        db.SaveChanges();
                        app.Logger.LogInformation("Database seeded with initial data.");
                    }
                    else
                    {
                        app.Logger.LogInformation(
                            "Database already contains data, skipping seeding."
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                // Log or rethrow as appropriate for your app; failing to migrate should be handled
                app.Logger.LogError(
                    ex,
                    "An error occurred while migrating or seeding the database."
                );
                throw;
            }
        }

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            // In development, expose the OpenAPI document and Scalar UI
            app.MapOpenApi();
            app.MapScalarApiReference(options =>
            {
                // Enhanced Configuration
                options.Title = "ECommerceApp.RyanW84 API Documentation";
                options.Theme = ScalarTheme.BluePlanet;
                options.Layout = ScalarLayout.Modern;
                options.DarkMode = true;

                // API Client Configuration
                options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);

                // UI Customization
                options.HideModels = false;
                options.ShowSidebar = true;
                options.DefaultOpenAllTags = true;

                // Search Configuration
                options.SearchHotKey = "k";

                // Custom Styling
                options.CustomCss = @"
                    .scalar-api-reference {
                        --scalar-color-primary: #2563eb;
                        --scalar-color-secondary: #64748b;
                        --scalar-color-accent: #06b6d4;
                    }
                    .dark-mode .scalar-api-reference {
                        --scalar-color-background: #0f172a;
                        --scalar-color-1: #4e668dff;
                        --scalar-color-2: #67778fff;
                        --scalar-color-3: #6d809bff;
                    }
                ";
            });
        }
        else
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        // Required to match endpoints to controller routes
        app.UseRouting();

        // If you use authorization/Authentication, keep this. Safe to call even if you don't currently use them.
        app.UseAuthorization();

        // Map attribute routed controllers (needed so controller actions are reachable)
        app.MapControllers();

        // Optional: simple root endpoint - redirect to Scalar API documentation
        app.MapGet("/", () => Results.Redirect("/scalar/v1"));

        app.Run();
    }
}
