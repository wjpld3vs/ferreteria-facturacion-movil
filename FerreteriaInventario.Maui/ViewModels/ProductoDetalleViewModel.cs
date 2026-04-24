using System.Windows.Input;
using FerreteriaInventario.Maui.Helpers;
using FerreteriaInventario.Maui.Models;
using FerreteriaInventario.Maui.Services;

namespace FerreteriaInventario.Maui.ViewModels;

public class ProductoDetalleViewModel : BaseViewModel
{
    private readonly ProductoService _productoService;
    private int _id;
    private string _codigo = string.Empty;
    private string _nombre = string.Empty;
    private string _descripcion = string.Empty;
    private string _categoria = string.Empty;
    private string _marca = string.Empty;
    private string _unidadMedida = "Unidad";
    private decimal _precioCompra;
    private decimal _precioVenta;
    private decimal _stockActual;
    private decimal _stockMinimo;
    private bool _activo = true;

    public ProductoDetalleViewModel(ProductoService productoService)
    {
        _productoService = productoService;
        Title = "Producto";
        SaveCommand = new Command(async () => await SaveAsync());
    }

    public int Id { get => _id; set => SetProperty(ref _id, value); }
    public string Codigo { get => _codigo; set => SetProperty(ref _codigo, value); }
    public string Nombre { get => _nombre; set => SetProperty(ref _nombre, value); }
    public string Descripcion { get => _descripcion; set => SetProperty(ref _descripcion, value); }
    public string Categoria { get => _categoria; set => SetProperty(ref _categoria, value); }
    public string Marca { get => _marca; set => SetProperty(ref _marca, value); }
    public string UnidadMedida { get => _unidadMedida; set => SetProperty(ref _unidadMedida, value); }
    public decimal PrecioCompra { get => _precioCompra; set => SetProperty(ref _precioCompra, value); }
    public decimal PrecioVenta { get => _precioVenta; set => SetProperty(ref _precioVenta, value); }
    public decimal StockActual { get => _stockActual; set => SetProperty(ref _stockActual, value); }
    public decimal StockMinimo { get => _stockMinimo; set => SetProperty(ref _stockMinimo, value); }
    public bool Activo { get => _activo; set => SetProperty(ref _activo, value); }

    public ICommand SaveCommand { get; }

    public async Task LoadAsync(int id)
    {
        if (id == 0)
        {
            return;
        }

        await RunSafeAsync(async () =>
        {
            var item = await _productoService.GetByIdAsync(id);
            Id = item.Id;
            Codigo = item.Codigo;
            Nombre = item.Nombre;
            Descripcion = item.Descripcion ?? string.Empty;
            Categoria = item.Categoria;
            Marca = item.Marca;
            UnidadMedida = item.UnidadMedida;
            PrecioCompra = item.PrecioCompra;
            PrecioVenta = item.PrecioVenta;
            StockActual = item.StockActual;
            StockMinimo = item.StockMinimo;
            Activo = item.Activo;
        });
    }

    private async Task SaveAsync()
    {
        await RunSafeAsync(async () =>
        {
            if (string.IsNullOrWhiteSpace(Codigo) || string.IsNullOrWhiteSpace(Nombre))
            {
                throw new InvalidOperationException("Codigo y nombre son obligatorios.");
            }

            await _productoService.SaveAsync(new ProductoModel
            {
                Id = Id,
                Codigo = Codigo.Trim(),
                Nombre = Nombre.Trim(),
                Descripcion = Descripcion?.Trim(),
                Categoria = Categoria.Trim(),
                Marca = Marca.Trim(),
                UnidadMedida = UnidadMedida.Trim(),
                PrecioCompra = PrecioCompra,
                PrecioVenta = PrecioVenta,
                StockActual = StockActual,
                StockMinimo = StockMinimo,
                Activo = Activo
            });

            await Shell.Current.GoToAsync("..");
        }, "Producto guardado correctamente.");
    }
}
