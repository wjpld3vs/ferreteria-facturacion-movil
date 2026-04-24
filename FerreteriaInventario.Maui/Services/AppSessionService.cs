using FerreteriaInventario.Maui.Models;

namespace FerreteriaInventario.Maui.Services;

public class AppSessionService
{
    private readonly TokenStorageService _tokenStorageService;

    public AppSessionService(TokenStorageService tokenStorageService)
    {
        _tokenStorageService = tokenStorageService;
    }

    public event EventHandler? SessionChanged;

    public string? Token { get; private set; }
    public UsuarioModel? CurrentUser { get; private set; }
    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(Token) && CurrentUser is not null;
    public bool IsAdmin => CurrentUser?.RolNombre == "Admin";

    public async Task RestoreSessionAsync(AuthService authService)
    {
        Token = await _tokenStorageService.GetTokenAsync();
        CurrentUser = await _tokenStorageService.GetUserAsync();
        var expiration = await _tokenStorageService.GetExpirationAsync();

        if (string.IsNullOrWhiteSpace(Token) || CurrentUser is null || expiration is null || expiration <= DateTime.UtcNow)
        {
            await LogoutAsync();
            return;
        }

        try
        {
            CurrentUser = await authService.GetCurrentUserAsync();
            SessionChanged?.Invoke(this, EventArgs.Empty);
        }
        catch
        {
            await LogoutAsync();
        }
    }

    public async Task SetSessionAsync(LoginResponseModel response)
    {
        Token = response.Token;
        CurrentUser = response.Usuario;
        await _tokenStorageService.SaveSessionAsync(response);
        SessionChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task LogoutAsync()
    {
        Token = null;
        CurrentUser = null;
        _tokenStorageService.ClearSession();
        SessionChanged?.Invoke(this, EventArgs.Empty);
        if (Shell.Current is not null)
        {
            await MainThread.InvokeOnMainThreadAsync(() => Shell.Current.GoToAsync("//login"));
        }
    }
}
