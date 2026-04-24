using FerreteriaInventario.Maui.ViewModels;

namespace FerreteriaInventario.Maui.Views;

public partial class ProductoDetallePage : ContentPage, IQueryAttributable
{
    private readonly ProductoDetalleViewModel _viewModel;

    public ProductoDetallePage(ProductoDetalleViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var idValue) && int.TryParse(idValue.ToString(), out var id))
        {
            await _viewModel.LoadAsync(id);
        }
    }
}
