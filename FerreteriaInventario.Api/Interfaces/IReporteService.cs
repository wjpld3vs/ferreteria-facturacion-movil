using FerreteriaInventario.Api.DTOs;

namespace FerreteriaInventario.Api.Interfaces;

public interface IReporteService
{
    Task<List<ReporteInventarioDto>> GetInventarioAsync();
    Task<List<StockBajoDto>> GetStockBajoAsync();
    Task<ReporteVentasPorFechaDto> GetVentasPorFechaAsync(DateTime fechaInicio, DateTime fechaFin);
    Task<ReporteComprasPorFechaDto> GetComprasPorFechaAsync(DateTime fechaInicio, DateTime fechaFin);
}
