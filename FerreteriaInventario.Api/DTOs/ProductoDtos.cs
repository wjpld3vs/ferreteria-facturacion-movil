using System.ComponentModel.DataAnnotations;

namespace FerreteriaInventario.Api.DTOs;

public class ProductoCreateDto
{
    [Required]
    [StringLength(40)]
    public string Codigo { get; set; } = string.Empty;

    [Required]
    [StringLength(120)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Descripcion { get; set; }

    [Required]
    [StringLength(80)]
    public string Categoria { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    public string Marca { get; set; } = string.Empty;

    [Required]
    [StringLength(40)]
    public string UnidadMedida { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal PrecioCompra { get; set; }

    [Range(0, double.MaxValue)]
    public decimal PrecioVenta { get; set; }

    [Range(0, double.MaxValue)]
    public decimal StockActual { get; set; }

    [Range(0, double.MaxValue)]
    public decimal StockMinimo { get; set; }

    public bool Activo { get; set; } = true;
}

public class ProductoUpdateDto : ProductoCreateDto
{
}

public class ProductoResponseDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string UnidadMedida { get; set; } = string.Empty;
    public decimal PrecioCompra { get; set; }
    public decimal PrecioVenta { get; set; }
    public decimal StockActual { get; set; }
    public decimal StockMinimo { get; set; }
    public bool StockBajo => StockActual <= StockMinimo;
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
}
