using FerreteriaInventario.Api.DTOs;

namespace FerreteriaInventario.Api.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    Task<UsuarioResponseDto> RegisterAsync(UsuarioCreateDto request);
    Task<UsuarioResponseDto> GetMeAsync(int usuarioId);
}
