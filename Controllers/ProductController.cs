using Microsoft.AspNetCore.Mvc;
using ECommerceApp.RyanW84.Interfaces;
using ECommerceApp.RyanW84.Services;

namespace ECommerceApp.RyanW84.Controllers;
[ApiController]
[Route("api/[controller]")]
public class ProductController(IProductService productService): ControllerBase
    {
    private readonly IProductService _productService = productService;

    [HttpGet]
    public async Task<IActionResult> GetProductsAsync()
        {
        var products = await _productService.GetProductsAsync();
        return Ok(products);
        }
    }
