using ECommerceApp.RyanW84.Data;
using ECommerceApp.RyanW84.Interfaces;
using ECommerceApp.RyanW84.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

namespace ECommerceApp.RyanW84;

public class Program
{
    // Entry point of the application
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();

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

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();

            app.MapOpenApi();
            app.MapScalarApiReference(options =>
            {
                options.OpenApiRoutePattern = "api/document.json";
                options.Title = "ECommerceApp.RyanW84 API";
                options.Theme = ScalarTheme.BluePlanet;
                options.Layout = ScalarLayout.Modern;
                options.DarkMode = true;
            });

            var context = app
                .Services.CreateScope()
                .ServiceProvider.GetRequiredService<ECommerceDbContext>();

            context.SeedData();
            context.SaveChanges();
        }
        else
        {
            // In development expose the OpenAPI document and Scalar UI too
            app.MapOpenApi(pattern: "api/document.json");
            app.MapScalarApiReference(options =>
            {
                options.OpenApiRoutePattern = "api/document.json";
                options.Title = "ECommerceApp.RyanW84 API (Dev)";
                options.Theme = ScalarTheme.BluePlanet;
                options.Layout = ScalarLayout.Modern;
                options.DarkMode = true;
            });
        }

        app.UseHttpsRedirection();

        // Required to match endpoints to controller routes
        app.UseRouting();

        // If you use authorization/Authentication, keep this. Safe to call even if you don't currently use them.
        app.UseAuthorization();

        // Map attribute routed controllers (needed so controller actions are reachable)
        app.MapControllers();

        // Optional: simple root endpoint - helps avoid 404 on GET /
        app.MapGet("/", () => Results.Redirect("/api/document.json"));

        app.Run();
    }
}
