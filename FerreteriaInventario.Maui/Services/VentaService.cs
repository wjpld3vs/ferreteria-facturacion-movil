using FerreteriaInventario.Maui.Models;

namespace FerreteriaInventario.Maui.Services;

public class VentaService
{
    private readonly ApiService _apiService;

    public VentaService(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<List<VentaModel>> GetAllAsync() =>
        await _apiService.GetAsync<List<VentaModel>>("api/ventas") ?? new List<VentaModel>();

    public async Task<VentaModel> CreateAsync(VentaCreateModel model) =>
        await _apiService.PostAsync<VentaCreateModel, VentaModel>("api/ventas", model)
        ?? throw new InvalidOperationException("No se pudo registrar la venta.");
}
