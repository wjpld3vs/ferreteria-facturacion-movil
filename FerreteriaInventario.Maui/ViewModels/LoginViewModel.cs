using System.Windows.Input;
using FerreteriaInventario.Maui.Helpers;
using FerreteriaInventario.Maui.Models;
using FerreteriaInventario.Maui.Services;

namespace FerreteriaInventario.Maui.ViewModels;

public class LoginViewModel : BaseViewModel
{
    private readonly AuthService _authService;
    private string _usuarioOEmail = "admin";
    private string _password = "Admin123*";

    public LoginViewModel(AuthService authService)
    {
        _authService = authService;
        Title = "Iniciar sesion";
        LoginCommand = new Command(async () => await LoginAsync(), () => !IsBusy);
    }

    public string UsuarioOEmail
    {
        get => _usuarioOEmail;
        set => SetProperty(ref _usuarioOEmail, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public ICommand LoginCommand { get; }

    private async Task LoginAsync()
    {
        await RunSafeAsync(async () =>
        {
            if (string.IsNullOrWhiteSpace(UsuarioOEmail) || string.IsNullOrWhiteSpace(Password))
            {
                throw new InvalidOperationException("Debes ingresar usuario y contrasena.");
            }

            await _authService.LoginAsync(new LoginRequestModel
            {
                UsuarioOEmail = UsuarioOEmail.Trim(),
                Password = Password
            });

            await Shell.Current.GoToAsync("//dashboard");
        });
    }
}
