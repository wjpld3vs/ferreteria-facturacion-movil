using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FerreteriaInventario.Api.Helpers;
using FerreteriaInventario.Api.Models;
using FerreteriaInventario.Api.Services;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;

namespace FerreteriaInventario.Tests.Unit.Services;

public class TokenServiceTests
{
    private readonly TokenService _sut;

    public TokenServiceTests()
    {
        _sut = new TokenService(TestConfig.CreateTestJwtOptions());
    }

    [Fact]
    public void GenerateToken_ConUsuarioValido_DeberiaRetornarTokenYExpiration()
    {
        // Arrange
        var usuario = new Usuario
        {
            Id = 1,
            Nombre = "Juan Perez",
            Email = "jperez@test.com",
            NombreUsuario = "jperez",
            Rol = new Rol { Id = 1, Nombre = "Admin" }
        };

        // Act
        var (token, expiration) = _sut.GenerateToken(usuario);

        // Assert
        token.Should().NotBeNullOrEmpty();
        expiration.Should().BeAfter(DateTime.UtcNow);
        expiration.Should().BeBefore(DateTime.UtcNow.AddMinutes(61));
    }

    [Fact]
    public void GenerateToken_DeberiaIncluirClaimsCorrectos()
    {
        // Arrange
        var usuario = new Usuario
        {
            Id = 42,
            Nombre = "Maria Garcia",
            Email = "maria@test.com",
            NombreUsuario = "maria",
            Rol = new Rol { Id = 2, Nombre = "Operario" }
        };

        // Act
        var (token, _) = _sut.GenerateToken(usuario);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == "42");
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == "Maria Garcia");
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == "maria@test.com");
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Operario");
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.UniqueName && c.Value == "maria");
    }

    [Fact]
    public void GenerateToken_ConRolNulo_DeberiaManejarGraciosamente()
    {
        // Arrange
        var usuario = new Usuario
        {
            Id = 1,
            Nombre = "Test",
            Email = "test@test.com",
            NombreUsuario = "test",
            Rol = null!
        };

        // Act
        var (token, _) = _sut.GenerateToken(usuario);

        // Assert
        token.Should().NotBeNullOrEmpty();

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == string.Empty);
    }

    [Fact]
    public void GenerateToken_DeberiaTenerExpirationCorrecta()
    {
        // Arrange
        var usuario = new Usuario
        {
            Id = 1,
            Nombre = "Test",
            Email = "test@test.com",
            NombreUsuario = "test",
            Rol = new Rol { Nombre = "Admin" }
        };
        var beforeGeneration = DateTime.UtcNow;

        // Act
        var (_, expiration) = _sut.GenerateToken(usuario);
        var afterGeneration = DateTime.UtcNow;

        // Assert
        var expectedMinExpiration = beforeGeneration.AddMinutes(TestConfig.CreateTestJwtSettings().ExpirationMinutes);
        var expectedMaxExpiration = afterGeneration.AddMinutes(TestConfig.CreateTestJwtSettings().ExpirationMinutes);

        expiration.Should().BeOnOrAfter(expectedMinExpiration);
        expiration.Should().BeOnOrBefore(expectedMaxExpiration);
    }

    [Fact]
    public void GenerateToken_DeberiaGenerarTokensUnicos()
    {
        // Arrange
        var usuario = new Usuario
        {
            Id = 1,
            Nombre = "Test",
            Email = "test@test.com",
            NombreUsuario = "test",
            Rol = new Rol { Nombre = "Admin" }
        };

        // Act
        var (token1, _) = _sut.GenerateToken(usuario);
        var (token2, _) = _sut.GenerateToken(usuario);

        // Assert
        token1.Should().NotBe(token2);
    }
}
