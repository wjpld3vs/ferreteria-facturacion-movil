using FerreteriaInventario.Maui.ViewModels;

namespace FerreteriaInventario.Maui.Views;

public partial class ComprasPage : ContentPage
{
    private readonly ComprasViewModel _viewModel;

    public ComprasPage(ComprasViewModel viewModel)
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
