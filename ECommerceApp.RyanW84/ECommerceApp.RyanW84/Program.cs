using ECommerceApp.RyanW84.Interfaces;
using ECommerceApp.RyanW84.Services;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.RyanW84;

public class Program
{
    // Entry point of the application
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();
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
        }
        app.UseRouting();
        app.MapControllers();
        app.Run();
    }
}
