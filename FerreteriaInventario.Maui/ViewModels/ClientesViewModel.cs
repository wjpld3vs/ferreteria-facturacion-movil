using System.Collections.ObjectModel;
using System.Windows.Input;
using FerreteriaInventario.Maui.Helpers;
using FerreteriaInventario.Maui.Models;
using FerreteriaInventario.Maui.Services;

namespace FerreteriaInventario.Maui.ViewModels;

public class ClientesViewModel : BaseViewModel
{
    private readonly ClienteService _clienteService;
    private string _nombre = string.Empty;
    private string _documento = string.Empty;
    private string _telefono = string.Empty;
    private string _email = string.Empty;
    private string _direccion = string.Empty;

    public ClientesViewModel(ClienteService clienteService)
    {
        _clienteService = clienteService;
        Title = "Clientes";
        Clientes = new ObservableCollection<ClienteModel>();
        CreateCommand = new Command(async () => await CreateAsync());
    }

    public ObservableCollection<ClienteModel> Clientes { get; }
    public string Nombre { get => _nombre; set => SetProperty(ref _nombre, value); }
    public string Documento { get => _documento; set => SetProperty(ref _documento, value); }
    public string Telefono { get => _telefono; set => SetProperty(ref _telefono, value); }
    public string Email { get => _email; set => SetProperty(ref _email, value); }
    public string Direccion { get => _direccion; set => SetProperty(ref _direccion, value); }
    public ICommand CreateCommand { get; }

    public async Task LoadAsync()
    {
        await RunSafeAsync(async () =>
        {
            var items = await _clienteService.GetAllAsync();
            Clientes.Clear();
            foreach (var item in items)
            {
                Clientes.Add(item);
            }
        });
    }

    private async Task CreateAsync()
    {
        await RunSafeAsync(async () =>
        {
            await _clienteService.CreateAsync(new ClienteModel
            {
                Nombre = Nombre.Trim(),
                Documento = Documento.Trim(),
                Telefono = Telefono.Trim(),
                Email = Email.Trim(),
                Direccion = Direccion.Trim(),
                Activo = true
            });

            Nombre = string.Empty;
            Documento = string.Empty;
            Telefono = string.Empty;
            Email = string.Empty;
            Direccion = string.Empty;
            await LoadAsync();
        }, "Cliente registrado correctamente.");
    }
}
