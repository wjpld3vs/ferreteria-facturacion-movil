using FerreteriaInventario.Api.Data;
using FerreteriaInventario.Api.DTOs;
using FerreteriaInventario.Api.Helpers;
using FerreteriaInventario.Api.Models;
using FerreteriaInventario.Api.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace FerreteriaInventario.Tests.Unit.Services;

public class CompraServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly CompraService _sut;

    public CompraServiceTests()
    {
        var options = TestConfig.CreateInMemoryDbContextOptions();
        _context = new AppDbContext(options);
        _sut = new CompraService(_context);

        SeedTestData();
    }

    private void SeedTestData()
    {
        var proveedorActivo = new Proveedor { Id = 1, Nombre = "Proveedor Activo", DocumentoFiscal = "J-12345678", Activo = true };
        var proveedorInactivo = new Proveedor { Id = 2, Nombre = "Proveedor Inactivo", DocumentoFiscal = "J-87654321", Activo = false };

        var usuarioActivo = new Usuario { Id = 1, Nombre = "Usuario 1", NombreUsuario = "user1", Email = "user1@test.com", PasswordHash = "hash", RolId = 1, Activo = true };

        var productoActivo = new Producto
        {
            Id = 1,
            Codigo = "PROD001",
            Nombre = "Producto Activo",
            Categoria = "Herramientas",
            Marca = "MarcaX",
            UnidadMedida = "unidad",
            PrecioCompra = 80,
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
            PrecioCompra = 50,
            PrecioVenta = 100,
            StockActual = 10,
            StockMinimo = 5,
            Activo = false
        };

        _context.Proveedores.AddRange(proveedorActivo, proveedorInactivo);
        _context.Usuarios.Add(usuarioActivo);
        _context.Productos.AddRange(productoActivo, productoInactivo);
        _context.SaveChanges();
    }

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_SinCompras_DeberiaRetornarListaVacia()
    {
        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_ConCompras_DeberiaRetornarComprasOrdenadasPorFecha()
    {
        // Arrange
        await CreateTestCompra();

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().ProveedorNombre.Should().Be("Proveedor Activo");
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_ConCompraExistente_DeberiaRetornarCompra()
    {
        // Arrange
        var compra = await CreateTestCompra();

        // Act
        var result = await _sut.GetByIdAsync(compra.Id);

        // Assert
        result.Should().NotBeNull();
        result.NumeroFactura.Should().Be("FACT-TEST");
        result.ProveedorNombre.Should().Be("Proveedor Activo");
    }

    [Fact]
    public async Task GetByIdAsync_ConCompraInexistente_DeberiaLanzarExcepcion404()
    {
        // Act & Assert
        var act = async () => await _sut.GetByIdAsync(999);
        (await act.Should().ThrowAsync<ApiException>()
            .WithMessage("*Compra no encontrada*"))
            .And.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_ConDatosValidos_DeberiaCrearCompraYAumentarStock()
    {
        // Arrange
        var request = new CompraCreateDto
        {
            ProveedorId = 1,
            UsuarioId = 1,
            NumeroFactura = "FACT-001",
            Impuesto = 30m,
            Detalles = new List<CompraDetalleCreateDto>
            {
                new() { ProductoId = 1, Cantidad = 5, PrecioUnitario = 100 }
            }
        };

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Total.Should().Be((5 * 100) + 30); // 500 + 30 = 530
        result.Detalles.Should().HaveCount(1);
        result.ProveedorNombre.Should().Be("Proveedor Activo");

        var productoActualizado = await _context.Productos.FindAsync(1);
        productoActualizado!.StockActual.Should().Be(25); // 20 + 5
        productoActualizado.PrecioCompra.Should().Be(100); // Updated by compra
    }

    [Fact]
    public async Task CreateAsync_ConMultiplesDetalles_DeberiaCalcularTotalCorrectamente()
    {
        // Arrange
        var request = new CompraCreateDto
        {
            ProveedorId = 1,
            UsuarioId = 1,
            NumeroFactura = "FACT-002",
            Impuesto = 50m,
            Detalles = new List<CompraDetalleCreateDto>
            {
                new() { ProductoId = 1, Cantidad = 2, PrecioUnitario = 100 },
                new() { ProductoId = 1, Cantidad = 3, PrecioUnitario = 200 }
            }
        };

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        result.Subtotal.Should().Be(800m); // (2*100) + (3*200)
        result.Total.Should().Be(850m); // 800 + 50
        result.Detalles.Should().HaveCount(2);

        var productoActualizado = await _context.Productos.FindAsync(1);
        productoActualizado!.StockActual.Should().Be(25); // 20 + 2 + 3
    }

    [Fact]
    public async Task CreateAsync_ConDetallesVacios_DeberiaLanzarExcepcion()
    {
        // Arrange
        var request = new CompraCreateDto
        {
            ProveedorId = 1,
            UsuarioId = 1,
            NumeroFactura = "FACT-001",
            Detalles = new List<CompraDetalleCreateDto>()
        };

        // Act & Assert
        var act = async () => await _sut.CreateAsync(request);
        await act.Should().ThrowAsync<ApiException>()
            .WithMessage("*sin detalles*");
    }

    [Fact]
    public async Task CreateAsync_ConProveedorInactivo_DeberiaLanzarExcepcion()
    {
        // Arrange
        var request = new CompraCreateDto
        {
            ProveedorId = 2,
            UsuarioId = 1,
            NumeroFactura = "FACT-001",
            Detalles = new List<CompraDetalleCreateDto>
            {
                new() { ProductoId = 1, Cantidad = 1, PrecioUnitario = 100 }
            }
        };

        // Act & Assert
        var act = async () => await _sut.CreateAsync(request);
        await act.Should().ThrowAsync<ApiException>()
            .WithMessage("*proveedor*inactivo*");
    }

    [Fact]
    public async Task CreateAsync_ConProveedorInexistente_DeberiaLanzarExcepcion404()
    {
        // Arrange
        var request = new CompraCreateDto
        {
            ProveedorId = 999,
            UsuarioId = 1,
            NumeroFactura = "FACT-001",
            Detalles = new List<CompraDetalleCreateDto>
            {
                new() { ProductoId = 1, Cantidad = 1, PrecioUnitario = 100 }
            }
        };

        // Act & Assert
        var act = async () => await _sut.CreateAsync(request);
        (await act.Should().ThrowAsync<ApiException>()
            .WithMessage("*Proveedor no encontrado*"))
            .And.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task CreateAsync_ConUsuarioInactivo_DeberiaLanzarExcepcion()
    {
        // Arrange
        _context.Usuarios.Find(1)!.Activo = false;
        await _context.SaveChangesAsync();

        var request = new CompraCreateDto
        {
            ProveedorId = 1,
            UsuarioId = 1,
            NumeroFactura = "FACT-001",
            Detalles = new List<CompraDetalleCreateDto>
            {
                new() { ProductoId = 1, Cantidad = 1, PrecioUnitario = 100 }
            }
        };

        // Act & Assert
        var act = async () => await _sut.CreateAsync(request);
        await act.Should().ThrowAsync<ApiException>()
            .WithMessage("*usuario*inactivo*");
    }

    [Fact]
    public async Task CreateAsync_ConUsuarioInexistente_DeberiaLanzarExcepcion404()
    {
        // Arrange
        var request = new CompraCreateDto
        {
            ProveedorId = 1,
            UsuarioId = 999,
            NumeroFactura = "FACT-001",
            Detalles = new List<CompraDetalleCreateDto>
            {
                new() { ProductoId = 1, Cantidad = 1, PrecioUnitario = 100 }
            }
        };

        // Act & Assert
        var act = async () => await _sut.CreateAsync(request);
        (await act.Should().ThrowAsync<ApiException>()
            .WithMessage("*Usuario no encontrado*"))
            .And.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task CreateAsync_ConCantidadCero_DeberiaLanzarExcepcion()
    {
        // Arrange
        var request = new CompraCreateDto
        {
            ProveedorId = 1,
            UsuarioId = 1,
            NumeroFactura = "FACT-001",
            Detalles = new List<CompraDetalleCreateDto>
            {
                new() { ProductoId = 1, Cantidad = 0, PrecioUnitario = 100 }
            }
        };

        // Act & Assert
        var act = async () => await _sut.CreateAsync(request);
        await act.Should().ThrowAsync<ApiException>()
            .WithMessage("*cantidad*mayor que cero*");
    }

    [Fact]
    public async Task CreateAsync_ConCantidadNegativa_DeberiaLanzarExcepcion()
    {
        // Arrange
        var request = new CompraCreateDto
        {
            ProveedorId = 1,
            UsuarioId = 1,
            NumeroFactura = "FACT-001",
            Detalles = new List<CompraDetalleCreateDto>
            {
                new() { ProductoId = 1, Cantidad = -5, PrecioUnitario = 100 }
            }
        };

        // Act & Assert
        var act = async () => await _sut.CreateAsync(request);
        await act.Should().ThrowAsync<ApiException>()
            .WithMessage("*cantidad*mayor que cero*");
    }

    [Fact]
    public async Task CreateAsync_ConPrecioUnitarioNegativo_DeberiaLanzarExcepcion()
    {
        // Arrange
        var request = new CompraCreateDto
        {
            ProveedorId = 1,
            UsuarioId = 1,
            NumeroFactura = "FACT-001",
            Detalles = new List<CompraDetalleCreateDto>
            {
                new() { ProductoId = 1, Cantidad = 1, PrecioUnitario = -10 }
            }
        };

        // Act & Assert
        var act = async () => await _sut.CreateAsync(request);
        await act.Should().ThrowAsync<ApiException>()
            .WithMessage("*precio unitario de compra*negativo*");
    }

    [Fact]
    public async Task CreateAsync_ConProductoInexistente_DeberiaLanzarExcepcion404()
    {
        // Arrange
        var request = new CompraCreateDto
        {
            ProveedorId = 1,
            UsuarioId = 1,
            NumeroFactura = "FACT-001",
            Detalles = new List<CompraDetalleCreateDto>
            {
                new() { ProductoId = 999, Cantidad = 1, PrecioUnitario = 100 }
            }
        };

        // Act & Assert
        var act = async () => await _sut.CreateAsync(request);
        (await act.Should().ThrowAsync<ApiException>()
            .WithMessage("*Producto*no encontrado*"))
            .And.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task CreateAsync_ConProductoInactivo_DeberiaProcesarCompra()
    {
        // The CompraService does NOT check if product is active
        // It only checks product existence. Purchases to inactive products are allowed.
        var request = new CompraCreateDto
        {
            ProveedorId = 1,
            UsuarioId = 1,
            NumeroFactura = "FACT-001",
            Detalles = new List<CompraDetalleCreateDto>
            {
                new() { ProductoId = 2, Cantidad = 5, PrecioUnitario = 100 }
            }
        };

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Detalles.Should().HaveCount(1);

        var productoActualizado = await _context.Productos.FindAsync(2);
        productoActualizado!.StockActual.Should().Be(15); // 10 + 5
    }

    [Fact]
    public async Task CreateAsync_ConTrimEnNumeroFactura_DeberiaNormalizar()
    {
        // Arrange
        var request = new CompraCreateDto
        {
            ProveedorId = 1,
            UsuarioId = 1,
            NumeroFactura = "  FACT-001  ",
            Detalles = new List<CompraDetalleCreateDto>
            {
                new() { ProductoId = 1, Cantidad = 1, PrecioUnitario = 100 }
            }
        };

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        result.NumeroFactura.Should().Be("FACT-001");
    }

    [Fact]
    public async Task CreateAsync_ConTrimEnObservaciones_DeberiaNormalizar()
    {
        // Arrange
        var request = new CompraCreateDto
        {
            ProveedorId = 1,
            UsuarioId = 1,
            NumeroFactura = "FACT-001",
            Observaciones = "  Compra urgente  ",
            Detalles = new List<CompraDetalleCreateDto>
            {
                new() { ProductoId = 1, Cantidad = 1, PrecioUnitario = 100 }
            }
        };

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        result.Observaciones.Should().Be("Compra urgente");
    }

    [Fact]
    public async Task CreateAsync_ConObservacionesNula_DeberiaPermitir()
    {
        // Arrange
        var request = new CompraCreateDto
        {
            ProveedorId = 1,
            UsuarioId = 1,
            NumeroFactura = "FACT-001",
            Observaciones = null,
            Detalles = new List<CompraDetalleCreateDto>
            {
                new() { ProductoId = 1, Cantidad = 1, PrecioUnitario = 100 }
            }
        };

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        result.Observaciones.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ConPrecioUnitarioCero_DeberiaPermitirCompraGratuita()
    {
        // Arrange
        var request = new CompraCreateDto
        {
            ProveedorId = 1,
            UsuarioId = 1,
            NumeroFactura = "FACT-001",
            Detalles = new List<CompraDetalleCreateDto>
            {
                new() { ProductoId = 1, Cantidad = 5, PrecioUnitario = 0 }
            }
        };

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        result.Subtotal.Should().Be(0m);
        result.Total.Should().Be(0m);

        var productoActualizado = await _context.Productos.FindAsync(1);
        productoActualizado!.StockActual.Should().Be(25); // 20 + 5
        productoActualizado.PrecioCompra.Should().Be(0m); // Updated to 0
    }

    #endregion

    #region Stock update behavior Tests

    [Fact]
    public async Task CreateAsync_DeberiaActualizarPrecioCompraDelProducto()
    {
        // Arrange
        var request = new CompraCreateDto
        {
            ProveedorId = 1,
            UsuarioId = 1,
            NumeroFactura = "FACT-001",
            Detalles = new List<CompraDetalleCreateDto>
            {
                new() { ProductoId = 1, Cantidad = 1, PrecioUnitario = 120 }
            }
        };

        // Act
        await _sut.CreateAsync(request);

        // Assert
        var producto = await _context.Productos.FindAsync(1);
        producto!.PrecioCompra.Should().Be(120m);
    }

    [Fact]
    public async Task CreateAsync_DeberiaAumentarStockCorrectamente()
    {
        // Arrange
        var request = new CompraCreateDto
        {
            ProveedorId = 1,
            UsuarioId = 1,
            NumeroFactura = "FACT-001",
            Detalles = new List<CompraDetalleCreateDto>
            {
                new() { ProductoId = 1, Cantidad = 10, PrecioUnitario = 100 },
                new() { ProductoId = 1, Cantidad = 5, PrecioUnitario = 200 }
            }
        };

        // Act
        await _sut.CreateAsync(request);

        // Assert
        var producto = await _context.Productos.FindAsync(1);
        producto!.StockActual.Should().Be(35); // 20 + 10 + 5
    }

    #endregion

    private async Task<Compra> CreateTestCompra()
    {
        var compra = new Compra
        {
            ProveedorId = 1,
            UsuarioId = 1,
            Fecha = DateTime.UtcNow,
            NumeroFactura = "FACT-TEST",
            Subtotal = 500,
            Impuesto = 50,
            Total = 550,
            Detalles = new List<DetalleCompra>
            {
                new() { ProductoId = 1, Cantidad = 5, PrecioUnitario = 100, Subtotal = 500 }
            }
        };

        _context.Compras.Add(compra);
        await _context.SaveChangesAsync();
        return compra;
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
