using FerreteriaInventario.Api.Data;
using FerreteriaInventario.Api.DTOs;
using FerreteriaInventario.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FerreteriaInventario.Api.Services;

public class ReporteService : IReporteService
{
    private readonly AppDbContext _context;

    public ReporteService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ReporteInventarioDto>> GetInventarioAsync()
    {
        return await _context.Productos
            .OrderBy(x => x.Nombre)
            .Select(x => new ReporteInventarioDto
            {
                ProductoId = x.Id,
                Codigo = x.Codigo,
                Nombre = x.Nombre,
                Categoria = x.Categoria,
                Marca = x.Marca,
                StockActual = x.StockActual,
                StockMinimo = x.StockMinimo,
                PrecioCompra = x.PrecioCompra,
                PrecioVenta = x.PrecioVenta,
                ValorInventario = x.StockActual * x.PrecioCompra,
                StockBajo = x.StockActual <= x.StockMinimo
            })
            .ToListAsync();
    }

    public async Task<List<StockBajoDto>> GetStockBajoAsync()
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

    public async Task<ReporteVentasPorFechaDto> GetVentasPorFechaAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        var start = fechaInicio.Date;
        var endExclusive = fechaFin.Date.AddDays(1);

        var ventas = await _context.Ventas
            .Include(x => x.Cliente)
            .Include(x => x.Detalles)
                .ThenInclude(x => x.Producto)
            .Where(x => x.Fecha >= start && x.Fecha < endExclusive)
            .OrderBy(x => x.Fecha)
            .ToListAsync();

        return new ReporteVentasPorFechaDto
        {
            FechaInicio = start,
            FechaFin = fechaFin.Date,
            TotalPeriodo = ventas.Sum(x => x.Total),
            Movimientos = ventas.Select(x => new ReporteMovimientoDto
            {
                Fecha = x.Fecha,
                NumeroDocumento = x.NumeroComprobante,
                EntidadNombre = x.Cliente?.Nombre ?? string.Empty,
                Subtotal = x.Subtotal,
                Impuesto = x.Impuesto,
                Descuento = x.Descuento,
                Total = x.Total
            }).ToList(),
            TotalesPorDia = ventas
                .GroupBy(x => x.Fecha.Date)
                .Select(group => new TotalPorDiaDto
                {
                    Fecha = group.Key,
                    Total = group.Sum(x => x.Total)
                })
                .OrderBy(x => x.Fecha)
                .ToList(),
            ProductosMasVendidos = ventas
                .SelectMany(x => x.Detalles)
                .GroupBy(x => new { x.ProductoId, Nombre = x.Producto!.Nombre })
                .Select(group => new ProductoMasVendidoDto
                {
                    ProductoId = group.Key.ProductoId,
                    Nombre = group.Key.Nombre,
                    CantidadVendida = group.Sum(x => x.Cantidad)
                })
                .OrderByDescending(x => x.CantidadVendida)
                .Take(5)
                .ToList()
        };
    }

    public async Task<ReporteComprasPorFechaDto> GetComprasPorFechaAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        var start = fechaInicio.Date;
        var endExclusive = fechaFin.Date.AddDays(1);

        var compras = await _context.Compras
            .Include(x => x.Proveedor)
            .Where(x => x.Fecha >= start && x.Fecha < endExclusive)
            .OrderBy(x => x.Fecha)
            .ToListAsync();

        return new ReporteComprasPorFechaDto
        {
            FechaInicio = start,
            FechaFin = fechaFin.Date,
            TotalPeriodo = compras.Sum(x => x.Total),
            Movimientos = compras.Select(x => new ReporteMovimientoDto
            {
                Fecha = x.Fecha,
                NumeroDocumento = x.NumeroFactura,
                EntidadNombre = x.Proveedor?.Nombre ?? string.Empty,
                Subtotal = x.Subtotal,
                Impuesto = x.Impuesto,
                Descuento = 0,
                Total = x.Total
            }).ToList(),
            TotalesPorDia = compras
                .GroupBy(x => x.Fecha.Date)
                .Select(group => new TotalPorDiaDto
                {
                    Fecha = group.Key,
                    Total = group.Sum(x => x.Total)
                })
                .OrderBy(x => x.Fecha)
                .ToList()
        };
    }
}
