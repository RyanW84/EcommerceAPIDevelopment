using ECommerceApp.RyanW84.Data;
using ECommerceApp.RyanW84.Interfaces;
using ECommerceApp.RyanW84.Middleware;
using ECommerceApp.RyanW84.Services;
using ECommerceApp.RyanW84.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using Scalar.AspNetCore;
using System.Runtime.InteropServices;

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

        builder.Services
            .AddFluentValidationAutoValidation()
            .AddFluentValidationClientsideAdapters();

        builder.Services.AddValidatorsFromAssemblyContaining<ProductValidator>();

        builder.Services.AddResponseCaching();

        // Add Scalar for API documentation
        builder.Services.AddOpenApi();

        // Configure database context
        string connectionString;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            connectionString = "Server=(localdb)\\mssqllocaldb;Database=ECommerceDb;Trusted_Connection=True;";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }
        else
        {
            throw new PlatformNotSupportedException("Unsupported OS platform.");
        }

        builder.Services.AddDbContext<Data.ECommerceDbContext>(options =>
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                options.UseSqlServer(connectionString);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                options.UseSqlite(connectionString);
            }
            else
            {
                throw new PlatformNotSupportedException("Unsupported OS platform.");
            }
        });
        // Register application services
        builder.Services.AddScoped<IProductService, ProductService>();
        builder.Services.AddScoped<ICategoryService, CategoryService>();
        builder.Services.AddScoped<ISaleService, SaleService>();
        builder.Services.AddScoped<ISalesSummaryService, SalesSummaryService>();
        builder.Services.AddScoped<IProductRepository, Repositories.ProductRepository>();
        builder.Services.AddScoped<ICategoryRepository, Repositories.CategoryRepository>();
        builder.Services.AddScoped<ISaleRepository, Repositories.SaleRepository>();

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
                    // Force reseeding for testing
                    db.SeedData();
                    db.SaveChanges();
                    app.Logger.LogInformation("Database seeded with initial data.");
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

        // Add global exception handling
        app.UseGlobalExceptionHandler();

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

    app.UseResponseCaching();

        // If you use authorization/Authentication, keep this. Safe to call even if you don't currently use them.
        app.UseAuthorization();

        // Map attribute routed controllers (needed so controller actions are reachable)
        app.MapControllers();

        // Optional: simple root endpoint - redirect to Scalar API documentation
        app.MapGet("/", () => Results.Redirect("/scalar/v1"));

        app.Run();
    }
}
