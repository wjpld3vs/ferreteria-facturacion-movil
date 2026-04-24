using FerreteriaInventario.Maui.Models;

namespace FerreteriaInventario.Maui.Services;

public class ClienteService
{
    private readonly ApiService _apiService;

    public ClienteService(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<List<ClienteModel>> GetAllAsync() =>
        await _apiService.GetAsync<List<ClienteModel>>("api/clientes") ?? new List<ClienteModel>();

    public async Task<ClienteModel> CreateAsync(ClienteModel model) =>
        await _apiService.PostAsync<ClienteModel, ClienteModel>("api/clientes", model)
        ?? throw new InvalidOperationException("No fue posible registrar el cliente.");
}
