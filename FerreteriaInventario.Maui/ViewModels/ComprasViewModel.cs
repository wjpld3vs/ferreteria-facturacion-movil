using System.Collections.ObjectModel;
using System.Windows.Input;
using FerreteriaInventario.Maui.Helpers;
using FerreteriaInventario.Maui.Models;
using FerreteriaInventario.Maui.Services;
using FerreteriaInventario.Maui.Views;

namespace FerreteriaInventario.Maui.ViewModels;

public class ComprasViewModel : BaseViewModel
{
    private readonly CompraService _compraService;

    public ComprasViewModel(CompraService compraService)
    {
        _compraService = compraService;
        Title = "Compras";
        Compras = new ObservableCollection<CompraModel>();
        NewCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(CompraDetallePage)));
    }

    public ObservableCollection<CompraModel> Compras { get; }
    public ICommand NewCommand { get; }

    public async Task LoadAsync()
    {
        await RunSafeAsync(async () =>
        {
            var items = await _compraService.GetAllAsync();
            Compras.Clear();
            foreach (var item in items)
            {
                Compras.Add(item);
            }
        });
    }
}
