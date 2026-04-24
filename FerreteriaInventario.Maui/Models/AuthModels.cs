namespace FerreteriaInventario.Maui.Models;

public class LoginRequestModel
{
    public string UsuarioOEmail { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponseModel
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
    public UsuarioModel Usuario { get; set; } = new();
}

public class ApiErrorModel
{
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
    public int StatusCode { get; set; }
}
