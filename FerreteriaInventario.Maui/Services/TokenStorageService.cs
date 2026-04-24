using System.Text.Json;
using FerreteriaInventario.Maui.Models;

namespace FerreteriaInventario.Maui.Services;

public class TokenStorageService
{
    private const string TokenKey = "auth_token";
    private const string ExpirationKey = "auth_expiration";
    private const string UserKey = "auth_user";

    public async Task SaveSessionAsync(LoginResponseModel session)
    {
        await SecureStorage.Default.SetAsync(TokenKey, session.Token);
        await SecureStorage.Default.SetAsync(ExpirationKey, session.Expiration.ToString("O"));
        await SecureStorage.Default.SetAsync(UserKey, JsonSerializer.Serialize(session.Usuario));
    }

    public async Task<string?> GetTokenAsync() => await SecureStorage.Default.GetAsync(TokenKey);

    public async Task<DateTime?> GetExpirationAsync()
    {
        var raw = await SecureStorage.Default.GetAsync(ExpirationKey);
        return DateTime.TryParse(raw, out var expiration) ? expiration : null;
    }

    public async Task<UsuarioModel?> GetUserAsync()
    {
        var raw = await SecureStorage.Default.GetAsync(UserKey);
        return string.IsNullOrWhiteSpace(raw) ? null : JsonSerializer.Deserialize<UsuarioModel>(raw);
    }

    public void ClearSession()
    {
        SecureStorage.Default.Remove(TokenKey);
        SecureStorage.Default.Remove(ExpirationKey);
        SecureStorage.Default.Remove(UserKey);
    }
}
