using FerreteriaInventario.Api.Data;
using FerreteriaInventario.Api.DTOs;
using FerreteriaInventario.Api.Helpers;
using FerreteriaInventario.Api.Models;
using FerreteriaInventario.Api.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
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

    [Fact]
    public async Task GetAllAsync_SinProductos_DeberiaRetornarListaVacia()
    {
        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region SearchAsync Tests

    [Fact]
    public async Task SearchAsync_ConTexto_DeberiaFiltrarPorNombre()
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
    public async Task SearchAsync_ConCodigo_DeberiaEncontrarProducto()
    {
        // Arrange
        await SeedProductos();

        // Act
        var result = await _sut.SearchAsync("PROD-002");

        // Assert
        result.Should().HaveCount(1);
        result[0].Nombre.Should().Be("Destornillador");
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

    [Fact]
    public async Task UpdateAsync_ConProductoInexistente_DeberiaLanzarExcepcion404()
    {
        // Arrange
        var request = new ProductoUpdateDto
        {
            Codigo = "PROD-NEW",
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
        var act = async () => await _sut.UpdateAsync(999, request);
        await act.Should().ThrowAsync<ApiException>()
            .Where(e => e.StatusCode == StatusCodes.Status404NotFound);
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

    [Fact]
    public async Task SetActiveAsync_ConProductoInexistente_DeberiaLanzarExcepcion404()
    {
        // Act & Assert
        var act = async () => await _sut.SetActiveAsync(999, true);
        await act.Should().ThrowAsync<ApiException>()
            .Where(e => e.StatusCode == StatusCodes.Status404NotFound);
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_ConProductoExistente_DeberiaRetornarProducto()
    {
        // Arrange
        await SeedProductos();

        // Act
        var result = await _sut.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Nombre.Should().Be("Alicate");
    }

    [Fact]
    public async Task GetByIdAsync_ConProductoInexistente_DeberiaLanzarExcepcion404()
    {
        // Act & Assert
        var act = async () => await _sut.GetByIdAsync(999);
        await act.Should().ThrowAsync<ApiException>()
            .Where(e => e.StatusCode == StatusCodes.Status404NotFound);
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
