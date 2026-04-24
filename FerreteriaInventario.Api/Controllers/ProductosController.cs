using FerreteriaInventario.Api.DTOs;
using FerreteriaInventario.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FerreteriaInventario.Api.Controllers;

[ApiController]
[Route("api/productos")]
[Authorize]
public class ProductosController : ControllerBase
{
    private readonly IProductoService _service;

    public ProductosController(IProductoService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Operario")]
    public async Task<ActionResult<List<ProductoResponseDto>>> GetAll() => Ok(await _service.GetAllAsync());

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Operario")]
    public async Task<ActionResult<ProductoResponseDto>> GetById(int id) => Ok(await _service.GetByIdAsync(id));

    [HttpGet("buscar")]
    [Authorize(Roles = "Admin,Operario")]
    public async Task<ActionResult<List<ProductoResponseDto>>> Buscar([FromQuery] string? texto)
        => Ok(await _service.SearchAsync(texto));

    [HttpGet("stock-bajo")]
    [Authorize(Roles = "Admin,Operario")]
    public async Task<ActionResult<List<StockBajoDto>>> StockBajo() => Ok(await _service.GetLowStockAsync());

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductoResponseDto>> Create([FromBody] ProductoCreateDto request)
    {
        var producto = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = producto.Id }, producto);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductoResponseDto>> Update(int id, [FromBody] ProductoUpdateDto request)
        => Ok(await _service.UpdateAsync(id, request));

    [HttpPatch("{id:int}/activar")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductoResponseDto>> Activar(int id) => Ok(await _service.SetActiveAsync(id, true));

    [HttpPatch("{id:int}/desactivar")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductoResponseDto>> Desactivar(int id) => Ok(await _service.SetActiveAsync(id, false));
}
