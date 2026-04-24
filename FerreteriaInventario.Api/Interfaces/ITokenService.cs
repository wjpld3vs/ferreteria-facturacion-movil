using FerreteriaInventario.Api.Models;

namespace FerreteriaInventario.Api.Interfaces;

public interface ITokenService
{
    (string Token, DateTime Expiration) GenerateToken(Usuario usuario);
}
