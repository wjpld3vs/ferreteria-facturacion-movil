using FerreteriaInventario.Api.DTOs;

namespace FerreteriaInventario.Api.Interfaces;

public interface IProveedorService
{
    Task<List<ProveedorResponseDto>> GetAllAsync();
    Task<ProveedorResponseDto> GetByIdAsync(int id);
    Task<ProveedorResponseDto> CreateAsync(ProveedorCreateDto request);
    Task<ProveedorResponseDto> UpdateAsync(int id, ProveedorUpdateDto request);
    Task<ProveedorResponseDto> SetActiveAsync(int id, bool activo);
}
