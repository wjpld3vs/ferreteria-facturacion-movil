# GUÍA TÉCNICA DE IMPLEMENTACIÓN - TESTING Y CI/CD

---

# PARTE 1: GUÍA TÉCNICA DE TESTING

---

## 1.1 PREPARACIÓN DEL ENTORNO DE TESTING

### Dependencias necesarias

Crear el proyecto de tests dentro de la solución:

```bash
cd FerreteriaInventario.Api
dotnet new xunit -n FerreteriaInventario.Tests -o ../FerreteriaInventario.Tests
dotnet sln add ../FerreteriaInventario.Tests/FerreteriaInventario.Tests.csproj
```

Agregar los siguientes paquetes NuGet al proyecto `FerreteriaInventario.Tests`:

```xml
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.0" />
<PackageReference Include="xunit" Version="2.9.0" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.8.2">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
</PackageReference>
<PackageReference Include="Moq" Version="4.20.70" />
<PackageReference Include="FluentAssertions" Version="6.12.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="9.0.9" />
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="9.0.9" />
<PackageReference Include="coverlet.collector" Version="6.0.2">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
</PackageReference>
```

Referencia al proyecto principal:

```xml
<ItemGroup>
    <ProjectReference Include="..\FerreteriaInventario.Api\FerreteriaInventario.Api.csproj" />
</ItemGroup>
```

### Configuración para tests

Crear `FerreteriaInventario.Tests/TestConfig.cs`:

```csharp
namespace FerreteriaInventario.Tests;

public class TestConfig
{
    public static DbContextOptions<AppDbContext> CreateInMemoryDbContextOptions()
    {
        return new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    public static JwtSettings CreateTestJwtSettings()
    {
        return new JwtSettings
        {
            Key = "test-key-that-is-at-least-32-characters-long-for-hmacsha256",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpirationMinutes = 60
        };
    }

    public static IOptions<JwtSettings> CreateTestJwtOptions()
    {
        return Options.Create(CreateTestJwtSettings());
    }
}
```

### Separación de entornos

El proyecto de tests utiliza:
- **Base de datos**: SQLite InMemory (no afecta desarrollo ni producción)
- **JWT**: Configuración de test segregada
- **Sin acceso a recursos externos**: Todos los servicios externos se mockean

---

## 1.2 ESTRUCTURA DE CARPETAS Y ARCHIVOS

### Ubicación y propósito

```
FerreteriaInventario.Tests/
├── Unit/
│   └── Services/
│       ├── AuthServiceTests.cs          # Tests de lógica de autenticación
│       ├── TokenServiceTests.cs         # Tests de generación JWT
│       ├── VentaServiceTests.cs         # Tests de lógica de ventas
│       ├── ProductoServiceTests.cs      # Tests de gestión de productos
│       ├── CompraServiceTests.cs        # Tests de gestión de compras
│       ├── ClienteServiceTests.cs       # Tests de gestión de clientes
│       └── ProveedorServiceTests.cs     # Tests de gestión de proveedores
├── Integration/
│   └── Controllers/
│       ├── VentasControllerTests.cs     # Tests end-to-end de endpoint ventas
│       └── AuthControllerTests.cs       # Tests end-to-end de endpoint auth
├── Fixtures/
│   ├── TestDbContextFactory.cs          # Factory para crear DbContext in-memory
│   └── TestDataFactory.cs               # Generador de datos de prueba
├── Helpers/
│   └── AssertionHelpers.cs              # Aserciones personalizadas reutilizables
├── TestConfig.cs                        # Configuración centralizada de tests
└── FerreteriaInventario.Tests.csproj   # Archivo de proyecto
```

### Convenciones de nombres

| Elemento | Convención | Ejemplo |
|----------|------------|---------|
| Clase de tests | `[NombreServicio]Tests` | `VentaServiceTests` |
| Método de test | `[Metodo]_[Escenario]_[ResultadoEsperado]` | `CreateAsync_ConStockSuficiente_DeberiaRestarStock` |
| Clase fixture | `[Nombre]Factory` o `[Nombre]Fixture` | `TestDataFactory` |
| Namespace unit tests | `FerreteriaInventario.Tests.Unit.Services` | - |
| Namespace integration tests | `FerreteriaInventario.Tests.Integration.Controllers` | - |

---

## 1.3 IMPLEMENTACIÓN DE TESTS UNITARIOS

### A) AuthServiceTests

**Archivo**: `Unit/Services/AuthServiceTests.cs`

```csharp
using FerreteriaInventario.Api.Data;
using FerreteriaInventario.Api.DTOs;
using FerreteriaInventario.Api.Helpers;
using FerreteriaInventario.Api.Interfaces;
using FerreteriaInventario.Api.Models;
using FerreteriaInventario.Api.Services;
using FluentAssertions;
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
        await act.Should().ThrowAsync<ApiException>()
            .WithMessage("*Usuario o contrasena invalidos*")
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
        await act.Should().ThrowAsync<ApiException>()
            .WithMessage("*inactivo*")
            .And.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Theory]
    [InlineData("  ")]
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
        await act.Should().ThrowAsync<ApiException>()
            .WithMessage("*rol*no existe*")
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
        await act.Should().ThrowAsync<ApiException>()
            .WithMessage("*Usuario no encontrado*")
            .And.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    #endregion

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
```

---

### B) TokenServiceTests

**Archivo**: `Unit/Services/TokenServiceTests.cs`

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FerreteriaInventario.Api.Helpers;
using FerreteriaInventario.Api.Models;
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
```

---

### C) VentaServiceTests

**Archivo**: `Unit/Services/VentaServiceTests.cs`

```csharp
using FerreteriaInventario.Api.Data;
using FerreteriaInventario.Api.DTOs;
using FerreteriaInventario.Api.Helpers;
using FerreteriaInventario.Api.Models;
using FerreteriaInventario.Api.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FerreteriaInventario.Tests.Unit.Services;

