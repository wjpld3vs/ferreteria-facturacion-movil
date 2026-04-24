using FerreteriaInventario.Maui.Services;
using FerreteriaInventario.Maui.Views;

namespace FerreteriaInventario.Maui;

public partial class AppShell : Shell
{
    private readonly IServiceProvider _serviceProvider;
    private readonly AppSessionService _sessionService;
    private readonly AuthService _authService;
    private bool _isInitialized;

    public AppShell(IServiceProvider serviceProvider, AppSessionService sessionService, AuthService authService)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
        _sessionService = sessionService;
        _authService = authService;

        Routing.RegisterRoute(nameof(ProductoDetallePage), typeof(ProductoDetallePage));
        Routing.RegisterRoute(nameof(CompraDetallePage), typeof(CompraDetallePage));
        Routing.RegisterRoute(nameof(VentaDetallePage), typeof(VentaDetallePage));

        _sessionService.SessionChanged += (_, _) => MainThread.BeginInvokeOnMainThread(BuildMenu);
        BuildMenu();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_isInitialized)
        {
            return;
        }

        _isInitialized = true;
        await _sessionService.RestoreSessionAsync(_authService);
        BuildMenu();

        if (_sessionService.IsAuthenticated)
        {
            await GoToAsync("//dashboard");
        }
        else
        {
            await GoToAsync("//login");
        }
    }

    private void BuildMenu()
    {
        Items.Clear();

        Items.Add(new ShellContent
        {
            Route = "login",
            ContentTemplate = new DataTemplate(() => _serviceProvider.GetRequiredService<LoginPage>())
        });

        if (!_sessionService.IsAuthenticated)
        {
            FlyoutBehavior = FlyoutBehavior.Disabled;
            return;
        }

        FlyoutBehavior = FlyoutBehavior.Flyout;

        Items.Add(CreateMenuItem("Dashboard", "dashboard", () => _serviceProvider.GetRequiredService<DashboardPage>()));
        Items.Add(CreateMenuItem("Productos", "productos", () => _serviceProvider.GetRequiredService<ProductosPage>()));
        Items.Add(CreateMenuItem("Clientes", "clientes", () => _serviceProvider.GetRequiredService<ClientesPage>()));
        Items.Add(CreateMenuItem("Ventas", "ventas", () => _serviceProvider.GetRequiredService<VentasPage>()));

        if (_sessionService.IsAdmin)
        {
            Items.Add(CreateMenuItem("Proveedores", "proveedores", () => _serviceProvider.GetRequiredService<ProveedoresPage>()));
            Items.Add(CreateMenuItem("Compras", "compras", () => _serviceProvider.GetRequiredService<ComprasPage>()));
            Items.Add(CreateMenuItem("Usuarios", "usuarios", () => _serviceProvider.GetRequiredService<UsuariosPage>()));
            Items.Add(CreateMenuItem("Reportes", "reportes", () => _serviceProvider.GetRequiredService<ReportesPage>()));
        }
    }

    private static ShellContent CreateMenuItem(string title, string route, Func<Page> factory)
    {
        return new ShellContent
        {
            Title = title,
            Route = route,
            ContentTemplate = new DataTemplate(factory)
        };
    }
}
