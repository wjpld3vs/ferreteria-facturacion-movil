using FerreteriaInventario.Api.Data;
using FerreteriaInventario.Api.DTOs;
using FerreteriaInventario.Api.Helpers;
using FerreteriaInventario.Api.Interfaces;
using FerreteriaInventario.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FerreteriaInventario.Api.Services;

public class CompraService : ICompraService
{
    private readonly AppDbContext _context;

    public CompraService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<CompraResponseDto>> GetAllAsync()
    {
        var compras = await _context.Compras
            .Include(x => x.Proveedor)
            .Include(x => x.Usuario)
            .Include(x => x.Detalles)
                .ThenInclude(x => x.Producto)
            .OrderByDescending(x => x.Fecha)
            .ToListAsync();

        return compras.Select(x => x.ToDto()).ToList();
    }

    public async Task<CompraResponseDto> GetByIdAsync(int id)
    {
        var compra = await GetCompraAsync(id);
        return compra.ToDto();
    }

    public async Task<CompraResponseDto> CreateAsync(CompraCreateDto request)
    {
        if (request.Detalles.Count == 0)
        {
            throw new ApiException("No se permite registrar compras sin detalles.");
        }

        var proveedor = await _context.Proveedores.FirstOrDefaultAsync(x => x.Id == request.ProveedorId);
        if (proveedor is null)
        {
            throw new ApiException("Proveedor no encontrado.", StatusCodes.Status404NotFound);
        }

        if (!proveedor.Activo)
        {
            throw new ApiException("El proveedor esta inactivo.");
        }

        var usuario = await _context.Usuarios.FirstOrDefaultAsync(x => x.Id == request.UsuarioId);
        if (usuario is null)
        {
            throw new ApiException("Usuario no encontrado.", StatusCodes.Status404NotFound);
        }

        if (!usuario.Activo)
        {
            throw new ApiException("El usuario esta inactivo.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        var compra = new Compra
        {
            ProveedorId = request.ProveedorId,
            UsuarioId = request.UsuarioId,
            Fecha = DateTime.UtcNow,
            NumeroFactura = request.NumeroFactura.Trim(),
            Impuesto = request.Impuesto,
            Observaciones = request.Observaciones?.Trim()
        };

        foreach (var detailRequest in request.Detalles)
        {
            if (detailRequest.Cantidad <= 0)
            {
                throw new ApiException("La cantidad de cada detalle de compra debe ser mayor que cero.");
            }

            if (detailRequest.PrecioUnitario < 0)
            {
                throw new ApiException("El precio unitario de compra no puede ser negativo.");
            }

            var producto = await _context.Productos.FirstOrDefaultAsync(x => x.Id == detailRequest.ProductoId);
            if (producto is null)
            {
                throw new ApiException($"Producto con id {detailRequest.ProductoId} no encontrado.", StatusCodes.Status404NotFound);
            }

            var subtotal = detailRequest.Cantidad * detailRequest.PrecioUnitario;
            producto.StockActual += detailRequest.Cantidad;
            producto.PrecioCompra = detailRequest.PrecioUnitario;

            compra.Detalles.Add(new DetalleCompra
            {
                ProductoId = detailRequest.ProductoId,
                Cantidad = detailRequest.Cantidad,
                PrecioUnitario = detailRequest.PrecioUnitario,
                Subtotal = subtotal
            });
        }

        compra.Subtotal = compra.Detalles.Sum(x => x.Subtotal);
        compra.Total = compra.Subtotal + compra.Impuesto;

        _context.Compras.Add(compra);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        compra = await GetCompraAsync(compra.Id);
        return compra.ToDto();
    }

    private async Task<Compra> GetCompraAsync(int id)
    {
        var compra = await _context.Compras
            .Include(x => x.Proveedor)
            .Include(x => x.Usuario)
            .Include(x => x.Detalles)
                .ThenInclude(x => x.Producto)
            .FirstOrDefaultAsync(x => x.Id == id);

        return compra ?? throw new ApiException("Compra no encontrada.", StatusCodes.Status404NotFound);
    }
}
