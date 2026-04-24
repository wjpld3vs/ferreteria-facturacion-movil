using FerreteriaInventario.Api.Data;
using FerreteriaInventario.Api.DTOs;
using FerreteriaInventario.Api.Helpers;
using FerreteriaInventario.Api.Interfaces;
using FerreteriaInventario.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FerreteriaInventario.Api.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly PasswordHasher<Usuario> _passwordHasher = new();

    public AuthService(AppDbContext context, ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        var normalized = request.UsuarioOEmail.Trim().ToLowerInvariant();
        var usuario = await _context.Usuarios
            .Include(x => x.Rol)
            .FirstOrDefaultAsync(x =>
                x.NombreUsuario.ToLower() == normalized ||
                x.Email.ToLower() == normalized);

        if (usuario is null)
        {
            throw new ApiException("Usuario o contrasena invalidos.", StatusCodes.Status401Unauthorized);
        }

        if (!usuario.Activo)
        {
            throw new ApiException("El usuario esta inactivo y no puede iniciar sesion.", StatusCodes.Status403Forbidden);
        }

        var verification = _passwordHasher.VerifyHashedPassword(usuario, usuario.PasswordHash, request.Password);
        if (verification == PasswordVerificationResult.Failed)
        {
            throw new ApiException("Usuario o contrasena invalidos.", StatusCodes.Status401Unauthorized);
        }

        var (token, expiration) = _tokenService.GenerateToken(usuario);

        return new LoginResponseDto
        {
            Token = token,
            Expiration = expiration,
            Usuario = usuario.ToDto()
        };
    }

    public async Task<UsuarioResponseDto> RegisterAsync(UsuarioCreateDto request)
    {
        var roleExists = await _context.Roles.AnyAsync(x => x.Id == request.RolId);
        if (!roleExists)
        {
            throw new ApiException("El rol especificado no existe.", StatusCodes.Status404NotFound);
        }

        if (await _context.Usuarios.AnyAsync(x => x.NombreUsuario.ToLower() == request.NombreUsuario.ToLower()))
        {
            throw new ApiException("El nombre de usuario ya esta registrado.");
        }

        if (await _context.Usuarios.AnyAsync(x => x.Email.ToLower() == request.Email.ToLower()))
        {
            throw new ApiException("El correo electronico ya esta registrado.");
        }

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

        usuario = await _context.Usuarios.Include(x => x.Rol).FirstAsync(x => x.Id == usuario.Id);
        return usuario.ToDto();
    }

    public async Task<UsuarioResponseDto> GetMeAsync(int usuarioId)
    {
        var usuario = await _context.Usuarios
            .Include(x => x.Rol)
            .FirstOrDefaultAsync(x => x.Id == usuarioId);

        if (usuario is null)
        {
            throw new ApiException("Usuario no encontrado.", StatusCodes.Status404NotFound);
        }

        return usuario.ToDto();
    }
}
