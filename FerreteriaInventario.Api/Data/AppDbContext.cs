using FerreteriaInventario.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FerreteriaInventario.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<Compra> Compras => Set<Compra>();
    public DbSet<DetalleCompra> DetallesCompra => Set<DetalleCompra>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<DetalleVenta> DetallesVenta => Set<DetalleVenta>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Nombre).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Descripcion).HasMaxLength(250);
            entity.HasIndex(x => x.Nombre).IsUnique();
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Nombre).HasMaxLength(120).IsRequired();
            entity.Property(x => x.NombreUsuario).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(120).IsRequired();
            entity.Property(x => x.PasswordHash).IsRequired();
            entity.HasIndex(x => x.NombreUsuario).IsUnique();
            entity.HasIndex(x => x.Email).IsUnique();

            entity.HasOne(x => x.Rol)
                .WithMany(x => x.Usuarios)
                .HasForeignKey(x => x.RolId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Codigo).HasMaxLength(40).IsRequired();
            entity.Property(x => x.Nombre).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Descripcion).HasMaxLength(500);
            entity.Property(x => x.Categoria).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Marca).HasMaxLength(80).IsRequired();
            entity.Property(x => x.UnidadMedida).HasMaxLength(40).IsRequired();
            entity.Property(x => x.PrecioCompra).HasPrecision(18, 2);
            entity.Property(x => x.PrecioVenta).HasPrecision(18, 2);
            entity.HasIndex(x => x.Codigo).IsUnique();
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Nombre).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Documento).HasMaxLength(40).IsRequired();
            entity.Property(x => x.Telefono).HasMaxLength(30);
            entity.Property(x => x.Email).HasMaxLength(120);
            entity.Property(x => x.Direccion).HasMaxLength(250);
            entity.HasIndex(x => x.Documento).IsUnique();
        });

        modelBuilder.Entity<Proveedor>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Nombre).HasMaxLength(120).IsRequired();
            entity.Property(x => x.DocumentoFiscal).HasMaxLength(40).IsRequired();
            entity.Property(x => x.Telefono).HasMaxLength(30);
            entity.Property(x => x.Email).HasMaxLength(120);
            entity.Property(x => x.Direccion).HasMaxLength(250);
            entity.HasIndex(x => x.DocumentoFiscal).IsUnique();
        });

        modelBuilder.Entity<Compra>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.NumeroFactura).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Subtotal).HasPrecision(18, 2);
            entity.Property(x => x.Impuesto).HasPrecision(18, 2);
            entity.Property(x => x.Total).HasPrecision(18, 2);
            entity.Property(x => x.Observaciones).HasMaxLength(400);

            entity.HasOne(x => x.Proveedor)
                .WithMany(x => x.Compras)
                .HasForeignKey(x => x.ProveedorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Usuario)
                .WithMany(x => x.Compras)
                .HasForeignKey(x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DetalleCompra>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PrecioUnitario).HasPrecision(18, 2);
            entity.Property(x => x.Subtotal).HasPrecision(18, 2);

            entity.HasOne(x => x.Compra)
                .WithMany(x => x.Detalles)
                .HasForeignKey(x => x.CompraId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Producto)
                .WithMany(x => x.DetallesCompra)
                .HasForeignKey(x => x.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Venta>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.NumeroComprobante).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Subtotal).HasPrecision(18, 2);
            entity.Property(x => x.Impuesto).HasPrecision(18, 2);
            entity.Property(x => x.Descuento).HasPrecision(18, 2);
            entity.Property(x => x.Total).HasPrecision(18, 2);
            entity.Property(x => x.Observaciones).HasMaxLength(400);

            entity.HasOne(x => x.Cliente)
                .WithMany(x => x.Ventas)
                .HasForeignKey(x => x.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Usuario)
                .WithMany(x => x.Ventas)
                .HasForeignKey(x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DetalleVenta>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PrecioUnitario).HasPrecision(18, 2);
            entity.Property(x => x.Subtotal).HasPrecision(18, 2);

            entity.HasOne(x => x.Venta)
                .WithMany(x => x.Detalles)
                .HasForeignKey(x => x.VentaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Producto)
                .WithMany(x => x.DetallesVenta)
                .HasForeignKey(x => x.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
