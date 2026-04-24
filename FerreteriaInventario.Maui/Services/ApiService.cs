using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FerreteriaInventario.Maui.Models;

namespace FerreteriaInventario.Maui.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly AppSessionService _sessionService;
    private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public ApiService(HttpClient httpClient, AppSessionService sessionService)
    {
        _httpClient = httpClient;
        _sessionService = sessionService;
    }

    public Task<T?> GetAsync<T>(string endpoint, bool requiresAuth = true) =>
        SendAsync<T>(new HttpRequestMessage(HttpMethod.Get, endpoint), requiresAuth);

    public Task<T?> PostAsync<TRequest, T>(string endpoint, TRequest request, bool requiresAuth = true) =>
        SendAsync<T>(BuildRequest(HttpMethod.Post, endpoint, request), requiresAuth);

    public Task<T?> PutAsync<TRequest, T>(string endpoint, TRequest request, bool requiresAuth = true) =>
        SendAsync<T>(BuildRequest(HttpMethod.Put, endpoint, request), requiresAuth);

    public Task<T?> PatchAsync<T>(string endpoint, bool requiresAuth = true) =>
        SendAsync<T>(new HttpRequestMessage(HttpMethod.Patch, endpoint), requiresAuth);

    private HttpRequestMessage BuildRequest<TRequest>(HttpMethod method, string endpoint, TRequest request)
    {
        var json = JsonSerializer.Serialize(request, _serializerOptions);
        return new HttpRequestMessage(method, endpoint)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
    }

    private async Task<T?> SendAsync<T>(HttpRequestMessage request, bool requiresAuth)
    {
        if (requiresAuth && !string.IsNullOrWhiteSpace(_sessionService.Token))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _sessionService.Token);
        }

        try
        {
            var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
            {
                await _sessionService.LogoutAsync();
                throw new InvalidOperationException("La sesion expiro o no tienes permisos para realizar esta accion.");
            }

            if (!response.IsSuccessStatusCode)
            {
                var apiError = await response.Content.ReadFromJsonAsync<ApiErrorModel>(_serializerOptions);
                throw new InvalidOperationException(apiError?.Message ?? $"La API devolvio el estado {(int)response.StatusCode}.");
            }

            if (response.Content.Headers.ContentLength == 0)
            {
                return default;
            }

            return await response.Content.ReadFromJsonAsync<T>(_serializerOptions);
        }
        catch (HttpRequestException)
        {
            throw new InvalidOperationException("No se pudo conectar con la API. Verifica la URL base y que el backend este en ejecucion.");
        }
    }
}
