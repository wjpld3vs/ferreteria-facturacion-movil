using FerreteriaInventario.Maui.ViewModels;

namespace FerreteriaInventario.Maui.Views;

public partial class VentasPage : ContentPage
{
    private readonly VentasViewModel _viewModel;

    public VentasPage(VentasViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }
}
