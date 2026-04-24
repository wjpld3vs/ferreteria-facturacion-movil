using FerreteriaInventario.Maui.Models;

namespace FerreteriaInventario.Maui.Services;

public class CompraService
{
    private readonly ApiService _apiService;

    public CompraService(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<List<CompraModel>> GetAllAsync() =>
        await _apiService.GetAsync<List<CompraModel>>("api/compras") ?? new List<CompraModel>();

    public async Task<CompraModel> CreateAsync(CompraCreateModel model) =>
        await _apiService.PostAsync<CompraCreateModel, CompraModel>("api/compras", model)
        ?? throw new InvalidOperationException("No se pudo registrar la compra.");
}
