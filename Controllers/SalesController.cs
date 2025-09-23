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
        if (result.RequestFailed) return Problem(detail: result.ErrorMessage, statusCode: (int)result.ResponseCode);

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.SaleId }, result.Data);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _saleService.GetSaleByIdAsync(id, cancellationToken);
        if (result.RequestFailed) return Problem(detail: result.ErrorMessage, statusCode: (int)result.ResponseCode);
        return Ok(result.Data);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _saleService.GetSalesAsync(cancellationToken);
        if (result.RequestFailed) return Problem(detail: result.ErrorMessage, statusCode: (int)result.ResponseCode);
        return Ok(result.Data);
    }
}
