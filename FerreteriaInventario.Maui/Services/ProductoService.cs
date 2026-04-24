using FerreteriaInventario.Maui.Models;

namespace FerreteriaInventario.Maui.Services;

public class ProductoService
{
    private readonly ApiService _apiService;

    public ProductoService(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<List<ProductoModel>> GetAllAsync() =>
        await _apiService.GetAsync<List<ProductoModel>>("api/productos") ?? new List<ProductoModel>();

    public async Task<List<ProductoModel>> BuscarAsync(string texto) =>
        await _apiService.GetAsync<List<ProductoModel>>($"api/productos/buscar?texto={Uri.EscapeDataString(texto)}") ?? new List<ProductoModel>();

    public async Task<ProductoModel> GetByIdAsync(int id) =>
        await _apiService.GetAsync<ProductoModel>($"api/productos/{id}") ?? throw new InvalidOperationException("Producto no encontrado.");

    public async Task<ProductoModel> SaveAsync(ProductoModel model)
    {
        if (model.Id == 0)
        {
            return await _apiService.PostAsync<ProductoModel, ProductoModel>("api/productos", model)
                   ?? throw new InvalidOperationException("No fue posible registrar el producto.");
        }

        return await _apiService.PutAsync<ProductoModel, ProductoModel>($"api/productos/{model.Id}", model)
               ?? throw new InvalidOperationException("No fue posible actualizar el producto.");
    }
}
