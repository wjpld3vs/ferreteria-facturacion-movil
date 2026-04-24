using FerreteriaInventario.Api.DTOs;
using FerreteriaInventario.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FerreteriaInventario.Api.Controllers;

[ApiController]
[Route("api/clientes")]
[Authorize]
public class ClientesController : ControllerBase
{
    private readonly IClienteService _service;

    public ClientesController(IClienteService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Operario")]
    public async Task<ActionResult<List<ClienteResponseDto>>> GetAll() => Ok(await _service.GetAllAsync());

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Operario")]
    public async Task<ActionResult<ClienteResponseDto>> GetById(int id) => Ok(await _service.GetByIdAsync(id));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ClienteResponseDto>> Create([FromBody] ClienteCreateDto request)
    {
        var cliente = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = cliente.Id }, cliente);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ClienteResponseDto>> Update(int id, [FromBody] ClienteUpdateDto request)
        => Ok(await _service.UpdateAsync(id, request));

    [HttpPatch("{id:int}/desactivar")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ClienteResponseDto>> Desactivar(int id) => Ok(await _service.SetActiveAsync(id, false));
}
