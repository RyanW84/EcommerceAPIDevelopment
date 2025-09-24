using System.Threading;
using System.Threading.Tasks;
using ECommerceApp.RyanW84.Data.DTO;
using ECommerceApp.RyanW84.Data.Models;
using ECommerceApp.RyanW84.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApp.RyanW84.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController(IProductService productService) : ControllerBase
{
    private readonly IProductService _productService = productService;

    // GET /api/products
    [HttpGet]
    public async Task<IActionResult> GetProductsAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        // For now, we'll return all products and let the client handle pagination
        // In a future enhancement, we could implement server-side pagination
        var result = await _productService.GetProductsAsync();
        if (result.RequestFailed) return Problem(detail: result.ErrorMessage, statusCode: (int)result.ResponseCode);
        return Ok(result.Data);
    }

    // GET /api/products/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetProductById(int id, CancellationToken cancellationToken = default)
    {
        var result = await _productService.GetProductByIdAsync(id, cancellationToken);
        if (result.RequestFailed) return Problem(detail: result.ErrorMessage, statusCode: (int)result.ResponseCode);
        return Ok(result.Data);
    }

    // GET /api/products/category/{categoryId}
    [HttpGet("category/{categoryId:int}")]
    public async Task<IActionResult> GetProductsByCategory(int categoryId, CancellationToken cancellationToken = default)
    {
        var result = await _productService.GetProductsByCategoryIdAsync(categoryId, cancellationToken);
        if (result.RequestFailed) return Problem(detail: result.ErrorMessage, statusCode: (int)result.ResponseCode);
        return Ok(result.Data);
    }

    // POST /api/products
    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] ApiRequestDto<Product> request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _productService.CreateProductAsync(request, cancellationToken);
        if (result.RequestFailed) return Problem(detail: result.ErrorMessage, statusCode: (int)result.ResponseCode);

        return CreatedAtAction(nameof(GetProductById), new { id = result.Data!.ProductId }, result.Data);
    }

    // PUT /api/products/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] ApiRequestDto<Product> request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _productService.UpdateProductAsync(id, request, cancellationToken);
        if (result.RequestFailed) return Problem(detail: result.ErrorMessage, statusCode: (int)result.ResponseCode);
        return Ok(result.Data);
    }

    // DELETE /api/products/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProduct(int id, CancellationToken cancellationToken = default)
    {
        var result = await _productService.DeleteProductAsync(id, cancellationToken);
        if (result.RequestFailed) return Problem(detail: result.ErrorMessage, statusCode: (int)result.ResponseCode);
        return NoContent();
    }

    // GET /api/products/deleted
    [HttpGet("deleted")]
    public async Task<IActionResult> GetDeletedProducts(CancellationToken cancellationToken = default)
    {
        var result = await _productService.GetDeletedProductsAsync(cancellationToken);
        if (result.RequestFailed) return Problem(detail: result.ErrorMessage, statusCode: (int)result.ResponseCode);
        return Ok(result.Data);
    }

    // POST /api/products/{id}/restore
    [HttpPost("{id:int}/restore")]
    public async Task<IActionResult> RestoreProduct(int id, CancellationToken cancellationToken = default)
    {
        var result = await _productService.RestoreProductAsync(id, cancellationToken);
        if (result.RequestFailed) return Problem(detail: result.ErrorMessage, statusCode: (int)result.ResponseCode);
        return Ok(result.Data);
    }
}
