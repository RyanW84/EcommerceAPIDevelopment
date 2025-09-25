using System.Threading;
using System.Threading.Tasks;
using ECommerceApp.RyanW84.Data.DTO;
using ECommerceApp.RyanW84.Data.Models;
using ECommerceApp.RyanW84.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApp.RyanW84.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SalesController : ControllerBase
{
    private readonly ISaleService _saleService;

    public SalesController(ISaleService saleService) => _saleService = saleService;

    // POST /api/sales
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ApiRequestDto<Sale> request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _saleService.CreateSaleAsync(request, cancellationToken);
        if (result.RequestFailed) return this.FromFailure(result.ResponseCode, result.ErrorMessage);

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.SaleId }, result.Data);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _saleService.GetSaleByIdAsync(id, cancellationToken);
        if (result.RequestFailed) return this.FromFailure(result.ResponseCode, result.ErrorMessage);
        return Ok(result.Data);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] SaleQueryParameters queryParameters, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _saleService.GetSalesAsync(queryParameters, cancellationToken);
        if (result.RequestFailed) return this.FromFailure(result.ResponseCode, result.ErrorMessage);
        return Ok(result);
    }

    [HttpGet("with-deleted-products")]
    public async Task<IActionResult> GetAllWithDeletedProducts(CancellationToken cancellationToken)
    {
        var result = await _saleService.GetHistoricalSalesAsync(cancellationToken);
        if (result.RequestFailed) return this.FromFailure(result.ResponseCode, result.ErrorMessage);
        return Ok(result.Data);
    }

    [HttpGet("{id:int}/with-deleted-products")]
    public async Task<IActionResult> GetByIdWithDeletedProducts(int id, CancellationToken cancellationToken)
    {
        var result = await _saleService.GetSaleByIdWithHistoricalProductsAsync(id, cancellationToken);
        if (result.RequestFailed) return this.FromFailure(result.ResponseCode, result.ErrorMessage);
        return Ok(result.Data);
    }
}
