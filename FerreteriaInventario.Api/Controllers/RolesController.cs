using FerreteriaInventario.Api.DTOs;
using FerreteriaInventario.Api.Interfaces;
using FerreteriaInventario.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FerreteriaInventario.Api.Data;

namespace FerreteriaInventario.Api.Controllers;

[ApiController]
[Route("api/roles")]
[Authorize(Roles = "Admin")]
public class RolesController : ControllerBase
{
    private readonly AppDbContext _context;

    public RolesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<RolResponseDto>>> GetAll()
    {
        var roles = await _context.Roles
            .OrderBy(x => x.Nombre)
            .ToListAsync();

        return Ok(roles.Select(x => x.ToDto()).ToList());
    }
}
