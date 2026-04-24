using System.Collections.ObjectModel;
using System.Windows.Input;
using FerreteriaInventario.Maui.Helpers;
using FerreteriaInventario.Maui.Models;
using FerreteriaInventario.Maui.Services;
using FerreteriaInventario.Maui.Views;

namespace FerreteriaInventario.Maui.ViewModels;

public class VentasViewModel : BaseViewModel
{
    private readonly VentaService _ventaService;

    public VentasViewModel(VentaService ventaService)
    {
        _ventaService = ventaService;
        Title = "Ventas";
        Ventas = new ObservableCollection<VentaModel>();
        NewCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(VentaDetallePage)));
    }

    public ObservableCollection<VentaModel> Ventas { get; }
    public ICommand NewCommand { get; }

    public async Task LoadAsync()
    {
        await RunSafeAsync(async () =>
        {
            var items = await _ventaService.GetAllAsync();
            Ventas.Clear();
            foreach (var item in items)
            {
                Ventas.Add(item);
            }
        });
    }
}
