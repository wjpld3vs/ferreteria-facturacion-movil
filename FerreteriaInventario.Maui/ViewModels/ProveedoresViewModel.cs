using System.Collections.ObjectModel;
using System.Windows.Input;
using FerreteriaInventario.Maui.Helpers;
using FerreteriaInventario.Maui.Models;
using FerreteriaInventario.Maui.Services;

namespace FerreteriaInventario.Maui.ViewModels;

public class ProveedoresViewModel : BaseViewModel
{
    private readonly ProveedorService _proveedorService;
    private string _nombre = string.Empty;
    private string _documentoFiscal = string.Empty;
    private string _telefono = string.Empty;
    private string _email = string.Empty;
    private string _direccion = string.Empty;

    public ProveedoresViewModel(ProveedorService proveedorService)
    {
        _proveedorService = proveedorService;
        Title = "Proveedores";
        Proveedores = new ObservableCollection<ProveedorModel>();
        CreateCommand = new Command(async () => await CreateAsync());
    }

    public ObservableCollection<ProveedorModel> Proveedores { get; }
    public string Nombre { get => _nombre; set => SetProperty(ref _nombre, value); }
    public string DocumentoFiscal { get => _documentoFiscal; set => SetProperty(ref _documentoFiscal, value); }
    public string Telefono { get => _telefono; set => SetProperty(ref _telefono, value); }
    public string Email { get => _email; set => SetProperty(ref _email, value); }
    public string Direccion { get => _direccion; set => SetProperty(ref _direccion, value); }
    public ICommand CreateCommand { get; }

    public async Task LoadAsync()
    {
        await RunSafeAsync(async () =>
        {
            var items = await _proveedorService.GetAllAsync();
            Proveedores.Clear();
            foreach (var item in items)
            {
                Proveedores.Add(item);
            }
        });
    }

    private async Task CreateAsync()
    {
        await RunSafeAsync(async () =>
        {
            await _proveedorService.CreateAsync(new ProveedorModel
            {
                Nombre = Nombre.Trim(),
                DocumentoFiscal = DocumentoFiscal.Trim(),
                Telefono = Telefono.Trim(),
                Email = Email.Trim(),
                Direccion = Direccion.Trim(),
                Activo = true
            });

            Nombre = string.Empty;
            DocumentoFiscal = string.Empty;
            Telefono = string.Empty;
            Email = string.Empty;
            Direccion = string.Empty;
            await LoadAsync();
        }, "Proveedor registrado correctamente.");
    }
}
