namespace FerreteriaInventario.Api.Models;

public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int RolId { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public Rol? Rol { get; set; }
    public ICollection<Compra> Compras { get; set; } = new List<Compra>();
    public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
}
