using System.Net;

namespace FerreteriaInventario.Api.Helpers;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ApiException ex)
        {
            _logger.LogWarning(ex, "Error de negocio controlado");
            await WriteResponseAsync(context, ex.StatusCode, ex.Message, ex.Details);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no controlado");
            await WriteResponseAsync(
                context,
                (int)HttpStatusCode.InternalServerError,
                "Ocurrio un error interno en el servidor.",
                context.RequestServices.GetRequiredService<IHostEnvironment>().IsDevelopment() ? ex.Message : null);
        }
    }

    private static async Task WriteResponseAsync(HttpContext context, int statusCode, string message, string? details)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(new ErrorResponse
        {
            Message = message,
            Details = details,
            StatusCode = statusCode
        });
    }
}
