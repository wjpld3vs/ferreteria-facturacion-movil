using FerreteriaInventario.Maui.ViewModels;

namespace FerreteriaInventario.Maui.Views;

public partial class VentaDetallePage : ContentPage
{
    private readonly VentaDetalleViewModel _viewModel;

    public VentaDetallePage(VentaDetalleViewModel viewModel)
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
