using FerreteriaInventario.Api.Data;
using FerreteriaInventario.Api.DTOs;
using FerreteriaInventario.Api.Helpers;
using FerreteriaInventario.Api.Interfaces;
using FerreteriaInventario.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FerreteriaInventario.Api.Services;

public class ProductoService : IProductoService
{
    private readonly AppDbContext _context;

    public ProductoService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductoResponseDto>> GetAllAsync()
    {
        var productos = await _context.Productos
            .OrderBy(x => x.Nombre)
            .ToListAsync();

        return productos.Select(x => x.ToDto()).ToList();
    }

    public async Task<ProductoResponseDto> GetByIdAsync(int id)
    {
        var producto = await GetProductoAsync(id);
        return producto.ToDto();
    }

    public async Task<List<ProductoResponseDto>> SearchAsync(string? texto)
    {
        var query = _context.Productos.AsQueryable();

        if (!string.IsNullOrWhiteSpace(texto))
        {
            var search = texto.Trim().ToLower();
            query = query.Where(x =>
                x.Nombre.ToLower().Contains(search) ||
                x.Codigo.ToLower().Contains(search) ||
                x.Categoria.ToLower().Contains(search) ||
                x.Marca.ToLower().Contains(search));
        }

        var productos = await query
            .OrderBy(x => x.Nombre)
            .ToListAsync();

        return productos.Select(x => x.ToDto()).ToList();
    }

    public async Task<List<StockBajoDto>> GetLowStockAsync()
    {
        return await _context.Productos
            .Where(x => x.StockActual <= x.StockMinimo)
            .OrderBy(x => x.StockActual)
            .Select(x => new StockBajoDto
            {
                ProductoId = x.Id,
                Codigo = x.Codigo,
                Nombre = x.Nombre,
                StockActual = x.StockActual,
                StockMinimo = x.StockMinimo
            })
            .ToListAsync();
    }

    public async Task<ProductoResponseDto> CreateAsync(ProductoCreateDto request)
    {
        ValidateProducto(request);

        if (await _context.Productos.AnyAsync(x => x.Codigo.ToLower() == request.Codigo.ToLower()))
        {
            throw new ApiException("El codigo del producto ya existe.");
        }

        var producto = new Producto
        {
            Codigo = request.Codigo.Trim(),
            Nombre = request.Nombre.Trim(),
            Descripcion = request.Descripcion?.Trim(),
            Categoria = request.Categoria.Trim(),
            Marca = request.Marca.Trim(),
            UnidadMedida = request.UnidadMedida.Trim(),
            PrecioCompra = request.PrecioCompra,
            PrecioVenta = request.PrecioVenta,
            StockActual = request.StockActual,
            StockMinimo = request.StockMinimo,
            Activo = request.Activo
        };

        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();
        return producto.ToDto();
    }

    public async Task<ProductoResponseDto> UpdateAsync(int id, ProductoUpdateDto request)
    {
        ValidateProducto(request);

        var producto = await GetProductoAsync(id);

        if (await _context.Productos.AnyAsync(x =>
                x.Id != id &&
                x.Codigo.ToLower() == request.Codigo.ToLower()))
        {
            throw new ApiException("El codigo del producto ya existe.");
        }

        producto.Codigo = request.Codigo.Trim();
        producto.Nombre = request.Nombre.Trim();
        producto.Descripcion = request.Descripcion?.Trim();
        producto.Categoria = request.Categoria.Trim();
        producto.Marca = request.Marca.Trim();
        producto.UnidadMedida = request.UnidadMedida.Trim();
        producto.PrecioCompra = request.PrecioCompra;
        producto.PrecioVenta = request.PrecioVenta;
        producto.StockActual = request.StockActual;
        producto.StockMinimo = request.StockMinimo;
        producto.Activo = request.Activo;

        await _context.SaveChangesAsync();
        return producto.ToDto();
    }

    public async Task<ProductoResponseDto> SetActiveAsync(int id, bool activo)
    {
        var producto = await GetProductoAsync(id);
        producto.Activo = activo;
        await _context.SaveChangesAsync();
        return producto.ToDto();
    }

    private static void ValidateProducto(ProductoCreateDto request)
    {
        if (request.PrecioVenta < request.PrecioCompra)
        {
            throw new ApiException("El precio de venta no puede ser menor al precio de compra.");
        }

        if (request.StockActual < 0)
        {
            throw new ApiException("El stock actual no puede ser negativo.");
        }

        if (request.StockMinimo < 0)
        {
            throw new ApiException("El stock minimo no puede ser negativo.");
        }
    }

    private async Task<Producto> GetProductoAsync(int id)
    {
        var producto = await _context.Productos.FirstOrDefaultAsync(x => x.Id == id);
        return producto ?? throw new ApiException("Producto no encontrado.", StatusCodes.Status404NotFound);
    }
}
