using FerreteriaInventario.Api.DTOs;

namespace FerreteriaInventario.Api.Interfaces;

public interface IProductoService
{
    Task<List<ProductoResponseDto>> GetAllAsync();
    Task<ProductoResponseDto> GetByIdAsync(int id);
    Task<List<ProductoResponseDto>> SearchAsync(string? texto);
    Task<List<StockBajoDto>> GetLowStockAsync();
    Task<ProductoResponseDto> CreateAsync(ProductoCreateDto request);
    Task<ProductoResponseDto> UpdateAsync(int id, ProductoUpdateDto request);
    Task<ProductoResponseDto> SetActiveAsync(int id, bool activo);
}
