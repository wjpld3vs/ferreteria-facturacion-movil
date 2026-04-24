using System.Collections.ObjectModel;
using System.Windows.Input;
using FerreteriaInventario.Maui.Helpers;
using FerreteriaInventario.Maui.Models;
using FerreteriaInventario.Maui.Services;

namespace FerreteriaInventario.Maui.ViewModels;

public class ReportesViewModel : BaseViewModel
{
    private readonly ReporteService _reporteService;
    private DateTime _fechaInicio = DateTime.Today.AddDays(-7);
    private DateTime _fechaFin = DateTime.Today;
    private decimal _totalVentasPeriodo;
    private decimal _totalComprasPeriodo;
    private decimal _valorInventario;

    public ReportesViewModel(ReporteService reporteService)
    {
        _reporteService = reporteService;
        Title = "Reportes";
        Inventario = new ObservableCollection<ReporteInventarioModel>();
        StockBajo = new ObservableCollection<StockBajoModel>();
        VentasPeriodo = new ObservableCollection<ReporteMovimientoModel>();
        ComprasPeriodo = new ObservableCollection<ReporteMovimientoModel>();
        LoadCommand = new Command(async () => await LoadAsync());
    }

    public ObservableCollection<ReporteInventarioModel> Inventario { get; }
    public ObservableCollection<StockBajoModel> StockBajo { get; }
    public ObservableCollection<ReporteMovimientoModel> VentasPeriodo { get; }
    public ObservableCollection<ReporteMovimientoModel> ComprasPeriodo { get; }

    public DateTime FechaInicio { get => _fechaInicio; set => SetProperty(ref _fechaInicio, value); }
    public DateTime FechaFin { get => _fechaFin; set => SetProperty(ref _fechaFin, value); }
    public decimal TotalVentasPeriodo { get => _totalVentasPeriodo; set => SetProperty(ref _totalVentasPeriodo, value); }
    public decimal TotalComprasPeriodo { get => _totalComprasPeriodo; set => SetProperty(ref _totalComprasPeriodo, value); }
    public decimal ValorInventario { get => _valorInventario; set => SetProperty(ref _valorInventario, value); }

    public ICommand LoadCommand { get; }

    public async Task LoadAsync()
    {
        await RunSafeAsync(async () =>
        {
            var inventario = await _reporteService.GetInventarioAsync();
            var stock = await _reporteService.GetStockBajoAsync();
            var ventas = await _reporteService.GetVentasPorFechaAsync(FechaInicio, FechaFin);
            var compras = await _reporteService.GetComprasPorFechaAsync(FechaInicio, FechaFin);

            Inventario.Clear();
            foreach (var item in inventario)
            {
                Inventario.Add(item);
            }

            StockBajo.Clear();
            foreach (var item in stock)
            {
                StockBajo.Add(item);
            }

            VentasPeriodo.Clear();
            foreach (var item in ventas.Movimientos)
            {
                VentasPeriodo.Add(item);
            }

            ComprasPeriodo.Clear();
            foreach (var item in compras.Movimientos)
            {
                ComprasPeriodo.Add(item);
            }

            TotalVentasPeriodo = ventas.TotalPeriodo;
            TotalComprasPeriodo = compras.TotalPeriodo;
            ValorInventario = inventario.Sum(x => x.ValorInventario);
        });
    }
}
