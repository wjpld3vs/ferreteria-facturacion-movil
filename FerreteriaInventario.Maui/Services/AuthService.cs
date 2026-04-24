using FerreteriaInventario.Maui.Models;

namespace FerreteriaInventario.Maui.Services;

public class AuthService
{
    private readonly ApiService _apiService;
    private readonly AppSessionService _sessionService;

    public AuthService(ApiService apiService, AppSessionService sessionService)
    {
        _apiService = apiService;
        _sessionService = sessionService;
    }

    public async Task<LoginResponseModel> LoginAsync(LoginRequestModel request)
    {
        var response = await _apiService.PostAsync<LoginRequestModel, LoginResponseModel>("api/auth/login", request, false)
                       ?? throw new InvalidOperationException("No se recibio respuesta del servidor.");

        await _sessionService.SetSessionAsync(response);
        return response;
    }

    public async Task<UsuarioModel> GetCurrentUserAsync()
    {
        return await _apiService.GetAsync<UsuarioModel>("api/auth/me")
               ?? throw new InvalidOperationException("No fue posible obtener la sesion actual.");
    }

    public Task LogoutAsync() => _sessionService.LogoutAsync();
}
