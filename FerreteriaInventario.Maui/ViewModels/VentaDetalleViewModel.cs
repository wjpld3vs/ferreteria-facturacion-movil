using System.Collections.ObjectModel;
using System.Windows.Input;
using FerreteriaInventario.Maui.Helpers;
using FerreteriaInventario.Maui.Models;
using FerreteriaInventario.Maui.Services;

namespace FerreteriaInventario.Maui.ViewModels;

public class VentaDetalleViewModel : BaseViewModel
{
    private readonly VentaService _ventaService;
    private readonly ProductoService _productoService;
    private readonly ClienteService _clienteService;
    private readonly AppSessionService _sessionService;
    private ClienteModel? _selectedCliente;
    private ProductoModel? _selectedProducto;
    private string _numeroComprobante = $"VT-{DateTime.Now:yyyyMMddHHmm}";
    private decimal _cantidad = 1;
    private decimal _precioUnitario;
    private decimal _impuesto;
    private decimal _descuento;
    private string _observaciones = string.Empty;

    public VentaDetalleViewModel(
        VentaService ventaService,
        ProductoService productoService,
        ClienteService clienteService,
        AppSessionService sessionService)
    {
        _ventaService = ventaService;
        _productoService = productoService;
        _clienteService = clienteService;
        _sessionService = sessionService;
        Title = "Registrar venta";
        Clientes = new ObservableCollection<ClienteModel>();
        Productos = new ObservableCollection<ProductoModel>();
        Lineas = new ObservableCollection<LineaTransaccionModel>();
        AddLineaCommand = new Command(AddLinea);
        SaveCommand = new Command(async () => await SaveAsync());
    }

    public ObservableCollection<ClienteModel> Clientes { get; }
    public ObservableCollection<ProductoModel> Productos { get; }
    public ObservableCollection<LineaTransaccionModel> Lineas { get; }

    public ClienteModel? SelectedCliente
    {
        get => _selectedCliente;
        set => SetProperty(ref _selectedCliente, value);
    }

    public ProductoModel? SelectedProducto
    {
        get => _selectedProducto;
        set
        {
            if (SetProperty(ref _selectedProducto, value) && value is not null)
            {
                PrecioUnitario = value.PrecioVenta;
            }
        }
    }

    public string NumeroComprobante { get => _numeroComprobante; set => SetProperty(ref _numeroComprobante, value); }
    public decimal Cantidad { get => _cantidad; set => SetProperty(ref _cantidad, value); }
    public decimal PrecioUnitario { get => _precioUnitario; set => SetProperty(ref _precioUnitario, value); }
    public decimal Impuesto { get => _impuesto; set { if (SetProperty(ref _impuesto, value)) Recalculate(); } }
    public decimal Descuento { get => _descuento; set { if (SetProperty(ref _descuento, value)) Recalculate(); } }
    public string Observaciones { get => _observaciones; set => SetProperty(ref _observaciones, value); }
    public decimal Subtotal => Lineas.Sum(x => x.Subtotal);
    public decimal Total => Subtotal + Impuesto - Descuento;

    public ICommand AddLineaCommand { get; }
    public ICommand SaveCommand { get; }

    public async Task LoadAsync()
    {
        await RunSafeAsync(async () =>
        {
            var clientes = await _clienteService.GetAllAsync();
            var productos = await _productoService.GetAllAsync();

            Clientes.Clear();
            foreach (var cliente in clientes.Where(x => x.Activo))
            {
                Clientes.Add(cliente);
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

        if (Cantidad > SelectedProducto.StockActual)
        {
            ErrorMessage = "La cantidad no puede superar el stock disponible.";
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
        PrecioUnitario = SelectedProducto.PrecioVenta;
        Recalculate();
    }

    private async Task SaveAsync()
    {
        await RunSafeAsync(async () =>
        {
            if (SelectedCliente is null)
            {
                throw new InvalidOperationException("Selecciona un cliente.");
            }

            if (Lineas.Count == 0)
            {
                throw new InvalidOperationException("Agrega al menos un detalle de venta.");
            }

            if (!await ConfirmAsync("Confirmar venta", "Se registrara la venta y se descontara stock."))
            {
                return;
            }

            await _ventaService.CreateAsync(new VentaCreateModel
            {
                ClienteId = SelectedCliente.Id,
                UsuarioId = _sessionService.CurrentUser?.Id ?? 0,
                NumeroComprobante = NumeroComprobante.Trim(),
                Impuesto = Impuesto,
                Descuento = Descuento,
                Observaciones = Observaciones,
                Detalles = Lineas.Select(x => new VentaDetalleCreateModel
                {
                    ProductoId = x.ProductoId,
                    Cantidad = x.Cantidad,
                    PrecioUnitario = x.PrecioUnitario
                }).ToList()
            });

            await Shell.Current.GoToAsync("..");
        }, "Venta registrada correctamente.");
    }

    private void Recalculate()
    {
        OnPropertyChanged(nameof(Subtotal));
        OnPropertyChanged(nameof(Total));
    }
}
