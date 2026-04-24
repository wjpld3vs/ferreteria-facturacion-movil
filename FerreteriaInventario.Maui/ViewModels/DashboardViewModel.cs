using System.Collections.ObjectModel;
using System.Windows.Input;
using FerreteriaInventario.Maui.Helpers;
using FerreteriaInventario.Maui.Services;

namespace FerreteriaInventario.Maui.ViewModels;

public class DashboardViewModel : BaseViewModel
{
    private readonly AppSessionService _sessionService;
    private readonly AuthService _authService;

    public DashboardViewModel(AppSessionService sessionService, AuthService authService)
    {
        _sessionService = sessionService;
        _authService = authService;
        Title = "Dashboard";
        OpenPageCommand = new Command<string>(async route => await Shell.Current.GoToAsync($"//{route}"));
        LogoutCommand = new Command(async () => await _authService.LogoutAsync());
        QuickActions = new ObservableCollection<string>();
        Refresh();
        _sessionService.SessionChanged += (_, _) => Refresh();
    }

    public string Bienvenida => _sessionService.CurrentUser is null
        ? "Bienvenido"
        : $"Bienvenido, {_sessionService.CurrentUser.Nombre}";

    public string Rol => _sessionService.CurrentUser?.RolNombre ?? string.Empty;

    public ObservableCollection<string> QuickActions { get; }

    public ICommand OpenPageCommand { get; }
    public ICommand LogoutCommand { get; }

    public void Refresh()
    {
        QuickActions.Clear();
        QuickActions.Add("productos");
        QuickActions.Add("clientes");
        QuickActions.Add("ventas");

        if (_sessionService.IsAdmin)
        {
            QuickActions.Add("compras");
            QuickActions.Add("proveedores");
            QuickActions.Add("usuarios");
            QuickActions.Add("reportes");
        }

        OnPropertyChanged(nameof(Bienvenida));
        OnPropertyChanged(nameof(Rol));
    }
}
