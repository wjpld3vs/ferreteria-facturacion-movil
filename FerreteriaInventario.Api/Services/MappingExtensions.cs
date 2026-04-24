using FerreteriaInventario.Api.DTOs;
using FerreteriaInventario.Api.Models;

namespace FerreteriaInventario.Api.Services;

public static class MappingExtensions
{
    public static UsuarioResponseDto ToDto(this Usuario entity) => new()
    {
        Id = entity.Id,
        Nombre = entity.Nombre,
        NombreUsuario = entity.NombreUsuario,
        Email = entity.Email,
        RolId = entity.RolId,
        RolNombre = entity.Rol?.Nombre ?? string.Empty,
        Activo = entity.Activo,
        FechaCreacion = entity.FechaCreacion
    };

    public static RolResponseDto ToDto(this Rol entity) => new()
    {
        Id = entity.Id,
        Nombre = entity.Nombre,
        Descripcion = entity.Descripcion
    };

    public static ProductoResponseDto ToDto(this Producto entity) => new()
    {
        Id = entity.Id,
        Codigo = entity.Codigo,
        Nombre = entity.Nombre,
        Descripcion = entity.Descripcion,
        Categoria = entity.Categoria,
        Marca = entity.Marca,
        UnidadMedida = entity.UnidadMedida,
        PrecioCompra = entity.PrecioCompra,
        PrecioVenta = entity.PrecioVenta,
        StockActual = entity.StockActual,
        StockMinimo = entity.StockMinimo,
        Activo = entity.Activo,
        FechaCreacion = entity.FechaCreacion
    };

    public static ClienteResponseDto ToDto(this Cliente entity) => new()
    {
        Id = entity.Id,
        Nombre = entity.Nombre,
        Documento = entity.Documento,
        Telefono = entity.Telefono,
        Email = entity.Email,
        Direccion = entity.Direccion,
        Activo = entity.Activo,
        FechaCreacion = entity.FechaCreacion
    };

    public static ProveedorResponseDto ToDto(this Proveedor entity) => new()
    {
        Id = entity.Id,
        Nombre = entity.Nombre,
        DocumentoFiscal = entity.DocumentoFiscal,
        Telefono = entity.Telefono,
        Email = entity.Email,
        Direccion = entity.Direccion,
        Activo = entity.Activo,
        FechaCreacion = entity.FechaCreacion
    };

    public static CompraResponseDto ToDto(this Compra entity) => new()
    {
        Id = entity.Id,
        ProveedorId = entity.ProveedorId,
        ProveedorNombre = entity.Proveedor?.Nombre ?? string.Empty,
        UsuarioId = entity.UsuarioId,
        UsuarioNombre = entity.Usuario?.NombreUsuario ?? string.Empty,
        Fecha = entity.Fecha,
        NumeroFactura = entity.NumeroFactura,
        Subtotal = entity.Subtotal,
        Impuesto = entity.Impuesto,
        Total = entity.Total,
        Observaciones = entity.Observaciones,
        Detalles = entity.Detalles.Select(detail => new CompraDetalleResponseDto
        {
            Id = detail.Id,
            ProductoId = detail.ProductoId,
            ProductoNombre = detail.Producto?.Nombre ?? string.Empty,
            Cantidad = detail.Cantidad,
            PrecioUnitario = detail.PrecioUnitario,
            Subtotal = detail.Subtotal
        }).ToList()
    };

    public static VentaResponseDto ToDto(this Venta entity) => new()
    {
        Id = entity.Id,
        ClienteId = entity.ClienteId,
        ClienteNombre = entity.Cliente?.Nombre ?? string.Empty,
        UsuarioId = entity.UsuarioId,
        UsuarioNombre = entity.Usuario?.NombreUsuario ?? string.Empty,
        Fecha = entity.Fecha,
        NumeroComprobante = entity.NumeroComprobante,
        Subtotal = entity.Subtotal,
        Impuesto = entity.Impuesto,
        Descuento = entity.Descuento,
        Total = entity.Total,
        Observaciones = entity.Observaciones,
        Detalles = entity.Detalles.Select(detail => new VentaDetalleResponseDto
        {
            Id = detail.Id,
            ProductoId = detail.ProductoId,
            ProductoNombre = detail.Producto?.Nombre ?? string.Empty,
            Cantidad = detail.Cantidad,
            PrecioUnitario = detail.PrecioUnitario,
            Subtotal = detail.Subtotal
        }).ToList()
    };
}
