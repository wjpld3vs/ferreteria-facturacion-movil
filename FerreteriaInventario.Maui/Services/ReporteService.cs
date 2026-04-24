using FerreteriaInventario.Maui.Models;

namespace FerreteriaInventario.Maui.Services;

public class ReporteService
{
    private readonly ApiService _apiService;

    public ReporteService(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<List<ReporteInventarioModel>> GetInventarioAsync() =>
        await _apiService.GetAsync<List<ReporteInventarioModel>>("api/reportes/inventario") ?? new List<ReporteInventarioModel>();

    public async Task<List<StockBajoModel>> GetStockBajoAsync() =>
        await _apiService.GetAsync<List<StockBajoModel>>("api/reportes/stock-bajo") ?? new List<StockBajoModel>();

    public async Task<ReporteVentasPorFechaModel> GetVentasPorFechaAsync(DateTime inicio, DateTime fin) =>
        await _apiService.GetAsync<ReporteVentasPorFechaModel>($"api/reportes/ventas-por-fecha?fechaInicio={inicio:yyyy-MM-dd}&fechaFin={fin:yyyy-MM-dd}")
        ?? new ReporteVentasPorFechaModel();

    public async Task<ReporteComprasPorFechaModel> GetComprasPorFechaAsync(DateTime inicio, DateTime fin) =>
        await _apiService.GetAsync<ReporteComprasPorFechaModel>($"api/reportes/compras-por-fecha?fechaInicio={inicio:yyyy-MM-dd}&fechaFin={fin:yyyy-MM-dd}")
        ?? new ReporteComprasPorFechaModel();
}
