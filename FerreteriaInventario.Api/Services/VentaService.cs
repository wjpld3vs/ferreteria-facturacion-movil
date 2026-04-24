using FerreteriaInventario.Api.Data;
using FerreteriaInventario.Api.DTOs;
using FerreteriaInventario.Api.Helpers;
using FerreteriaInventario.Api.Interfaces;
using FerreteriaInventario.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FerreteriaInventario.Api.Services;

public class VentaService : IVentaService
{
    private readonly AppDbContext _context;

    public VentaService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<VentaResponseDto>> GetAllAsync()
    {
        var ventas = await _context.Ventas
            .Include(x => x.Cliente)
            .Include(x => x.Usuario)
            .Include(x => x.Detalles)
                .ThenInclude(x => x.Producto)
            .OrderByDescending(x => x.Fecha)
            .ToListAsync();

        return ventas.Select(x => x.ToDto()).ToList();
    }

    public async Task<VentaResponseDto> GetByIdAsync(int id)
    {
        var venta = await GetVentaAsync(id);
        return venta.ToDto();
    }

    public async Task<VentaResponseDto> CreateAsync(VentaCreateDto request)
    {
        if (request.Detalles.Count == 0)
        {
            throw new ApiException("No se permite registrar ventas sin detalles.");
        }

        var cliente = await _context.Clientes.FirstOrDefaultAsync(x => x.Id == request.ClienteId);
        if (cliente is null)
        {
            throw new ApiException("Cliente no encontrado.", StatusCodes.Status404NotFound);
        }

        if (!cliente.Activo)
        {
            throw new ApiException("El cliente esta inactivo.");
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

        var venta = new Venta
        {
            ClienteId = request.ClienteId,
            UsuarioId = request.UsuarioId,
            Fecha = DateTime.UtcNow,
            NumeroComprobante = request.NumeroComprobante.Trim(),
            Impuesto = request.Impuesto,
            Descuento = request.Descuento,
            Observaciones = request.Observaciones?.Trim()
        };

        foreach (var detailRequest in request.Detalles)
        {
            if (detailRequest.Cantidad <= 0)
            {
                throw new ApiException("La cantidad de cada detalle de venta debe ser mayor que cero.");
            }

            if (detailRequest.PrecioUnitario < 0)
            {
                throw new ApiException("El precio unitario de venta no puede ser negativo.");
            }

            var producto = await _context.Productos.FirstOrDefaultAsync(x => x.Id == detailRequest.ProductoId);
            if (producto is null)
            {
                throw new ApiException($"Producto con id {detailRequest.ProductoId} no encontrado.", StatusCodes.Status404NotFound);
            }

            if (!producto.Activo)
            {
                throw new ApiException($"El producto {producto.Nombre} esta inactivo y no puede venderse.");
            }

            if (producto.StockActual < detailRequest.Cantidad)
            {
                throw new ApiException($"Stock insuficiente para el producto {producto.Nombre}.");
            }

            var precioUnitario = detailRequest.PrecioUnitario == 0 ? producto.PrecioVenta : detailRequest.PrecioUnitario;
            var subtotal = detailRequest.Cantidad * precioUnitario;

            producto.StockActual -= detailRequest.Cantidad;

            venta.Detalles.Add(new DetalleVenta
            {
                ProductoId = detailRequest.ProductoId,
                Cantidad = detailRequest.Cantidad,
                PrecioUnitario = precioUnitario,
                Subtotal = subtotal
            });
        }

        venta.Subtotal = venta.Detalles.Sum(x => x.Subtotal);
        venta.Total = venta.Subtotal + venta.Impuesto - venta.Descuento;

        if (venta.Total < 0)
        {
            throw new ApiException("El total de la venta no puede ser negativo.");
        }

        _context.Ventas.Add(venta);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        venta = await GetVentaAsync(venta.Id);
        return venta.ToDto();
    }

    private async Task<Venta> GetVentaAsync(int id)
    {
        var venta = await _context.Ventas
            .Include(x => x.Cliente)
            .Include(x => x.Usuario)
            .Include(x => x.Detalles)
                .ThenInclude(x => x.Producto)
            .FirstOrDefaultAsync(x => x.Id == id);

        return venta ?? throw new ApiException("Venta no encontrada.", StatusCodes.Status404NotFound);
    }
}
