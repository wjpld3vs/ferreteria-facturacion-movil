using FerreteriaInventario.Api.DTOs;
using FerreteriaInventario.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FerreteriaInventario.Api.Controllers;

[ApiController]
[Route("api/proveedores")]
[Authorize]
public class ProveedoresController : ControllerBase
{
    private readonly IProveedorService _service;

    public ProveedoresController(IProveedorService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<ProveedorResponseDto>>> GetAll() => Ok(await _service.GetAllAsync());

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProveedorResponseDto>> GetById(int id) => Ok(await _service.GetByIdAsync(id));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProveedorResponseDto>> Create([FromBody] ProveedorCreateDto request)
    {
        var proveedor = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = proveedor.Id }, proveedor);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProveedorResponseDto>> Update(int id, [FromBody] ProveedorUpdateDto request)
        => Ok(await _service.UpdateAsync(id, request));

    [HttpPatch("{id:int}/desactivar")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProveedorResponseDto>> Desactivar(int id) => Ok(await _service.SetActiveAsync(id, false));
}
