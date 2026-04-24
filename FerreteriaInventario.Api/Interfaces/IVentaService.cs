using FerreteriaInventario.Api.DTOs;

namespace FerreteriaInventario.Api.Interfaces;

public interface IVentaService
{
    Task<List<VentaResponseDto>> GetAllAsync();
    Task<VentaResponseDto> GetByIdAsync(int id);
    Task<VentaResponseDto> CreateAsync(VentaCreateDto request);
}
