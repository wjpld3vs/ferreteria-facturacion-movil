using FerreteriaInventario.Api.DTOs;
using FerreteriaInventario.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FerreteriaInventario.Api.Controllers;

[ApiController]
[Route("api/ventas")]
[Authorize]
public class VentasController : ControllerBase
{
    private readonly IVentaService _service;

    public VentasController(IVentaService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Operario")]
    public async Task<ActionResult<List<VentaResponseDto>>> GetAll() => Ok(await _service.GetAllAsync());

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Operario")]
    public async Task<ActionResult<VentaResponseDto>> GetById(int id) => Ok(await _service.GetByIdAsync(id));

    [HttpPost]
    [Authorize(Roles = "Admin,Operario")]
    public async Task<ActionResult<VentaResponseDto>> Create([FromBody] VentaCreateDto request)
    {
        var venta = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = venta.Id }, venta);
    }
}
