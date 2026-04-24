using FerreteriaInventario.Api.DTOs;
using FerreteriaInventario.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FerreteriaInventario.Api.Controllers;

[ApiController]
[Route("api/usuarios")]
[Authorize(Roles = "Admin")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _service;

    public UsuariosController(IUsuarioService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<UsuarioResponseDto>>> GetAll() => Ok(await _service.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UsuarioResponseDto>> GetById(int id) => Ok(await _service.GetByIdAsync(id));

    [HttpPost]
    public async Task<ActionResult<UsuarioResponseDto>> Create([FromBody] UsuarioCreateDto request)
    {
        var usuario = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = usuario.Id }, usuario);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<UsuarioResponseDto>> Update(int id, [FromBody] UsuarioUpdateDto request)
        => Ok(await _service.UpdateAsync(id, request));

    [HttpPatch("{id:int}/activar")]
    public async Task<ActionResult<UsuarioResponseDto>> Activar(int id) => Ok(await _service.SetActiveAsync(id, true));

    [HttpPatch("{id:int}/desactivar")]
    public async Task<ActionResult<UsuarioResponseDto>> Desactivar(int id) => Ok(await _service.SetActiveAsync(id, false));
}
