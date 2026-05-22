using FerreteriaInventario.Api.Data;
using FerreteriaInventario.Api.DTOs;
using FerreteriaInventario.Api.Helpers;
using FerreteriaInventario.Api.Interfaces;
using FerreteriaInventario.Api.Models;
using FerreteriaInventario.Api.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace FerreteriaInventario.Tests.Unit.Services;

public class AuthServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly AuthService _sut;
    private readonly PasswordHasher<Usuario> _passwordHasher = new();

    public AuthServiceTests()
    {
        var options = TestConfig.CreateInMemoryDbContextOptions();
        _context = new AppDbContext(options);
        _tokenServiceMock = new Mock<ITokenService>();
        _sut = new AuthService(_context, _tokenServiceMock.Object);

        SeedTestData();
    }

    private void SeedTestData()
    {
        var rolAdmin = new Rol { Id = 1, Nombre = "Admin", Descripcion = "Administrador" };
        var rolOperario = new Rol { Id = 2, Nombre = "Operario", Descripcion = "Operario de tienda" };

        var usuarioActivo = new Usuario
        {
            Id = 1,
            Nombre = "Juan Perez",
            NombreUsuario = "jperez",
            Email = "jperez@test.com",
            PasswordHash = _passwordHasher.HashPassword(null!, "Password123!"),
            RolId = 1,
            Activo = true
        };

        var usuarioInactivo = new Usuario
        {
            Id = 2,
            Nombre = "Maria Lopez",
            NombreUsuario = "mlopez",
            Email = "mlopez@test.com",
            PasswordHash = _passwordHasher.HashPassword(null!, "Password123!"),
            RolId = 2,
            Activo = false
        };

        _context.Roles.AddRange(rolAdmin, rolOperario);
        _context.Usuarios.AddRange(usuarioActivo, usuarioInactivo);
        _context.SaveChanges();
    }

    #region LoginAsync Tests

    [Fact]
    public async Task LoginAsync_ConCredencialesValidas_DeberiaRetornarToken()
    {
        // Arrange
        var request = new LoginRequestDto
        {
            UsuarioOEmail = "jperez",
            Password = "Password123!"
        };

        var expectedExpiration = DateTime.UtcNow.AddHours(1);
        _tokenServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<Usuario>()))
            .Returns(("fake-jwt-token", expectedExpiration));

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be("fake-jwt-token");
        result.Expiration.Should().Be(expectedExpiration);
        result.Usuario.NombreUsuario.Should().Be("jperez");
    }

    [Fact]
    public async Task LoginAsync_ConEmailValido_DeberiaRetornarToken()
    {
        // Arrange
        var request = new LoginRequestDto
        {
            UsuarioOEmail = "jperez@test.com",
            Password = "Password123!"
        };

        _tokenServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<Usuario>()))
            .Returns(("fake-jwt-token", DateTime.UtcNow.AddHours(1)));

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        result.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task LoginAsync_ConUsuarioInvalido_DeberiaLanzarExcepcion401()
    {
        // Arrange
        var request = new LoginRequestDto
        {
            UsuarioOEmail = "noexiste",
            Password = "Password123!"
        };

        // Act & Assert
        var act = async () => await _sut.LoginAsync(request);
        (await act.Should().ThrowAsync<ApiException>()
            .WithMessage("*Usuario o contrasena invalidos*"))
            .And.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public async Task LoginAsync_ConPasswordIncorrecto_DeberiaLanzarExcepcion401()
    {
        // Arrange
        var request = new LoginRequestDto
        {
            UsuarioOEmail = "jperez",
            Password = "PasswordIncorrecto"
        };

        // Act & Assert
        var act = async () => await _sut.LoginAsync(request);
        await act.Should().ThrowAsync<ApiException>()
            .WithMessage("*Usuario o contrasena invalidos*");
    }

    [Fact]
    public async Task LoginAsync_ConUsuarioInactivo_DeberiaLanzarExcepcion403()
    {
        // Arrange
        var request = new LoginRequestDto
        {
            UsuarioOEmail = "mlopez",
            Password = "Password123!"
        };

        // Act & Assert
        var act = async () => await _sut.LoginAsync(request);
        (await act.Should().ThrowAsync<ApiException>()
            .WithMessage("*inactivo*"))
            .And.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Theory]
    [InlineData("jperez@test.com")]
    [InlineData("JPEREZ")]
    public async Task LoginAsync_ConTrimYCaseInsensitive_DeberiaNormalizar(string usernameInput)
    {
        // Arrange
        var request = new LoginRequestDto
        {
            UsuarioOEmail = usernameInput,
            Password = "Password123!"
        };

        _tokenServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<Usuario>()))
            .Returns(("fake-jwt-token", DateTime.UtcNow.AddHours(1)));

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region RegisterAsync Tests

    [Fact]
    public async Task RegisterAsync_ConDatosValidos_DeberiaCrearUsuario()
    {
        // Arrange
        var request = new UsuarioCreateDto
        {
            Nombre = "Carlos Rodriguez",
            NombreUsuario = "crodriguez",
            Email = "crodriguez@test.com",
            Password = "SecurePassword123!",
            RolId = 2,
            Activo = true
        };

        // Act
        var result = await _sut.RegisterAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.NombreUsuario.Should().Be("crodriguez");
        result.Email.Should().Be("crodriguez@test.com");
        result.RolNombre.Should().Be("Operario");

        var usuarioEnDb = await _context.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == "crodriguez");
        usuarioEnDb.Should().NotBeNull();
    }

    [Fact]
    public async Task RegisterAsync_ConRolInexistente_DeberiaLanzarExcepcion404()
    {
        // Arrange
        var request = new UsuarioCreateDto
        {
            Nombre = "Test User",
            NombreUsuario = "testuser",
            Email = "test@test.com",
            Password = "Password123!",
            RolId = 999,
            Activo = true
        };

        // Act & Assert
        var act = async () => await _sut.RegisterAsync(request);
        (await act.Should().ThrowAsync<ApiException>()
            .WithMessage("*rol*no existe*"))
            .And.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task RegisterAsync_ConNombreUsuarioDuplicado_DeberiaLanzarExcepcion()
    {
        // Arrange
        var request = new UsuarioCreateDto
        {
            Nombre = "Otro Usuario",
            NombreUsuario = "jperez",
            Email = "otro@test.com",
            Password = "Password123!",
            RolId = 1,
            Activo = true
        };

        // Act & Assert
        var act = async () => await _sut.RegisterAsync(request);
        await act.Should().ThrowAsync<ApiException>()
            .WithMessage("*nombre de usuario*ya esta registrado*");
    }

    [Fact]
    public async Task RegisterAsync_ConEmailDuplicado_DeberiaLanzarExcepcion()
    {
        // Arrange
        var request = new UsuarioCreateDto
        {
            Nombre = "Otro Usuario",
            NombreUsuario = "otronombre",
            Email = "jperez@test.com",
            Password = "Password123!",
            RolId = 1,
            Activo = true
        };

        // Act & Assert
        var act = async () => await _sut.RegisterAsync(request);
        await act.Should().ThrowAsync<ApiException>()
            .WithMessage("*correo*ya esta registrado*");
    }

    [Fact]
    public async Task RegisterAsync_ConTrimEnDatos_DeberiaGuardarSinEspacios()
    {
        // Arrange
        var request = new UsuarioCreateDto
        {
            Nombre = "  Juan Perez  ",
            NombreUsuario = "  jperez2  ",
            Email = "  jperez2@test.com  ",
            Password = "Password123!",
            RolId = 1,
            Activo = true
        };

        // Act
        var result = await _sut.RegisterAsync(request);

        // Assert
        result.Nombre.Should().Be("Juan Perez");
        result.NombreUsuario.Should().Be("jperez2");
        result.Email.Should().Be("jperez2@test.com");
    }

    #endregion

    #region GetMeAsync Tests

    [Fact]
    public async Task GetMeAsync_ConUsuarioExistente_DeberiaRetornarUsuario()
    {
        // Act
        var result = await _sut.GetMeAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.NombreUsuario.Should().Be("jperez");
    }

    [Fact]
    public async Task GetMeAsync_ConUsuarioInexistente_DeberiaLanzarExcepcion404()
    {
        // Act & Assert
        var act = async () => await _sut.GetMeAsync(999);
        (await act.Should().ThrowAsync<ApiException>()
            .WithMessage("*Usuario no encontrado*"))
            .And.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    #endregion

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
