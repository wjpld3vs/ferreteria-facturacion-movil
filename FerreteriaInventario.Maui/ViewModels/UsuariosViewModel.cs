using System.Collections.ObjectModel;
using System.Windows.Input;
using FerreteriaInventario.Maui.Helpers;
using FerreteriaInventario.Maui.Models;
using FerreteriaInventario.Maui.Services;

namespace FerreteriaInventario.Maui.ViewModels;

public class UsuariosViewModel : BaseViewModel
{
    private readonly UsuarioService _usuarioService;
    private string _nombre = string.Empty;
    private string _nombreUsuario = string.Empty;
    private string _email = string.Empty;
    private string _password = "Temporal123*";
    private RolModel? _selectedRol;

    public UsuariosViewModel(UsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
        Title = "Usuarios";
        Usuarios = new ObservableCollection<UsuarioModel>();
        Roles = new ObservableCollection<RolModel>();
        CreateCommand = new Command(async () => await CreateAsync());
    }

    public ObservableCollection<UsuarioModel> Usuarios { get; }
    public ObservableCollection<RolModel> Roles { get; }

    public string Nombre { get => _nombre; set => SetProperty(ref _nombre, value); }
    public string NombreUsuario { get => _nombreUsuario; set => SetProperty(ref _nombreUsuario, value); }
    public string Email { get => _email; set => SetProperty(ref _email, value); }
    public string Password { get => _password; set => SetProperty(ref _password, value); }
    public RolModel? SelectedRol { get => _selectedRol; set => SetProperty(ref _selectedRol, value); }

    public ICommand CreateCommand { get; }

    public async Task LoadAsync()
    {
        await RunSafeAsync(async () =>
        {
            var roles = await _usuarioService.GetRolesAsync();
            var usuarios = await _usuarioService.GetAllAsync();

            Roles.Clear();
            foreach (var rol in roles)
            {
                Roles.Add(rol);
            }

            Usuarios.Clear();
            foreach (var usuario in usuarios)
            {
                Usuarios.Add(usuario);
            }
        });
    }

    private async Task CreateAsync()
    {
        await RunSafeAsync(async () =>
        {
            if (SelectedRol is null)
            {
                throw new InvalidOperationException("Selecciona un rol.");
            }

            await _usuarioService.CreateAsync(new UsuarioCreateModel
            {
                Nombre = Nombre.Trim(),
                NombreUsuario = NombreUsuario.Trim(),
                Email = Email.Trim(),
                Password = Password,
                RolId = SelectedRol.Id,
                Activo = true
            });

            Nombre = string.Empty;
            NombreUsuario = string.Empty;
            Email = string.Empty;
            Password = "Temporal123*";
            await LoadAsync();
        }, "Usuario creado correctamente.");
    }
}
