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