public class VentaServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly VentaService _sut;

    public VentaServiceTests()
    {
        var options = TestConfig.CreateInMemoryDbContextOptions();
        _context = new AppDbContext(options);
        _sut = new VentaService(_context);

        SeedTestData();
    }

    private void SeedTestData()
    {
        var clienteActivo = new Cliente { Id = 1, Nombre = "Cliente 1", Documento = "12345678", Activo = true };
        var clienteInactivo = new Cliente { Id = 2, Nombre = "Cliente Inactivo", Documento = "87654321", Activo = false };

        var usuarioActivo = new Usuario { Id = 1, Nombre = "Usuario 1", NombreUsuario = "user1", Email = "user1@test.com", PasswordHash = "hash", RolId = 1, Activo = true };

        var productoActivo = new Producto
        {
            Id = 1,
            Codigo = "PROD001",
            Nombre = "Producto Activo",
            Categoria = "Herramientas",
            Marca = "MarcaX",
            UnidadMedida = "unidad",
            PrecioCompra = 100,
            PrecioVenta = 150,
            StockActual = 20,
            StockMinimo = 5,
            Activo = true
        };

        var productoInactivo = new Producto
        {
            Id = 2,
            Codigo = "PROD002",
            Nombre = "Producto Inactivo",
            Categoria = "Herramientas",
            Marca = "MarcaX",
            UnidadMedida = "unidad",
            PrecioCompra = 100,
            PrecioVenta = 150,
            StockActual = 20,
            StockMinimo = 5,
            Activo = false
        };

        var productoStockBajo = new Producto
        {
            Id = 3,
            Codigo = "PROD003",
            Nombre = "Stock Bajo",
            Categoria = "Herramientas",
            Marca = "MarcaX",
            UnidadMedida = "unidad",
            PrecioCompra = 50,
            PrecioVenta = 80,
            StockActual = 2,
            StockMinimo = 5,
            Activo = true
        };

        _context.Clientes.AddRange(clienteActivo, clienteInactivo);
        _context.Usuarios.Add(usuarioActivo);
        _context.Productos.AddRange(productoActivo, productoInactivo, productoStockBajo);
        _context.SaveChanges();
    }

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_SinVentas_DeberiaRetornarListaVacia()
    {
        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_ConVentas_DeberiaRetornarVentasOrdenadasPorFecha()
    {
        // Arrange
        await CreateTestVenta();

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().ClienteNombre.Should().Be("Cliente 1");
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_ConDatosValidos_DeberiaCrearVentaYRestarStock()
    {
        // Arrange
        var request = new VentaCreateDto
        {
            ClienteId = 1,
            UsuarioId = 1,
            NumeroComprobante = "COMP-001",
            Impuesto = 15m,
            Descuento = 5m,
            Detalles = new List<VentaDetalleCreateDto>
            {
                new() { ProductoId = 1, Cantidad = 3, PrecioUnitario = 0 }
            }
        };

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Total.Should().Be((3 * 150) + 15 - 5); // 450 + 15 - 5 = 460
        result.Detalles.Should().HaveCount(1);

        var productoActualizado = await _context.Productos.FindAsync(1);
        productoActualizado!.StockActual.Should().Be(17); // 20 - 3
    }

    [Fact]
    public async Task CreateAsync_ConDetallesVacios_DeberiaLanzarExcepcion()
    {
        // Arrange
        var request = new VentaCreateDto
        {
            ClienteId = 1,
            UsuarioId = 1,
            NumeroComprobante = "COMP-001",
            Detalles = new List<VentaDetalleCreateDto>()
        };

        // Act & Assert
        var act = async () => await _sut.CreateAsync(request);
        await act.Should().ThrowAsync<ApiException>()
            .WithMessage("*sin detalles*");
    }

    [Fact]
    public async Task CreateAsync_ConClienteInactivo_DeberiaLanzarExcepcion()
    {
        // Arrange
        var request = new VentaCreateDto
        {
            ClienteId = 2,
            UsuarioId = 1,
            NumeroComprobante = "COMP-001",
            Detalles = new List<VentaDetalleCreateDto>
            {
                new() { ProductoId = 1, Cantidad = 1, PrecioUnitario = 0 }
            }
        };

        // Act & Assert
        var act = async () => await _sut.CreateAsync(request);
        await act.Should().ThrowAsync<ApiException>()
            .WithMessage("*cliente*inactivo*");
    }

    [Fact]
    public async Task CreateAsync_ConUsuarioInactivo_DeberiaLanzarExcepcion()
    {
        // Arrange
        _context.Usuarios.Find(1)!.Activo = false;
        await _context.SaveChangesAsync();

        var request = new VentaCreateDto
        {
            ClienteId = 1,
            UsuarioId = 1,
            NumeroComprobante = "COMP-001",
            Detalles = new List<VentaDetalleCreateDto>
            {
                new() { ProductoId = 1, Cantidad = 1, PrecioUnitario = 0 }
            }
        };

        // Act & Assert
        var act = async () => await _sut.CreateAsync(request);
        await act.Should().ThrowAsync<ApiException>()
            .WithMessage("*usuario*inactivo*");
    }

    [Fact]
    public async Task CreateAsync_ConCantidadCero_DeberiaLanzarExcepcion()
    {
        // Arrange
        var request = new VentaCreateDto
        {
            ClienteId = 1,
            UsuarioId = 1,
            NumeroComprobante = "COMP-001",
            Detalles = new List<VentaDetalleCreateDto>
            {
                new() { ProductoId = 1, Cantidad = 0, PrecioUnitario = 0 }
            }
        };

        // Act & Assert
        var act = async () => await _sut.CreateAsync(request);
        await act.Should().ThrowAsync<ApiException>()
            .WithMessage("*cantidad*mayor que cero*");
    }

    [Fact]
    public async Task CreateAsync_ConStockInsuficiente_DeberiaLanzarExcepcion()
    {
        // Arrange
        var request = new VentaCreateDto
        {
            ClienteId = 1,
            UsuarioId = 1,
            NumeroComprobante = "COMP-001",
            Detalles = new List<VentaDetalleCreateDto>
            {
                new() { ProductoId = 3, Cantidad = 10, PrecioUnitario = 0 } // Stock es 2
            }
        };

        // Act & Assert
        var act = async () => await _sut.CreateAsync(request);
        await act.Should().ThrowAsync<ApiException>()
            .WithMessage("*Stock insuficiente*");
    }

    [Fact]
    public async Task CreateAsync_ConProductoInactivo_DeberiaLanzarExcepcion()
    {
        // Arrange
        var request = new VentaCreateDto
        {
            ClienteId = 1,
            UsuarioId = 1,
            NumeroComprobante = "COMP-001",
            Detalles = new List<VentaDetalleCreateDto>
            {
                new() { ProductoId = 2, Cantidad = 1, PrecioUnitario = 0 } // Producto inactivo
            }
        };

        // Act & Assert
        var act = async () => await _sut.CreateAsync(request);
        await act.Should().ThrowAsync<ApiException>()
            .WithMessage("*producto*inactivo*");
    }

    [Fact]
    public async Task CreateAsync_ConPrecioUnitarioNegativo_DeberiaLanzarExcepcion()
    {
        // Arrange
        var request = new VentaCreateDto
        {
            ClienteId = 1,
            UsuarioId = 1,
            NumeroComprobante = "COMP-001",
            Detalles = new List<VentaDetalleCreateDto>
            {
                new() { ProductoId = 1, Cantidad = 1, PrecioUnitario = -10 }
            }
        };

        // Act & Assert
        var act = async () => await _sut.CreateAsync(request);
        await act.Should().ThrowAsync<ApiException>()
            .WithMessage("*precio unitario*negativo*");
    }

    [Fact]
    public async Task CreateAsync_ConDescuentoMayorAlTotal_DeberiaLanzarExcepcion()
    {
        // Arrange
        var request = new VentaCreateDto
        {
            ClienteId = 1,
            UsuarioId = 1,
            NumeroComprobante = "COMP-001",
            Impuesto = 0,
            Descuento = 10000m, // Mayor al subtotal
            Detalles = new List<VentaDetalleCreateDto>
            {
                new() { ProductoId = 1, Cantidad = 1, PrecioUnitario = 150 }
            }
        };

        // Act & Assert
        var act = async () => await _sut.CreateAsync(request);
        await act.Should().ThrowAsync<ApiException>()
            .WithMessage("*total*no puede ser negativo*");
    }

    [Fact]
    public async Task CreateAsync_ConPrecioUnitarioPersonalizado_DeberiaUsarEsePrecio()
    {
        // Arrange
        var precioPersonalizado = 200m;
        var request = new VentaCreateDto
        {
            ClienteId = 1,
            UsuarioId = 1,
            NumeroComprobante = "COMP-001",
            Detalles = new List<VentaDetalleCreateDto>
            {
                new() { ProductoId = 1, Cantidad = 2, PrecioUnitario = precioPersonalizado }
            }
        };

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        result.Detalles.First().PrecioUnitario.Should().Be(precioPersonalizado);
        result.Subtotal.Should().Be(400m); // 2 * 200
    }

    [Fact]
    public async Task CreateAsync_ConTrimEnNumeroComprobante_DeberiaNormalizar()
    {
        // Arrange
        var request = new VentaCreateDto
        {
            ClienteId = 1,
            UsuarioId = 1,
            NumeroComprobante = "  COMP-001  ",
            Detalles = new List<VentaDetalleCreateDto>
            {
                new() { ProductoId = 1, Cantidad = 1, PrecioUnitario = 0 }
            }
        };

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        result.NumeroComprobante.Should().Be("COMP-001");
    }

    #endregion

    private async Task<Venta> CreateTestVenta()
    {
        var venta = new Venta
        {
            ClienteId = 1,
            UsuarioId = 1,
            Fecha = DateTime.UtcNow,
            NumeroComprobante = "COMP-TEST",
            Subtotal = 150,
            Impuesto = 15,
            Total = 160,
            Detalles = new List<DetalleVenta>
            {
                new() { ProductoId = 1, Cantidad = 1, PrecioUnitario = 150, Subtotal = 150 }
            }
        };

        _context.Ventas.Add(venta);
        await _context.SaveChangesAsync();
        return venta;
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
```

---

### D) ProductoServiceTests

**Archivo**: `Unit/Services/ProductoServiceTests.cs`

```csharp
using FerreteriaInventario.Api.Data;
using FerreteriaInventario.Api.DTOs;
using FerreteriaInventario.Api.Helpers;
using FerreteriaInventario.Api.Models;
using FerreteriaInventario.Api.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FerreteriaInventario.Tests.Unit.Services;

public class ProductoServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ProductoService _sut;

    public ProductoServiceTests()
    {
        var options = TestConfig.CreateInMemoryDbContextOptions();
        _context = new AppDbContext(options);
        _sut = new ProductoService(_context);
    }

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ConProductos_DeberiaRetornarOrdenadosPorNombre()
    {
        // Arrange
        await SeedProductos();

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
        result[0].Nombre.Should().Be("Alicate");
        result[1].Nombre.Should().Be("Destornillador");
        result[2].Nombre.Should().Be("Martillo");
    }

    #endregion

    #region SearchAsync Tests

    [Fact]
    public async Task SearchAsync_ConTexto_DeberiaFiltrarPorNombreCodigoCategoriaMarca()
    {
        // Arrange
        await SeedProductos();

        // Act
        var result = await _sut.SearchAsync("ali");

        // Assert
        result.Should().HaveCount(1);
        result[0].Nombre.Should().Be("Alicate");
    }

    [Fact]
    public async Task SearchAsync_ConTextoMayusculas_DeberiaSerCaseInsensitive()
    {
        // Arrange
        await SeedProductos();

        // Act
        var result = await _sut.SearchAsync("MARTILLO");

        // Assert
        result.Should().HaveCount(1);
        result[0].Nombre.Should().Be("Martillo");
    }

    [Fact]
    public async Task SearchAsync_ConTextoVacio_DeberiaRetornarTodos()
    {
        // Arrange
        await SeedProductos();

        // Act
        var result = await _sut.SearchAsync("   ");

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task SearchAsync_SinCoincidencias_DeberiaRetornarListaVacia()
    {
        // Arrange
        await SeedProductos();

        // Act
        var result = await _sut.SearchAsync("xyz");

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetLowStockAsync Tests

    [Fact]
    public async Task GetLowStockAsync_DeberiaRetornarSoloProductosConStockBajo()
    {
        // Arrange
        await SeedProductos();

        // Act
        var result = await _sut.GetLowStockAsync();

        // Assert
        result.Should().HaveCount(1);
        result[0].Nombre.Should().Be("Alicate");
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_ConDatosValidos_DeberiaCrearProducto()
    {
        // Arrange
        var request = new ProductoCreateDto
        {
            Codigo = "NEW-001",
            Nombre = "Nuevo Producto",
            Categoria = "Electricidad",
            Marca = "MarcaX",
            UnidadMedida = "unidad",
            PrecioCompra = 50,
            PrecioVenta = 80,
            StockActual = 10,
            StockMinimo = 5,
            Activo = true
        };

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Codigo.Should().Be("NEW-001");
        result.Nombre.Should().Be("Nuevo Producto");
    }

    [Fact]
    public async Task CreateAsync_ConCodigoDuplicado_DeberiaLanzarExcepcion()
    {
        // Arrange
        await SeedProductos();
        var request = new ProductoCreateDto
        {
            Codigo = "PROD-001",
            Nombre = "Otro Producto",
            Categoria = "Herramientas",
            Marca = "MarcaX",
            UnidadMedida = "unidad",
            PrecioCompra = 50,
            PrecioVenta = 80,
            StockActual = 10,
            StockMinimo = 5
        };

        // Act & Assert
        var act = async () => await _sut.CreateAsync(request);
        await act.Should().ThrowAsync<ApiException>()
            .WithMessage("*codigo*ya existe*");
    }

    [Theory]
    [InlineData(80, 100)] // Venta < Compra
    [InlineData(-10, 100)]
    public async Task CreateAsync_ConPrecioVentaMenorAlCompra_DeberiaLanzarExcepcion(decimal precioVenta, decimal precioCompra)
    {
        // Arrange
        var request = new ProductoCreateDto
        {
            Codigo = "NEW-001",
            Nombre = "Test",
            Categoria = "Herramientas",
            Marca = "MarcaX",
            UnidadMedida = "unidad",
            PrecioCompra = precioCompra,
            PrecioVenta = precioVenta,
            StockActual = 10,
            StockMinimo = 5
        };

        // Act & Assert
        var act = async () => await _sut.CreateAsync(request);
        await act.Should().ThrowAsync<ApiException>()
            .WithMessage("*precio de venta*no puede ser menor*");
    }

    [Theory]
    [InlineData(-1, 5)]
    [InlineData(10, -1)]
    [InlineData(-1, -1)]
    public async Task CreateAsync_ConStockNegativo_DeberiaLanzarExcepcion(decimal stockActual, decimal stockMinimo)
    {
        // Arrange
        var request = new ProductoCreateDto
        {
            Codigo = "NEW-001",
            Nombre = "Test",
            Categoria = "Herramientas",
            Marca = "MarcaX",
            UnidadMedida = "unidad",
            PrecioCompra = 50,
            PrecioVenta = 80,
            StockActual = stockActual,
            StockMinimo = stockMinimo
        };

        // Act & Assert
        var act = async () => await _sut.CreateAsync(request);
        await act.Should().ThrowAsync<ApiException>()
            .WithMessage("*stock*no puede ser negativo*");
    }

    [Fact]
    public async Task CreateAsync_ConTrim_DeberiaNormalizarDatos()
    {
        // Arrange
        var request = new ProductoCreateDto
        {
            Codigo = "  NEW-001  ",
            Nombre = "  Nuevo Producto  ",
            Descripcion = "  Descripcion  ",
            Categoria = "  Herramientas  ",
            Marca = "  MarcaX  ",
            UnidadMedida = "  unidad  ",
            PrecioCompra = 50,
            PrecioVenta = 80,
            StockActual = 10,
            StockMinimo = 5,
            Activo = true
        };

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        result.Codigo.Should().Be("NEW-001");
        result.Nombre.Should().Be("Nuevo Producto");
        result.Descripcion.Should().Be("Descripcion");
        result.Categoria.Should().Be("Herramientas");
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_ConDatosValidos_DeberiaActualizarProducto()
    {
        // Arrange
        await SeedProductos();
        var request = new ProductoUpdateDto
        {
            Codigo = "PROD-001",
            Nombre = "Alicate Actualizado",
            Categoria = "Herramientas",
            Marca = "MarcaX",
            UnidadMedida = "unidad",
            PrecioCompra = 60,
            PrecioVenta = 100,
            StockActual = 25,
            StockMinimo = 10,
            Activo = true
        };

        // Act
        var result = await _sut.UpdateAsync(1, request);

        // Assert
        result.Nombre.Should().Be("Alicate Actualizado");
        result.PrecioVenta.Should().Be(100);
    }

    [Fact]
    public async Task UpdateAsync_ConCodigoDeOtroProducto_DeberiaLanzarExcepcion()
    {
        // Arrange
        await SeedProductos();
        var request = new ProductoUpdateDto
        {
            Codigo = "PROD-002", // Código de otro producto
            Nombre = "Test",
            Categoria = "Herramientas",
            Marca = "MarcaX",
            UnidadMedida = "unidad",
            PrecioCompra = 50,
            PrecioVenta = 80,
            StockActual = 10,
            StockMinimo = 5,
            Activo = true
        };

        // Act & Assert
        var act = async () => await _sut.UpdateAsync(1, request);
        await act.Should().ThrowAsync<ApiException>()
            .WithMessage("*codigo*ya existe*");
    }

    #endregion

    #region SetActiveAsync Tests

    [Fact]
    public async Task SetActiveAsync_DeberiaCambiarEstadoActivo()
    {
        // Arrange
        await SeedProductos();

        // Act
        var result = await _sut.SetActiveAsync(1, false);

        // Assert
        result.Activo.Should().BeFalse();
    }

    #endregion

    private async Task SeedProductos()
    {
        var productos = new[]
        {
            new Producto { Id = 1, Codigo = "PROD-001", Nombre = "Alicate", Descripcion = "Alicate de punta", Categoria = "Herramientas", Marca = "Truper", UnidadMedida = "unidad", PrecioCompra = 80, PrecioVenta = 120, StockActual = 3, StockMinimo = 5, Activo = true },
            new Producto { Id = 2, Codigo = "PROD-002", Nombre = "Destornillador", Descripcion = "Destornillador Phillips", Categoria = "Herramientas", Marca = "Truper", UnidadMedida = "unidad", PrecioCompra = 40, PrecioVenta = 60, StockActual = 15, StockMinimo = 5, Activo = true },
            new Producto { Id = 3, Codigo = "PROD-003", Nombre = "Martillo", Descripcion = "Martillo de carpintero", Categoria = "Herramientas", Marca = "DeWalt", UnidadMedida = "unidad", PrecioCompra = 120, PrecioVenta = 180, StockActual = 8, StockMinimo = 3, Activo = true }
        };

        _context.Productos.AddRange(productos);
        await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
```

---

## 1.4 IMPLEMENTACIÓN DE TESTS DE INTEGRACIÓN

### TestDbContextFactory

**Archivo**: `Fixtures/TestDbContextFactory.cs`

```csharp
using FerreteriaInventario.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace FerreteriaInventario.Tests.Fixtures;

public static class TestDbContextFactory
{
    private static int _databaseCounter = 0;

    public static AppDbContext Create()
    {
        var databaseName = $"TestDb_{Interlocked.Increment(ref _databaseCounter)}_{Guid.NewGuid():N}";

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: databaseName)
            .Options;

        return new AppDbContext(options);
    }

    public static void Destroy(AppDbContext context)
    {
        context.Database.EnsureDeleted();
        context.Dispose();
    }
}
```

### TestDataFactory

**Archivo**: `Fixtures/TestDataFactory.cs`

```csharp
using FerreteriaInventario.Api.Models;
using Microsoft.AspNetCore.Identity;

namespace FerreteriaInventario.Tests.Fixtures;

public static class TestDataFactory
{
    private static readonly PasswordHasher<Usuario> PasswordHasher = new();

    public static (Rol Admin, Rol Operario) CreateRoles()
    {
        return (
            new Rol { Id = 1, Nombre = "Admin", Descripcion = "Administrador" },
            new Rol { Id = 2, Nombre = "Operario", Descripcion = "Operario de tienda" }
        );
    }

    public static Usuario CreateUsuario(string nombreUsuario = "testuser", int rolId = 1, bool activo = true)
    {
        return new Usuario
        {
            Id = 0,
            Nombre = $"Test {nombreUsuario}",
            NombreUsuario = nombreUsuario,
            Email = $"{nombreUsuario}@test.com",
            PasswordHash = PasswordHasher.HashPassword(null!, "TestPassword123!"),
            RolId = rolId,
            Activo = activo
        };
    }

    public static Producto CreateProducto(string codigo = "TEST-001", decimal stockActual = 100, bool activo = true)
    {
        return new Producto
        {
            Id = 0,
            Codigo = codigo,
            Nombre = "Producto de Test",
            Descripcion = "Descripción de test",
            Categoria = "Test",
            Marca = "TestMarca",
            UnidadMedida = "unidad",
            PrecioCompra = 50m,
            PrecioVenta = 100m,
            StockActual = stockActual,
            StockMinimo = 10,
            Activo = activo
        };
    }

    public static Cliente CreateCliente(string documento = "12345678", bool activo = true)
    {
        return new Cliente
        {
            Id = 0,
            Nombre = "Cliente de Test",
            Documento = documento,
            Telefono = "1234567890",
            Email = "cliente@test.com",
            Direccion = "Dirección de test",
            Activo = activo
        };
    }

    public static Proveedor CreateProveedor(string documentoFiscal = "123456789", bool activo = true)
    {
        return new Proveedor
        {
            Id = 0,
            Nombre = "Proveedor de Test",
            DocumentoFiscal = documentoFiscal,
            Telefono = "1234567890",
            Email = "proveedor@test.com",
            Direccion = "Dirección de proveedor",
            Activo = activo
        };
    }

    public static Venta CreateVenta(int clienteId = 1, int usuarioId = 1)
    {
        return new Venta
        {
            Id = 0,
            ClienteId = clienteId,
            UsuarioId = usuarioId,
            Fecha = DateTime.UtcNow,
            NumeroComprobante = $"COMP-{Guid.NewGuid():N}".Substring(0, 20),
            Subtotal = 0,
            Impuesto = 0,
            Descuento = 0,
            Total = 0,
            Detalles = new List<DetalleVenta>()
        };
    }
}
```

### AuthControllerTests

**Archivo**: `Integration/Controllers/AuthControllerTests.cs`

```csharp
using FerreteriaInventario.Api.Data;
using FerreteriaInventario.Api.DTOs;
using FerreteriaInventario.Api.Helpers;
using FerreteriaInventario.Api.Models;
using FerreteriaInventario.Api.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;

namespace FerreteriaInventario.Tests.Integration.Controllers;

public class AuthControllerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly AuthController _controller;
    private readonly PasswordHasher<Usuario> _passwordHasher = new();

    public AuthControllerTests()
    {
        var options = TestConfig.CreateInMemoryDbContextOptions();
        _context = new AppDbContext(options);
        _tokenServiceMock = new Mock<ITokenService>();

        var authService = new AuthService(_context, _tokenServiceMock.Object);
        _controller = new AuthController(authService);

        SeedTestData();
    }

    private void SeedTestData()
    {
        var (admin, operario) = TestDataFactory.CreateRoles();
        _context.Roles.AddRange(admin, operario);

        var usuario = TestDataFactory.CreateUsuario("testuser", 1, true);
        usuario.PasswordHash = _passwordHasher.HashPassword(usuario, "Password123!");
        _context.Usuarios.Add(usuario);
        _context.SaveChanges();
    }

    [Fact]
    public async Task Login_ConCredencialesValidas_DeberiaRetornar200YToken()
    {
        // Arrange
        var request = new LoginRequestDto
        {
            UsuarioOEmail = "testuser",
            Password = "Password123!"
        };

        _tokenServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<Usuario>()))
            .Returns(("fake-jwt-token", DateTime.UtcNow.AddHours(1)));

        // Act
        var result = await _controller.Login(request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<LoginResponseDto>().Subject;
        response.Token.Should().Be("fake-jwt-token");
    }

    [Fact]
    public async Task Login_ConCredencialesInvalidas_DeberiaRetornar401()
    {
        // Arrange
        var request = new LoginRequestDto
        {
            UsuarioOEmail = "testuser",
            Password = "WrongPassword"
        };

        // Act
        var result = await _controller.Login(request);

        // Assert
        var unauthorizedResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        unauthorizedResult.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task Register_ConDatosValidos_DeberiaRetornar201()
    {
        // Arrange
        var request = new UsuarioCreateDto
        {
            Nombre = "Nuevo Usuario",
            NombreUsuario = "nuevousuario",
            Email = "nuevo@test.com",
            Password = "Password123!",
            RolId = 2,
            Activo = true
        };

        // Act
        var result = await _controller.Register(request);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
        var response = createdResult.Value.Should().BeOfType<UsuarioResponseDto>().Subject;
        response.NombreUsuario.Should().Be("nuevousuario");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}

public class AuthController
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    public async Task<ActionResult<LoginResponseDto>> Login(LoginRequestDto request)
    {
        try
        {
            var result = await _authService.LoginAsync(request);
            return Ok(result);
        }
        catch (ApiException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
    }

    public async Task<ActionResult<UsuarioResponseDto>> Register(UsuarioCreateDto request)
    {
        try
        {
            var result = await _authService.RegisterAsync(request);
            return CreatedAtAction(nameof(GetMe), new { id = result.Id }, result);
        }
        catch (ApiException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
    }

    public async Task<ActionResult<UsuarioResponseDto>> GetMe(int id)
    {
        try
        {
            var result = await _authService.GetMeAsync(id);
            return Ok(result);
        }
        catch (ApiException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
    }
}
```

---

## 1.5 IMPLEMENTACIÓN CON ENFOQUE TDD

### Flujo TDD para nueva funcionalidad

**Ejemplo**: Agregar método `DeleteAsync` a `ProductoService`

**Paso 1 - RED (Escribir test primero)**:

```csharp
[Fact]
public async Task DeleteAsync_ConProductoExistente_DeberiaEliminarProducto()
{
    // Arrange
    await SeedProductos();

    // Act
    await _sut.DeleteAsync(1);

    // Assert
    var producto = await _context.Productos.FindAsync(1);
    producto.Should().BeNull();
}

[Fact]
public async Task DeleteAsync_ConProductoInexistente_DeberiaLanzarExcepcion404()
{
    // Act & Assert
    var act = async () => await _sut.DeleteAsync(999);
    await act.Should().ThrowAsync<ApiException>()
        .Where(e => e.StatusCode == StatusCodes.Status404NotFound);
}
```

**Paso 2 - GREEN (Implementar mínimo para pasar)**:

En `IProductoService`:
```csharp
Task DeleteAsync(int id);
```

En `ProductoService`:
```csharp
public async Task DeleteAsync(int id)
{
    var producto = await GetProductoAsync(id);
    _context.Productos.Remove(producto);
    await _context.SaveChangesAsync();
}
```

**Paso 3 - REFACTOR**:
- Verificar que tests siguen pasando
- Opcional: agregar tests de caso límite adicionales

### Flujo TDD para refactor

**Ejemplo**: Refactorizar cálculo de totales en `VentaService.CreateAsync`

1. **Escribir tests que describan el comportamiento actual** (deben pasar antes del refactor)
2. **Refactorizar el código** (extraer método, renombrar, etc.)
3. **Verificar que tests siguen pasando**

---

## 1.6 COBERTURA, CALIDAD Y MANTENIMIENTO

### Configuración de coverage

Agregar a `FerreteriaInventario.Tests.csproj`:

```xml
<PropertyGroup>
    <CollectCoverage>true</CollectCoverage>
    <CoverletOutputFormat>opencover</CoverletOutputFormat>
    <CoverletOutput>./coverage/coverage.xml</CoverletOutput>
    <Threshold>80</Threshold>
    <ThresholdType>line</ThresholdType>
</PropertyGroup>
```

### Ejecución de tests con coverage

```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Criterios de calidad

| Criterio | Descripción |
|----------|-------------|
| Tests rápidos | Unit tests < 100ms cada uno |
| Tests independientes | Ningún test depende de otro |
| Nombres descriptivos | El nombre describe qué y cuándo falla |
| AAA Pattern | Arrange, Act, Assert claramente separados |
| Un assert por test | Preferible para localizar errores |
| No lógica en tests | Tests simples y directos |

### Detección de tests frágiles

Revisar en PR:
- Tests que tocan archivos, red, o servicios externos
- Tests con `Thread.Sleep` o delays
- Tests con datos compartidos mutables
- Tests con nombres genéricos (`Test1`, `Test2`)

---

## 1.7 CHECKLIST DE IMPLEMENTACIÓN

### Preparación
- [ ] Proyecto de tests creado dentro de la solución
- [ ] Paquetes xUnit, Moq, FluentAssertions, EF InMemory instalados
- [ ] Proyecto referencing al proyecto API
- [ ] `TestConfig.cs` creado con configuración reusable
- [ ] `TestDbContextFactory` creado
- [ ] `TestDataFactory` creado

### Tests Unitarios - Auth y Token
- [ ] AuthServiceTests con todos los casos de LoginAsync
- [ ] AuthServiceTests con todos los casos de RegisterAsync
- [ ] TokenServiceTests con validación de claims
- [ ] Cobertura de AuthService > 80%

### Tests Unitarios - Venta y Producto
- [ ] VentaServiceTests con casos happy path
- [ ] VentaServiceTests con validación de stock
- [ ] VentaServiceTests con casos de error
- [ ] ProductoServiceTests con SearchAsync
- [ ] ProductoServiceTests con CreateAsync/UpdateAsync
- [ ] Cobertura de VentaService > 80%
- [ ] Cobertura de ProductoService > 80%

### Tests de Integración
- [ ] TestDbContextFactory funcional
- [ ] Integration tests para AuthController
- [ ] Integration tests para VentasController
- [ ] Limpieza de datos después de cada test

### Configuración CI
- [ ] Script de ejecución de tests en GitHub Actions
- [ ] Reporte de coverage publicado
- [ ] Umbral de coverage al 80%
- [ ] Reglas de branch protection configuradas

---

# PARTE 2: GUÍA TÉCNICA CI/CD CON GITHUB ACTIONS

---

## 2.1 OBJETIVOS DEL PIPELINE

El pipeline debe garantizar:
- Validación automática en cada Pull Request
- Ejecución de todos los tests unitarios y de integración
- Validación de build del proyecto
- Reporte de cobertura de código
- Bloqueo de merges que no cumplan los criterios de calidad
- Despliegues seguros a staging/producción

---

## 2.2 ESTRUCTURA DE WORKFLOWS

```
.github/
└── workflows/
    ├── ci.yml              # PR: build + tests + coverage
    ├── main.yml            # Merge a main: todo + deploy staging
    └── deploy-production.yml  # Despliegue a producción (manual)
```

---

## 2.3 IMPLEMENTACIÓN PASO A PASO

### Workflow CI (Pull Requests)

**Archivo**: `.github/workflows/ci.yml`

```yaml
name: CI - Pull Request

on:
  pull_request:
    branches: [main]
    types: [opened, synchronize, reopened]

env:
  DOTNET_VERSION: '9.0.x'
  NODE_VERSION: '20.x'

jobs:
  build-and-test:
    name: Build & Test
    runs-on: ubuntu-latest

    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}

      - name: Restore dependencies
        run: dotnet restore FerreteriaInventarioApp.sln

      - name: Build solution
        run: dotnet build FerreteriaInventarioApp.sln --configuration Release --no-restore

      - name: Run Unit Tests
        run: dotnet test FerreteriaInventario.Tests/FerreteriaInventario.Tests.csproj --configuration Release --no-build --verbosity normal --filter "FullyQualifiedName~Unit"

      - name: Run Integration Tests
        run: dotnet test FerreteriaInventario.Tests/FerreteriaInventario.Tests.csproj --configuration Release --no-build --verbosity normal --filter "FullyQualifiedName~Integration"

      - name: Run All Tests with Coverage
        run: |
          dotnet test FerreteriaInventario.Tests/FerreteriaInventario.Tests.csproj \
            --configuration Release \
            --no-build \
            --verbosity normal \
            --collect:"XPlat Code Coverage" \
            /p:CoverletOutputFormat=opencover \
            /p:CoverletOutput=./coverage/

      - name: Upload coverage reports
        uses: actions/upload-artifact@v4
        with:
          name: coverage-reports
          path: FerreteriaInventario.Tests/coverage/
          retention-days: 7

      - name: Generate coverage summary
        run: |
          dotnet tool install --global dotnet-reportgenerator-globaltool || true
          reportgenerator \
            -reports:FerreteriaInventario.Tests/coverage/coverage.opencover.xml \
            -targetdir:./coverage/report \
            -reporttypes:HtmlSummary

      - name: Upload coverage summary
        uses: actions/upload-artifact@v4
        with:
          name: coverage-summary
          path: FerreteriaInventario.Tests/coverage/report/

      - name: Check coverage threshold
        run: |
          $coverage = (Get-Content ./coverage/coverage.xml -Raw | Select-String -Pattern "linecoverage=""([0-9.]+)""" | ForEach-Object { $_.Matches.Groups[1].Value } | Measure-Object -Average).Average
          if ($coverage -lt 80) {
            Write-Host "Coverage $coverage% is below 80% threshold"
            exit 1
          }
          Write-Host "Coverage: $coverage% - PASSED"
        shell: pwsh

  quality-gate:
    name: Quality Gate
    runs-on: ubuntu-latest
    needs: build-and-test
    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Run SonarQube scan (optional)
        run: echo "SonarQube analysis would run here"
```

### Workflow Main (Merge y Release)

**Archivo**: `.github/workflows/main.yml`

```yaml
name: CI - Main Branch

on:
  push:
    branches: [main]
  release:
    types: [published]

env:
  DOTNET_VERSION: '9.0.x'

jobs:
  build-test-and-deploy:
    name: Build, Test & Deploy Staging
    runs-on: ubuntu-latest
    environment: staging

    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}

      - name: Restore dependencies
        run: dotnet restore FerreteriaInventarioApp.sln

      - name: Build
        run: dotnet build FerreteriaInventarioApp.sln --configuration Release --no-restore

      - name: Run all tests
        run: dotnet test FerreteriaInventario.Tests/FerreteriaInventario.Tests.csproj --configuration Release --no-build --verbosity normal

      - name: Publish API
        run: |
          dotnet publish FerreteriaInventario.Api/FerreteriaInventario.Api.csproj \
            --configuration Release \
            --output ./publish/api

      - name: Upload API artifact
        uses: actions/upload-artifact@v4
        with:
          name: api-package
          path: ./publish/api/
          retention-days: 30

      - name: Deploy to Staging
        run: echo "Deployment to staging server would happen here"
        # Ejemplo para Azure Web App:
        # - name: Deploy to Azure Web App
        #   uses: azure/webapps-deploy@v3
        #   with:
        #     app-name: ${{ env.AZURE_WEBAPP_NAME }}
        #     publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
        #     package: ./publish/api/
```

### Workflow Deploy Production

**Archivo**: `.github/workflows/deploy-production.yml`

```yaml
name: Deploy - Production

on:
  workflow_dispatch:
    inputs:
      version:
        description: 'Version to deploy (e.g., v1.0.0)'
        required: true
        type: string

environment:
  name: production
  url: https://ferreteria.example.com

jobs:
  deploy-production:
    name: Deploy to Production
    runs-on: ubuntu-latest
    environment: production
    concurrency:
      group: production-deploy

    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '9.0.x'

      - name: Restore dependencies
        run: dotnet restore FerreteriaInventarioApp.sln

      - name: Build
        run: dotnet build FerreteriaInventarioApp.sln --configuration Release --no-restore

      - name: Run tests
        run: dotnet test FerreteriaInventario.Tests/FerreteriaInventario.Tests.csproj --configuration Release --no-build

      - name: Download API artifact
        uses: actions/download-artifact@v4
        with:
          name: api-package
          path: ./publish/api/

      - name: Deploy to Production
        run: echo "Production deployment with version ${{ inputs.version }}"
        # Usar Azure, AWS, o el proveedor de hosting correspondiente
```

---

## 2.4 ESTRATEGIA DE EJECUCIÓN DE TESTS

### Ejecución por evento

| Evento | Tests ejecutados | Tiempo máximo |
|--------|-----------------|---------------|
| PR | Unit + Integration | 10 min |
| Merge main | Unit + Integration + Smoke | 15 min |
| Release | Unit + Integration + Full suite | 20 min |

### Separación de jobs

```yaml
jobs:
  unit-tests:
    name: Unit Tests
    runs-on: ubuntu-latest
    steps:
      - run: dotnet test --filter "FullyQualifiedName~Unit" --verbosity normal

  integration-tests:
    name: Integration Tests
    runs-on: ubuntu-latest
    needs: unit-tests  # Solo si unit tests pasan
    steps:
      - run: dotnet test --filter "FullyQualifiedName~Integration" --verbosity normal
```

### Parallelización

```yaml
strategy:
  matrix:
    testAssembly:
      - FerreteriaInventario.Tests
    # Para proyectos más grandes, agregar más assemblies al matrix
```

---

## 2.5 REGLAS DE PROTECCIÓN DE RAMAS

Configurar en GitHub → Settings → Branches → Branch protection rules:

### Para rama `main`

| Configuración | Valor |
|--------------|-------|
| Require pull request before merging | ✅ Enabled |
| Require approvals | ✅ 1 mínimo |
| Dismiss stale approvals | ✅ Enabled |
| Require status checks to pass before merging | ✅ Enabled |
| Required status checks | `build-and-test`, `quality-gate` |
| Require branches to be up to date before merging | ✅ Enabled |
| Do not allow bypassing the above settings | ✅ Enabled |
| Include administrators | ✅ Enabled |

---

## 2.6 ESTRATEGIA DE DESPLIEGUE

### Flujo recomendado

```
Feature Branch → PR → Merge Main → Auto-Deploy Staging → Manual Deploy Production
```

### Environments en GitHub

Crear en GitHub → Settings → Environments:

**Staging**:
- Protection rules: None (auto-deploy)
- Secrets: `AZURE_WEBAPP_NAME`, `AZURE_WEBAPP_PUBLISH_PROFILE`

**Production**:
- Protection rules: Required reviewers (1 approve)
- Wait timer: 0
- Secrets: `AZURE_WEBAPP_NAME`, `AZURE_WEBAPP_PUBLISH_PROFILE`

---

## 2.7 SEGURIDAD Y BUENAS PRÁCTICAS

### Secrets

Usar GitHub Secrets para:
- `AZURE_WEBAPP_PUBLISH_PROFILE`
- `JWT_SECRET` (si se usa en CI)
- Credenciales de base de datos de staging

### Permisos de workflows

```yaml
permissions:
  contents: read
  pull-requests: write
  actions: read
  checks: write
  deployments: write
```

### Dependabot

Habilitar `dependabot.yml`:

```yaml
version: 2
updates:
  - package-ecosystem: "nuget"
    directory: "/"
    schedule:
      interval: "weekly"
    open-pull-requests-limit: 10
```

### Buenas prácticas adicionales

- No exponer secrets en logs (`::debug::` o condicional)
- Usar tags de versiones para releases
- Mantener workflows idempotentes
- Documentar cada workflow con comentarios

---

## 2.8 CHECKLIST CI/CD

### Workflows
- [ ] `.github/workflows/ci.yml` creado
- [ ] `.github/workflows/main.yml` creado
- [ ] `.github/workflows/deploy-production.yml` creado
- [ ] Jobs configurados correctamente
- [ ] Cache configurado para dotnet restore
- [ ] Artifacts configurados para persistir entre jobs

### Ejecución de tests
- [ ] Unit tests en PR
- [ ] Integration tests en PR
- [ ] Coverage reportado en PR
- [ ] Threshold de 80% configurado

### Protección de ramas
- [ ] Reglas de branch protection configuradas para main
- [ ] Status checks requeridos
- [ ] Approvals requeridos

### Secrets
- [ ] Secrets configurados en GitHub
- [ ] Producción requiere approval manual

### Documentación
- [ ] README actualizado con badges de CI
- [ ] Secrets documentados (no valores, solo qué se necesita)

---

## RESUMEN DE ARCHIVOS A CREAR

```
FerreteriaInventario.Tests/
├── FerreteriaInventario.Tests.csproj
├── TestConfig.cs
├── Unit/
│   └── Services/
│       ├── AuthServiceTests.cs
│       ├── TokenServiceTests.cs
│       ├── VentaServiceTests.cs
│       └── ProductoServiceTests.cs
├── Integration/
│   └── Controllers/
│       └── AuthControllerTests.cs
├── Fixtures/
│   ├── TestDbContextFactory.cs
│   └── TestDataFactory.cs
└── Helpers/
    └── AssertionHelpers.cs

.github/
└── workflows/
    ├── ci.yml
    ├── main.yml
    └── deploy-production.yml
```