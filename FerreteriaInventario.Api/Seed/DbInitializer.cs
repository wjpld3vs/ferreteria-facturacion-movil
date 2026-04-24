using FerreteriaInventario.Api.Data;
using FerreteriaInventario.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FerreteriaInventario.Api.Seed;

public class DbInitializer
{
    private readonly AppDbContext _context;
    private readonly ILogger<DbInitializer> _logger;
    private readonly PasswordHasher<Usuario> _passwordHasher = new();

    public DbInitializer(AppDbContext context, ILogger<DbInitializer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        var hasMigrations = _context.Database.GetMigrations().Any();
        if (hasMigrations)
        {
            await _context.Database.MigrateAsync();
        }
        else
        {
            await _context.Database.EnsureCreatedAsync();
        }

        if (await _context.Usuarios.AnyAsync())
        {
            _logger.LogInformation("La base de datos ya contiene datos iniciales.");
            return;
        }

        var adminRole = new Rol { Nombre = "Admin", Descripcion = "Acceso completo a la gestion del sistema." };
        var operarioRole = new Rol { Nombre = "Operario", Descripcion = "Acceso a consulta de catalogos y registro de ventas." };

        _context.Roles.AddRange(adminRole, operarioRole);
        await _context.SaveChangesAsync();

        var admin = new Usuario
        {
            Nombre = "Administrador General",
            NombreUsuario = "admin",
            Email = "admin@ferreteria.local",
            RolId = adminRole.Id,
            Activo = true
        };
        admin.PasswordHash = _passwordHasher.HashPassword(admin, "Admin123*");

        var operario = new Usuario
        {
            Nombre = "Operario Principal",
            NombreUsuario = "operario",
            Email = "operario@ferreteria.local",
            RolId = operarioRole.Id,
            Activo = true
        };
        operario.PasswordHash = _passwordHasher.HashPassword(operario, "Operario123*");

        _context.Usuarios.AddRange(admin, operario);

        var productos = new List<Producto>
        {
            new() { Codigo = "FER-001", Nombre = "Martillo", Descripcion = "Martillo de acero con mango ergonomico", Categoria = "Herramientas", Marca = "Truper", UnidadMedida = "Unidad", PrecioCompra = 7.50m, PrecioVenta = 11.50m, StockActual = 18, StockMinimo = 5, Activo = true },
            new() { Codigo = "FER-002", Nombre = "Destornillador plano", Descripcion = "Destornillador punta plana 6 pulgadas", Categoria = "Herramientas", Marca = "Stanley", UnidadMedida = "Unidad", PrecioCompra = 2.20m, PrecioVenta = 3.80m, StockActual = 25, StockMinimo = 8, Activo = true },
            new() { Codigo = "FER-003", Nombre = "Destornillador Phillips", Descripcion = "Destornillador punta Phillips 6 pulgadas", Categoria = "Herramientas", Marca = "Stanley", UnidadMedida = "Unidad", PrecioCompra = 2.40m, PrecioVenta = 4.10m, StockActual = 24, StockMinimo = 8, Activo = true },
            new() { Codigo = "FER-004", Nombre = "Taladro electrico", Descripcion = "Taladro 650W con mandril de 13 mm", Categoria = "Herramientas electricas", Marca = "Bosch", UnidadMedida = "Unidad", PrecioCompra = 42.00m, PrecioVenta = 58.00m, StockActual = 8, StockMinimo = 2, Activo = true },
            new() { Codigo = "FER-005", Nombre = "Tornillos", Descripcion = "Tornillos galvanizados 2 pulgadas", Categoria = "Fijaciones", Marca = "FixAll", UnidadMedida = "Caja", PrecioCompra = 3.40m, PrecioVenta = 5.20m, StockActual = 40, StockMinimo = 12, Activo = true },
            new() { Codigo = "FER-006", Nombre = "Clavos", Descripcion = "Clavos de acero 2 pulgadas", Categoria = "Fijaciones", Marca = "FixAll", UnidadMedida = "Caja", PrecioCompra = 2.70m, PrecioVenta = 4.20m, StockActual = 35, StockMinimo = 10, Activo = true },
            new() { Codigo = "FER-007", Nombre = "Cemento", Descripcion = "Bolsa de cemento gris de 42.5 kg", Categoria = "Construccion", Marca = "Holcim", UnidadMedida = "Bolsa", PrecioCompra = 8.50m, PrecioVenta = 11.90m, StockActual = 22, StockMinimo = 6, Activo = true },
            new() { Codigo = "FER-008", Nombre = "Pintura blanca", Descripcion = "Cubeta de pintura blanca satinada", Categoria = "Pinturas", Marca = "Sur", UnidadMedida = "Galon", PrecioCompra = 14.00m, PrecioVenta = 19.50m, StockActual = 14, StockMinimo = 4, Activo = true },
            new() { Codigo = "FER-009", Nombre = "Brocha", Descripcion = "Brocha profesional 4 pulgadas", Categoria = "Pinturas", Marca = "Pretul", UnidadMedida = "Unidad", PrecioCompra = 1.80m, PrecioVenta = 3.10m, StockActual = 20, StockMinimo = 6, Activo = true },
            new() { Codigo = "FER-010", Nombre = "Llave inglesa", Descripcion = "Llave ajustable de 10 pulgadas", Categoria = "Herramientas", Marca = "Truper", UnidadMedida = "Unidad", PrecioCompra = 6.80m, PrecioVenta = 9.90m, StockActual = 12, StockMinimo = 4, Activo = true },
            new() { Codigo = "FER-011", Nombre = "Alicate", Descripcion = "Alicate universal 8 pulgadas", Categoria = "Herramientas", Marca = "Truper", UnidadMedida = "Unidad", PrecioCompra = 5.10m, PrecioVenta = 7.80m, StockActual = 13, StockMinimo = 4, Activo = true },
            new() { Codigo = "FER-012", Nombre = "Cinta metrica", Descripcion = "Cinta metrica de 5 metros", Categoria = "Medicion", Marca = "Stanley", UnidadMedida = "Unidad", PrecioCompra = 3.90m, PrecioVenta = 6.20m, StockActual = 15, StockMinimo = 5, Activo = true },
            new() { Codigo = "FER-013", Nombre = "Tubo PVC", Descripcion = "Tubo PVC sanitario de 2 pulgadas", Categoria = "Plomeria", Marca = "Durman", UnidadMedida = "Unidad", PrecioCompra = 4.60m, PrecioVenta = 7.10m, StockActual = 30, StockMinimo = 10, Activo = true },
            new() { Codigo = "FER-014", Nombre = "Pegamento PVC", Descripcion = "Pegamento para PVC 500 ml", Categoria = "Plomeria", Marca = "Oatey", UnidadMedida = "Unidad", PrecioCompra = 3.20m, PrecioVenta = 5.00m, StockActual = 16, StockMinimo = 5, Activo = true },
            new() { Codigo = "FER-015", Nombre = "Cable electrico", Descripcion = "Rollo de cable THHN calibre 12", Categoria = "Electricidad", Marca = "Indeco", UnidadMedida = "Rollo", PrecioCompra = 24.00m, PrecioVenta = 31.50m, StockActual = 9, StockMinimo = 3, Activo = true }
        };

        var clientes = new List<Cliente>
        {
            new() { Nombre = "Carlos Ramirez", Documento = "CL-001", Telefono = "8888-1001", Email = "carlos.ramirez@cliente.local", Direccion = "Barrio Central, Managua", Activo = true },
            new() { Nombre = "Maria Lopez", Documento = "CL-002", Telefono = "8888-1002", Email = "maria.lopez@cliente.local", Direccion = "Colonia 15 de Septiembre, Managua", Activo = true },
            new() { Nombre = "Jose Martinez", Documento = "CL-003", Telefono = "8888-1003", Email = "jose.martinez@cliente.local", Direccion = "Masaya Centro", Activo = true },
            new() { Nombre = "Ana Gutierrez", Documento = "CL-004", Telefono = "8888-1004", Email = "ana.gutierrez@cliente.local", Direccion = "Carretera a Leon", Activo = true },
            new() { Nombre = "Constructora El Progreso", Documento = "CL-005", Telefono = "8888-1005", Email = "compras@progreso.local", Direccion = "Tipitapa", Activo = true }
        };

        var proveedores = new List<Proveedor>
        {
            new() { Nombre = "Distribuidora Central", DocumentoFiscal = "PR-001", Telefono = "2222-2001", Email = "ventas@distcentral.local", Direccion = "Zona industrial Managua", Activo = true },
            new() { Nombre = "Herramientas del Norte", DocumentoFiscal = "PR-002", Telefono = "2222-2002", Email = "contacto@herrnorte.local", Direccion = "Esteli", Activo = true },
            new() { Nombre = "Pinturas y Mas", DocumentoFiscal = "PR-003", Telefono = "2222-2003", Email = "pedidos@pinturasymas.local", Direccion = "Masaya", Activo = true },
            new() { Nombre = "PVC Solutions", DocumentoFiscal = "PR-004", Telefono = "2222-2004", Email = "servicio@pvcsolutions.local", Direccion = "Ciudad Sandino", Activo = true },
            new() { Nombre = "Electro Suministros", DocumentoFiscal = "PR-005", Telefono = "2222-2005", Email = "ventas@electrosuministros.local", Direccion = "Managua", Activo = true }
        };

        _context.Productos.AddRange(productos);
        _context.Clientes.AddRange(clientes);
        _context.Proveedores.AddRange(proveedores);
        await _context.SaveChangesAsync();

        var compra1 = new Compra
        {
            ProveedorId = proveedores[0].Id,
            UsuarioId = admin.Id,
            Fecha = DateTime.UtcNow.AddDays(-10),
            NumeroFactura = "FC-1001",
            Impuesto = 4.80m,
            Observaciones = "Reposicion de herramientas manuales"
        };
        compra1.Detalles.Add(new DetalleCompra { ProductoId = productos[0].Id, Cantidad = 10, PrecioUnitario = 7.50m, Subtotal = 75.00m });
        compra1.Detalles.Add(new DetalleCompra { ProductoId = productos[1].Id, Cantidad = 12, PrecioUnitario = 2.20m, Subtotal = 26.40m });
        compra1.Detalles.Add(new DetalleCompra { ProductoId = productos[2].Id, Cantidad = 12, PrecioUnitario = 2.40m, Subtotal = 28.80m });
        compra1.Subtotal = compra1.Detalles.Sum(x => x.Subtotal);
        compra1.Total = compra1.Subtotal + compra1.Impuesto;

        var compra2 = new Compra
        {
            ProveedorId = proveedores[2].Id,
            UsuarioId = admin.Id,
            Fecha = DateTime.UtcNow.AddDays(-8),
            NumeroFactura = "FC-1002",
            Impuesto = 3.50m,
            Observaciones = "Compra de pinturas y accesorios"
        };
        compra2.Detalles.Add(new DetalleCompra { ProductoId = productos[7].Id, Cantidad = 6, PrecioUnitario = 14.00m, Subtotal = 84.00m });
        compra2.Detalles.Add(new DetalleCompra { ProductoId = productos[8].Id, Cantidad = 10, PrecioUnitario = 1.80m, Subtotal = 18.00m });
        compra2.Subtotal = compra2.Detalles.Sum(x => x.Subtotal);
        compra2.Total = compra2.Subtotal + compra2.Impuesto;

        var compra3 = new Compra
        {
            ProveedorId = proveedores[4].Id,
            UsuarioId = admin.Id,
            Fecha = DateTime.UtcNow.AddDays(-6),
            NumeroFactura = "FC-1003",
            Impuesto = 5.20m,
            Observaciones = "Ingreso de materiales electricos y PVC"
        };
        compra3.Detalles.Add(new DetalleCompra { ProductoId = productos[12].Id, Cantidad = 15, PrecioUnitario = 4.60m, Subtotal = 69.00m });
        compra3.Detalles.Add(new DetalleCompra { ProductoId = productos[13].Id, Cantidad = 8, PrecioUnitario = 3.20m, Subtotal = 25.60m });
        compra3.Detalles.Add(new DetalleCompra { ProductoId = productos[14].Id, Cantidad = 4, PrecioUnitario = 24.00m, Subtotal = 96.00m });
        compra3.Subtotal = compra3.Detalles.Sum(x => x.Subtotal);
        compra3.Total = compra3.Subtotal + compra3.Impuesto;

        _context.Compras.AddRange(compra1, compra2, compra3);

        productos[0].StockActual += 10;
        productos[1].StockActual += 12;
        productos[2].StockActual += 12;
        productos[7].StockActual += 6;
        productos[8].StockActual += 10;
        productos[12].StockActual += 15;
        productos[13].StockActual += 8;
        productos[14].StockActual += 4;

        var venta1 = new Venta
        {
            ClienteId = clientes[0].Id,
            UsuarioId = operario.Id,
            Fecha = DateTime.UtcNow.AddDays(-5),
            NumeroComprobante = "VT-2001",
            Impuesto = 1.50m,
            Descuento = 0.50m,
            Observaciones = "Venta mostrador"
        };
        venta1.Detalles.Add(new DetalleVenta { ProductoId = productos[0].Id, Cantidad = 2, PrecioUnitario = 11.50m, Subtotal = 23.00m });
        venta1.Detalles.Add(new DetalleVenta { ProductoId = productos[11].Id, Cantidad = 1, PrecioUnitario = 6.20m, Subtotal = 6.20m });
        venta1.Subtotal = venta1.Detalles.Sum(x => x.Subtotal);
        venta1.Total = venta1.Subtotal + venta1.Impuesto - venta1.Descuento;

        var venta2 = new Venta
        {
            ClienteId = clientes[4].Id,
            UsuarioId = admin.Id,
            Fecha = DateTime.UtcNow.AddDays(-3),
            NumeroComprobante = "VT-2002",
            Impuesto = 3.20m,
            Descuento = 2.00m,
            Observaciones = "Venta a cliente corporativo"
        };
        venta2.Detalles.Add(new DetalleVenta { ProductoId = productos[6].Id, Cantidad = 5, PrecioUnitario = 11.90m, Subtotal = 59.50m });
        venta2.Detalles.Add(new DetalleVenta { ProductoId = productos[12].Id, Cantidad = 4, PrecioUnitario = 7.10m, Subtotal = 28.40m });
        venta2.Subtotal = venta2.Detalles.Sum(x => x.Subtotal);
        venta2.Total = venta2.Subtotal + venta2.Impuesto - venta2.Descuento;

        var venta3 = new Venta
        {
            ClienteId = clientes[1].Id,
            UsuarioId = operario.Id,
            Fecha = DateTime.UtcNow.AddDays(-1),
            NumeroComprobante = "VT-2003",
            Impuesto = 2.10m,
            Descuento = 0.00m,
            Observaciones = "Venta diaria de herramientas"
        };
        venta3.Detalles.Add(new DetalleVenta { ProductoId = productos[3].Id, Cantidad = 1, PrecioUnitario = 58.00m, Subtotal = 58.00m });
        venta3.Detalles.Add(new DetalleVenta { ProductoId = productos[10].Id, Cantidad = 2, PrecioUnitario = 7.80m, Subtotal = 15.60m });
        venta3.Subtotal = venta3.Detalles.Sum(x => x.Subtotal);
        venta3.Total = venta3.Subtotal + venta3.Impuesto - venta3.Descuento;

        _context.Ventas.AddRange(venta1, venta2, venta3);

        productos[0].StockActual -= 2;
        productos[11].StockActual -= 1;
        productos[6].StockActual -= 5;
        productos[12].StockActual -= 4;
        productos[3].StockActual -= 1;
        productos[10].StockActual -= 2;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Datos de prueba generados correctamente.");
    }
}
