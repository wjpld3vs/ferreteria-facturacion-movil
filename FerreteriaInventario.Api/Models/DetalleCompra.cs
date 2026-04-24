namespace FerreteriaInventario.Api.Models;

public class DetalleCompra
{
    public int Id { get; set; }
    public int CompraId { get; set; }
    public int ProductoId { get; set; }
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }

    public Compra? Compra { get; set; }
    public Producto? Producto { get; set; }
}
