namespace FerreteriaInventario.Api.DTOs;

public class ReporteInventarioDto
{
    public int ProductoId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public decimal StockActual { get; set; }
    public decimal StockMinimo { get; set; }
    public decimal PrecioCompra { get; set; }
    public decimal PrecioVenta { get; set; }
    public decimal ValorInventario { get; set; }
    public bool StockBajo { get; set; }
}

public class StockBajoDto
{
    public int ProductoId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public decimal StockActual { get; set; }
    public decimal StockMinimo { get; set; }
}

public class ReporteMovimientoDto
{
    public DateTime Fecha { get; set; }
    public string NumeroDocumento { get; set; } = string.Empty;
    public string EntidadNombre { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal Impuesto { get; set; }
    public decimal Descuento { get; set; }
    public decimal Total { get; set; }
}

public class TotalPorDiaDto
{
    public DateTime Fecha { get; set; }
    public decimal Total { get; set; }
}

public class ProductoMasVendidoDto
{
    public int ProductoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal CantidadVendida { get; set; }
}

public class ReporteVentasPorFechaDto
{
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public decimal TotalPeriodo { get; set; }
    public List<ReporteMovimientoDto> Movimientos { get; set; } = new();
    public List<TotalPorDiaDto> TotalesPorDia { get; set; } = new();
    public List<ProductoMasVendidoDto> ProductosMasVendidos { get; set; } = new();
}

public class ReporteComprasPorFechaDto
{
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public decimal TotalPeriodo { get; set; }
    public List<ReporteMovimientoDto> Movimientos { get; set; } = new();
    public List<TotalPorDiaDto> TotalesPorDia { get; set; } = new();
}
