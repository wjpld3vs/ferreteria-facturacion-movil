using FerreteriaInventario.Api.DTOs;

namespace FerreteriaInventario.Api.Interfaces;

public interface IClienteService
{
    Task<List<ClienteResponseDto>> GetAllAsync();
    Task<ClienteResponseDto> GetByIdAsync(int id);
    Task<ClienteResponseDto> CreateAsync(ClienteCreateDto request);
    Task<ClienteResponseDto> UpdateAsync(int id, ClienteUpdateDto request);
    Task<ClienteResponseDto> SetActiveAsync(int id, bool activo);
}
