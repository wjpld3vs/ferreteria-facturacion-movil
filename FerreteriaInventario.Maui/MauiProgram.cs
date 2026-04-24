using FerreteriaInventario.Maui.Helpers;
using FerreteriaInventario.Maui.Services;
using FerreteriaInventario.Maui.ViewModels;
using FerreteriaInventario.Maui.Views;
using Microsoft.Extensions.Logging;

namespace FerreteriaInventario.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddSingleton(new HttpClient
        {
            BaseAddress = new Uri(ApiSettings.GetBaseUrl())
        });

        builder.Services.AddSingleton<TokenStorageService>();
        builder.Services.AddSingleton<AppSessionService>();
        builder.Services.AddSingleton<ApiService>();
        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<ProductoService>();
        builder.Services.AddSingleton<ClienteService>();
        builder.Services.AddSingleton<ProveedorService>();
        builder.Services.AddSingleton<CompraService>();
        builder.Services.AddSingleton<VentaService>();
        builder.Services.AddSingleton<UsuarioService>();
        builder.Services.AddSingleton<ReporteService>();

        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<DashboardViewModel>();
        builder.Services.AddTransient<ProductosViewModel>();
        builder.Services.AddTransient<ProductoDetalleViewModel>();
        builder.Services.AddTransient<ClientesViewModel>();
        builder.Services.AddTransient<ProveedoresViewModel>();
        builder.Services.AddTransient<ComprasViewModel>();
        builder.Services.AddTransient<CompraDetalleViewModel>();
        builder.Services.AddTransient<VentasViewModel>();
        builder.Services.AddTransient<VentaDetalleViewModel>();
        builder.Services.AddTransient<UsuariosViewModel>();
        builder.Services.AddTransient<ReportesViewModel>();

        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<ProductosPage>();
        builder.Services.AddTransient<ProductoDetallePage>();
        builder.Services.AddTransient<ClientesPage>();
        builder.Services.AddTransient<ProveedoresPage>();
        builder.Services.AddTransient<ComprasPage>();
        builder.Services.AddTransient<CompraDetallePage>();
        builder.Services.AddTransient<VentasPage>();
        builder.Services.AddTransient<VentaDetallePage>();
        builder.Services.AddTransient<UsuariosPage>();
        builder.Services.AddTransient<ReportesPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
