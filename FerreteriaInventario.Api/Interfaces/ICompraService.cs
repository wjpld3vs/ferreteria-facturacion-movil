using FerreteriaInventario.Api.DTOs;

namespace FerreteriaInventario.Api.Interfaces;

public interface ICompraService
{
    Task<List<CompraResponseDto>> GetAllAsync();
    Task<CompraResponseDto> GetByIdAsync(int id);
    Task<CompraResponseDto> CreateAsync(CompraCreateDto request);
}
