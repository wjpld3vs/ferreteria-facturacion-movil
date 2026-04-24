using FerreteriaInventario.Maui.Models;

namespace FerreteriaInventario.Maui.Services;

public class UsuarioService
{
    private readonly ApiService _apiService;

    public UsuarioService(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<List<UsuarioModel>> GetAllAsync() =>
        await _apiService.GetAsync<List<UsuarioModel>>("api/usuarios") ?? new List<UsuarioModel>();

    public async Task<List<RolModel>> GetRolesAsync() =>
        await _apiService.GetAsync<List<RolModel>>("api/roles") ?? new List<RolModel>();

    public async Task<UsuarioModel> CreateAsync(UsuarioCreateModel model) =>
        await _apiService.PostAsync<UsuarioCreateModel, UsuarioModel>("api/usuarios", model)
        ?? throw new InvalidOperationException("No fue posible registrar el usuario.");
}
