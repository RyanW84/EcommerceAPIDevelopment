using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

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
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
        // Register application services
        builder.Services.AddScoped<Services.IProductService , Services.ProductService>();
        builder.Services.AddScoped<Services.ICategoryService , Services.CategoryService>();
        builder.Services.AddScoped<Services.ISaleService , Services.SaleService>();
        builder.Services.AddScoped<Services.ISalesSummaryService , Services.SalesSummaryService>();
        var app = builder.Build();
        // Configure the HTTP request pipeline.
        if(!app.Environment.IsDevelopment())
            {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
            }
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.MapControllers();
        app.Run();
        }
    }