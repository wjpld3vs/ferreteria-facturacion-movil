using System.ComponentModel.DataAnnotations;

namespace FerreteriaInventario.Api.DTOs;

public class VentaDetalleCreateDto
{
    [Range(1, int.MaxValue)]
    public int ProductoId { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal Cantidad { get; set; }

    [Range(0, double.MaxValue)]
    public decimal PrecioUnitario { get; set; }
}

public class VentaCreateDto
{
    [Range(1, int.MaxValue)]
    public int ClienteId { get; set; }

    [Range(1, int.MaxValue)]
    public int UsuarioId { get; set; }

    [Required]
    [StringLength(50)]
    public string NumeroComprobante { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal Impuesto { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Descuento { get; set; }

    [StringLength(400)]
    public string? Observaciones { get; set; }

    [MinLength(1)]
    public List<VentaDetalleCreateDto> Detalles { get; set; } = new();
}

public class VentaDetalleResponseDto
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}

public class VentaResponseDto
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
    public List<VentaDetalleResponseDto> Detalles { get; set; } = new();
}
