using System.ComponentModel.DataAnnotations;

namespace FerreteriaInventario.Api.DTOs;

public class CompraDetalleCreateDto
{
    [Range(1, int.MaxValue)]
    public int ProductoId { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal Cantidad { get; set; }

    [Range(0, double.MaxValue)]
    public decimal PrecioUnitario { get; set; }
}

public class CompraCreateDto
{
    [Range(1, int.MaxValue)]
    public int ProveedorId { get; set; }

    [Range(1, int.MaxValue)]
    public int UsuarioId { get; set; }

    [Required]
    [StringLength(50)]
    public string NumeroFactura { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal Impuesto { get; set; }

    [StringLength(400)]
    public string? Observaciones { get; set; }

    [MinLength(1)]
    public List<CompraDetalleCreateDto> Detalles { get; set; } = new();
}

public class CompraDetalleResponseDto
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}

public class CompraResponseDto
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
    public List<CompraDetalleResponseDto> Detalles { get; set; } = new();
}
