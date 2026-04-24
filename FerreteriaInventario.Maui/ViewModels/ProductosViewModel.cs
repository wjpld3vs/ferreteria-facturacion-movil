using System.Collections.ObjectModel;
using System.Windows.Input;
using FerreteriaInventario.Maui.Helpers;
using FerreteriaInventario.Maui.Models;
using FerreteriaInventario.Maui.Services;
using FerreteriaInventario.Maui.Views;

namespace FerreteriaInventario.Maui.ViewModels;

public class ProductosViewModel : BaseViewModel
{
    private readonly ProductoService _productoService;
    private readonly AppSessionService _sessionService;
    private string _searchText = string.Empty;

    public ProductosViewModel(ProductoService productoService, AppSessionService sessionService)
    {
        _productoService = productoService;
        _sessionService = sessionService;
        Title = "Productos";
        Productos = new ObservableCollection<ProductoModel>();
        SearchCommand = new Command(async () => await LoadAsync());
        NewProductCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(ProductoDetallePage)));
        OpenDetailCommand = new Command<ProductoModel>(async item =>
        {
            if (item is not null)
            {
                await Shell.Current.GoToAsync($"{nameof(ProductoDetallePage)}?id={item.Id}");
            }
        });
    }

    public ObservableCollection<ProductoModel> Productos { get; }
    public bool CanManage => _sessionService.IsAdmin;

    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    public ICommand SearchCommand { get; }
    public ICommand NewProductCommand { get; }
    public ICommand OpenDetailCommand { get; }

    public async Task LoadAsync()
    {
        await RunSafeAsync(async () =>
        {
            var items = string.IsNullOrWhiteSpace(SearchText)
                ? await _productoService.GetAllAsync()
                : await _productoService.BuscarAsync(SearchText);

            Productos.Clear();
            foreach (var item in items)
            {
                Productos.Add(item);
            }
        });
    }
}
