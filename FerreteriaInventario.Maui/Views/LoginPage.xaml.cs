using FerreteriaInventario.Maui.ViewModels;

namespace FerreteriaInventario.Maui.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
