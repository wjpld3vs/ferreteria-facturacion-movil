namespace FerreteriaInventario.Maui.Models;

public class ReporteInventarioModel
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

public class StockBajoModel
{
    public int ProductoId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public decimal StockActual { get; set; }
    public decimal StockMinimo { get; set; }
}

public class ReporteMovimientoModel
{
    public DateTime Fecha { get; set; }
    public string NumeroDocumento { get; set; } = string.Empty;
    public string EntidadNombre { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal Impuesto { get; set; }
    public decimal Descuento { get; set; }
    public decimal Total { get; set; }
}

public class TotalPorDiaModel
{
    public DateTime Fecha { get; set; }
    public decimal Total { get; set; }
}

public class ProductoMasVendidoModel
{
    public int ProductoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal CantidadVendida { get; set; }
}

public class ReporteVentasPorFechaModel
{
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public decimal TotalPeriodo { get; set; }
    public List<ReporteMovimientoModel> Movimientos { get; set; } = new();
    public List<TotalPorDiaModel> TotalesPorDia { get; set; } = new();
    public List<ProductoMasVendidoModel> ProductosMasVendidos { get; set; } = new();
}

public class ReporteComprasPorFechaModel
{
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public decimal TotalPeriodo { get; set; }
    public List<ReporteMovimientoModel> Movimientos { get; set; } = new();
    public List<TotalPorDiaModel> TotalesPorDia { get; set; } = new();
}
