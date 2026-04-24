using FerreteriaInventario.Api.DTOs;
using FerreteriaInventario.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FerreteriaInventario.Api.Controllers;

[ApiController]
[Route("api/compras")]
[Authorize(Roles = "Admin")]
public class ComprasController : ControllerBase
{
    private readonly ICompraService _service;

    public ComprasController(ICompraService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<CompraResponseDto>>> GetAll() => Ok(await _service.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CompraResponseDto>> GetById(int id) => Ok(await _service.GetByIdAsync(id));

    [HttpPost]
    public async Task<ActionResult<CompraResponseDto>> Create([FromBody] CompraCreateDto request)
    {
        var compra = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = compra.Id }, compra);
    }
}
