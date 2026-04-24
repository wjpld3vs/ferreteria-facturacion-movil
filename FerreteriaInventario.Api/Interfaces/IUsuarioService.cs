using FerreteriaInventario.Api.DTOs;

namespace FerreteriaInventario.Api.Interfaces;

public interface IUsuarioService
{
    Task<List<UsuarioResponseDto>> GetAllAsync();
    Task<UsuarioResponseDto> GetByIdAsync(int id);
    Task<UsuarioResponseDto> CreateAsync(UsuarioCreateDto request);
    Task<UsuarioResponseDto> UpdateAsync(int id, UsuarioUpdateDto request);
    Task<UsuarioResponseDto> SetActiveAsync(int id, bool activo);
}
