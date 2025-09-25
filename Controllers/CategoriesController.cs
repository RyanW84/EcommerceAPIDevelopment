using System.Threading;
using System.Threading.Tasks;
using ECommerceApp.RyanW84.Data.DTO;
using ECommerceApp.RyanW84.Data.Models;
using ECommerceApp.RyanW84.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApp.RyanW84.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService) => _categoryService = categoryService;

    // POST /api/categories
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ApiRequestDto<Category> request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _categoryService.CreateCategoryAsync(request, cancellationToken);
        if (result.RequestFailed) return this.FromFailure(result.ResponseCode, result.ErrorMessage);

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.CategoryId }, result.Data);
    }

    // GET /api/categories
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] CategoryQueryParameters queryParameters, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _categoryService.GetAllCategoriesAsync(queryParameters, cancellationToken);
        if (result.RequestFailed) return this.FromFailure(result.ResponseCode, result.ErrorMessage);
        return Ok(result);
    }

    // GET /api/categories/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _categoryService.GetCategoryAsync(id, cancellationToken);
        if (result.RequestFailed) return this.FromFailure(result.ResponseCode, result.ErrorMessage);
        return Ok(result.Data);
    }

    // GET /api/categories/name/{name}
    [HttpGet("name/{name}")]
    public async Task<IActionResult> GetByName(string name, CancellationToken cancellationToken)
    {
        var result = await _categoryService.GetCategoryByNameAsync(name, cancellationToken);
        if (result.RequestFailed) return this.FromFailure(result.ResponseCode, result.ErrorMessage);
        return Ok(result.Data);
    }

    // PUT /api/categories/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ApiRequestDto<Category> request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _categoryService.UpdateCategoryAsync(id, request, cancellationToken);
        if (result.RequestFailed) return this.FromFailure(result.ResponseCode, result.ErrorMessage);
        return Ok(result.Data);
    }

    // DELETE /api/categories/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _categoryService.DeleteCategoryAsync(id, cancellationToken);
        if (result.RequestFailed) return this.FromFailure(result.ResponseCode, result.ErrorMessage);
        return NoContent();
    }

    // GET /api/categories/deleted
    [HttpGet("deleted")]
    public async Task<IActionResult> GetDeletedCategories(CancellationToken cancellationToken)
    {
        var result = await _categoryService.GetDeletedCategoriesAsync(cancellationToken);
        if (result.RequestFailed) return this.FromFailure(result.ResponseCode, result.ErrorMessage);
        return Ok(result.Data);
    }

    // POST /api/categories/{id}/restore
    [HttpPost("{id:int}/restore")]
    public async Task<IActionResult> Restore(int id, CancellationToken cancellationToken)
    {
        var result = await _categoryService.RestoreCategoryAsync(id, cancellationToken);
        if (result.RequestFailed) return this.FromFailure(result.ResponseCode, result.ErrorMessage);
        return Ok(new { message = "Category restored successfully" });
    }
}
