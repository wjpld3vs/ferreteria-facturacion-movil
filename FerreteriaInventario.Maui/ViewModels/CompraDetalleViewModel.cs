using System.Collections.ObjectModel;
using System.Windows.Input;
using FerreteriaInventario.Maui.Helpers;
using FerreteriaInventario.Maui.Models;
using FerreteriaInventario.Maui.Services;

namespace FerreteriaInventario.Maui.ViewModels;

public class CompraDetalleViewModel : BaseViewModel
{
    private readonly CompraService _compraService;
    private readonly ProductoService _productoService;
    private readonly ProveedorService _proveedorService;
    private readonly AppSessionService _sessionService;
    private ProveedorModel? _selectedProveedor;
    private ProductoModel? _selectedProducto;
    private string _numeroFactura = $"FC-{DateTime.Now:yyyyMMddHHmm}";
    private decimal _cantidad = 1;
    private decimal _precioUnitario;
    private decimal _impuesto;
    private string _observaciones = string.Empty;

    public CompraDetalleViewModel(
        CompraService compraService,
        ProductoService productoService,
        ProveedorService proveedorService,
        AppSessionService sessionService)
    {
        _compraService = compraService;
        _productoService = productoService;
        _proveedorService = proveedorService;
        _sessionService = sessionService;
        Title = "Registrar compra";
        Proveedores = new ObservableCollection<ProveedorModel>();
        Productos = new ObservableCollection<ProductoModel>();
        Lineas = new ObservableCollection<LineaTransaccionModel>();
        AddLineaCommand = new Command(AddLinea);
        SaveCommand = new Command(async () => await SaveAsync());
    }

    public ObservableCollection<ProveedorModel> Proveedores { get; }
    public ObservableCollection<ProductoModel> Productos { get; }
    public ObservableCollection<LineaTransaccionModel> Lineas { get; }

    public ProveedorModel? SelectedProveedor
    {
        get => _selectedProveedor;
        set => SetProperty(ref _selectedProveedor, value);
    }

    public ProductoModel? SelectedProducto
    {
        get => _selectedProducto;
        set
        {
            if (SetProperty(ref _selectedProducto, value) && value is not null)
            {
                PrecioUnitario = value.PrecioCompra;
            }
        }
    }

    public string NumeroFactura { get => _numeroFactura; set => SetProperty(ref _numeroFactura, value); }
    public decimal Cantidad { get => _cantidad; set => SetProperty(ref _cantidad, value); }
    public decimal PrecioUnitario { get => _precioUnitario; set => SetProperty(ref _precioUnitario, value); }
    public decimal Impuesto { get => _impuesto; set { if (SetProperty(ref _impuesto, value)) Recalculate(); } }
    public string Observaciones { get => _observaciones; set => SetProperty(ref _observaciones, value); }
    public decimal Subtotal => Lineas.Sum(x => x.Subtotal);
    public decimal Total => Subtotal + Impuesto;

    public ICommand AddLineaCommand { get; }
    public ICommand SaveCommand { get; }

    public async Task LoadAsync()
    {
        await RunSafeAsync(async () =>
        {
            var proveedores = await _proveedorService.GetAllAsync();
            var productos = await _productoService.GetAllAsync();

            Proveedores.Clear();
            foreach (var proveedor in proveedores.Where(x => x.Activo))
            {
                Proveedores.Add(proveedor);
            }

            Productos.Clear();
            foreach (var producto in productos.Where(x => x.Activo))
            {
                Productos.Add(producto);
            }
        });
    }

    private void AddLinea()
    {
        if (SelectedProducto is null || Cantidad <= 0 || PrecioUnitario < 0)
        {
            return;
        }

        Lineas.Add(new LineaTransaccionModel
        {
            ProductoId = SelectedProducto.Id,
            ProductoNombre = SelectedProducto.Nombre,
            Cantidad = Cantidad,
            PrecioUnitario = PrecioUnitario
        });

        Cantidad = 1;
        PrecioUnitario = SelectedProducto.PrecioCompra;
        Recalculate();
    }

    private async Task SaveAsync()
    {
        await RunSafeAsync(async () =>
        {
            if (SelectedProveedor is null)
            {
                throw new InvalidOperationException("Selecciona un proveedor.");
            }

            if (Lineas.Count == 0)
            {
                throw new InvalidOperationException("Agrega al menos un detalle de compra.");
            }

            if (!await ConfirmAsync("Confirmar compra", "Se registrara la compra y se actualizara el stock."))
            {
                return;
            }

            await _compraService.CreateAsync(new CompraCreateModel
            {
                ProveedorId = SelectedProveedor.Id,
                UsuarioId = _sessionService.CurrentUser?.Id ?? 0,
                NumeroFactura = NumeroFactura.Trim(),
                Impuesto = Impuesto,
                Observaciones = Observaciones,
                Detalles = Lineas.Select(x => new CompraDetalleCreateModel
                {
                    ProductoId = x.ProductoId,
                    Cantidad = x.Cantidad,
                    PrecioUnitario = x.PrecioUnitario
                }).ToList()
            });

            await Shell.Current.GoToAsync("..");
        }, "Compra registrada correctamente.");
    }

    private void Recalculate()
    {
        OnPropertyChanged(nameof(Subtotal));
        OnPropertyChanged(nameof(Total));
    }
}
