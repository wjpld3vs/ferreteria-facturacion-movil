namespace FerreteriaInventario.Maui.Models;

public class CompraDetalleCreateModel
{
    public int ProductoId { get; set; }
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
}

public class CompraCreateModel
{
    public int ProveedorId { get; set; }
    public int UsuarioId { get; set; }
    public string NumeroFactura { get; set; } = string.Empty;
    public decimal Impuesto { get; set; }
    public string? Observaciones { get; set; }
    public List<CompraDetalleCreateModel> Detalles { get; set; } = new();
}

public class CompraDetalleModel
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}

public class CompraModel
{
    public int Id { get; set; }
    public int ProveedorId { get; set; }
    public string ProveedorNombre { get; set; } = string.Empty;
    public int UsuarioId { get; set; }
    public string UsuarioNombre { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public string NumeroFactura { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal Impuesto { get; set; }
    public decimal Total { get; set; }
    public string? Observaciones { get; set; }
    public List<CompraDetalleModel> Detalles { get; set; } = new();
}

public class VentaDetalleCreateModel
{
    public int ProductoId { get; set; }
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
}

public class VentaCreateModel
{
    public int ClienteId { get; set; }
    public int UsuarioId { get; set; }
    public string NumeroComprobante { get; set; } = string.Empty;
    public decimal Impuesto { get; set; }
    public decimal Descuento { get; set; }
    public string? Observaciones { get; set; }
    public List<VentaDetalleCreateModel> Detalles { get; set; } = new();
}

public class VentaDetalleModel
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}

public class VentaModel
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public int UsuarioId { get; set; }
    public string UsuarioNombre { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public string NumeroComprobante { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal Impuesto { get; set; }
    public decimal Descuento { get; set; }
    public decimal Total { get; set; }
    public string? Observaciones { get; set; }
    public List<VentaDetalleModel> Detalles { get; set; } = new();
}

public class LineaTransaccionModel
{
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal => Cantidad * PrecioUnitario;
}
