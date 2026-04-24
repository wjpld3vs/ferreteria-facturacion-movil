using System.ComponentModel.DataAnnotations;

namespace FerreteriaInventario.Api.DTOs;

public class LoginRequestDto
{
    [Required]
    [StringLength(120, MinimumLength = 3)]
    public string UsuarioOEmail { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
    public UsuarioResponseDto Usuario { get; set; } = new();
}
