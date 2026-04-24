using FerreteriaInventario.Maui.Models;

namespace FerreteriaInventario.Maui.Services;

public class ProveedorService
{
    private readonly ApiService _apiService;

    public ProveedorService(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<List<ProveedorModel>> GetAllAsync() =>
        await _apiService.GetAsync<List<ProveedorModel>>("api/proveedores") ?? new List<ProveedorModel>();

    public async Task<ProveedorModel> CreateAsync(ProveedorModel model) =>
        await _apiService.PostAsync<ProveedorModel, ProveedorModel>("api/proveedores", model)
        ?? throw new InvalidOperationException("No fue posible registrar el proveedor.");
}
