using FerreteriaInventario.Api.Data;
using FerreteriaInventario.Api.DTOs;
using FerreteriaInventario.Api.Helpers;
using FerreteriaInventario.Api.Interfaces;
using FerreteriaInventario.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FerreteriaInventario.Api.Services;

public class UsuarioService : IUsuarioService
{
    private readonly AppDbContext _context;
    private readonly PasswordHasher<Usuario> _passwordHasher = new();

    public UsuarioService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<UsuarioResponseDto>> GetAllAsync()
    {
        var usuarios = await _context.Usuarios
            .Include(x => x.Rol)
            .OrderBy(x => x.Nombre)
            .ToListAsync();

        return usuarios.Select(x => x.ToDto()).ToList();
    }

    public async Task<UsuarioResponseDto> GetByIdAsync(int id)
    {
        var usuario = await GetUsuarioAsync(id);
        return usuario.ToDto();
    }

    public async Task<UsuarioResponseDto> CreateAsync(UsuarioCreateDto request)
    {
        await ValidateUsuarioAsync(request.NombreUsuario, request.Email, request.RolId);

        var usuario = new Usuario
        {
            Nombre = request.Nombre.Trim(),
            NombreUsuario = request.NombreUsuario.Trim(),
            Email = request.Email.Trim(),
            RolId = request.RolId,
            Activo = request.Activo
        };

        usuario.PasswordHash = _passwordHasher.HashPassword(usuario, request.Password);

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        usuario = await GetUsuarioAsync(usuario.Id);
        return usuario.ToDto();
    }

    public async Task<UsuarioResponseDto> UpdateAsync(int id, UsuarioUpdateDto request)
    {
        var usuario = await GetUsuarioAsync(id);

        await ValidateUsuarioAsync(request.NombreUsuario, request.Email, request.RolId, id);

        usuario.Nombre = request.Nombre.Trim();
        usuario.NombreUsuario = request.NombreUsuario.Trim();
        usuario.Email = request.Email.Trim();
        usuario.RolId = request.RolId;
        usuario.Activo = request.Activo;

        await _context.SaveChangesAsync();

        usuario = await GetUsuarioAsync(id);
        return usuario.ToDto();
    }

    public async Task<UsuarioResponseDto> SetActiveAsync(int id, bool activo)
    {
        var usuario = await GetUsuarioAsync(id);
        usuario.Activo = activo;
        await _context.SaveChangesAsync();
        return usuario.ToDto();
    }

    private async Task ValidateUsuarioAsync(string nombreUsuario, string email, int rolId, int? id = null)
    {
        if (!await _context.Roles.AnyAsync(x => x.Id == rolId))
        {
            throw new ApiException("El rol especificado no existe.", StatusCodes.Status404NotFound);
        }

        if (await _context.Usuarios.AnyAsync(x =>
                x.Id != id &&
                x.NombreUsuario.ToLower() == nombreUsuario.ToLower()))
        {
            throw new ApiException("El nombre de usuario ya existe.");
        }

        if (await _context.Usuarios.AnyAsync(x =>
                x.Id != id &&
                x.Email.ToLower() == email.ToLower()))
        {
            throw new ApiException("El correo electronico ya existe.");
        }
    }

    private async Task<Usuario> GetUsuarioAsync(int id)
    {
        var usuario = await _context.Usuarios
            .Include(x => x.Rol)
            .FirstOrDefaultAsync(x => x.Id == id);

        return usuario ?? throw new ApiException("Usuario no encontrado.", StatusCodes.Status404NotFound);
    }
}
