using FerreteriaInventario.Api.DTOs;
using FerreteriaInventario.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FerreteriaInventario.Api.Controllers;

[ApiController]
[Route("api/reportes")]
[Authorize(Roles = "Admin")]
public class ReportesController : ControllerBase
{
    private readonly IReporteService _service;

    public ReportesController(IReporteService service)
    {
        _service = service;
    }

    [HttpGet("inventario")]
    public async Task<ActionResult<List<ReporteInventarioDto>>> Inventario() => Ok(await _service.GetInventarioAsync());

    [HttpGet("stock-bajo")]
    public async Task<ActionResult<List<StockBajoDto>>> StockBajo() => Ok(await _service.GetStockBajoAsync());

    [HttpGet("ventas-por-fecha")]
    public async Task<ActionResult<ReporteVentasPorFechaDto>> VentasPorFecha([FromQuery] DateTime fechaInicio, [FromQuery] DateTime fechaFin)
        => Ok(await _service.GetVentasPorFechaAsync(fechaInicio, fechaFin));

    [HttpGet("compras-por-fecha")]
    public async Task<ActionResult<ReporteComprasPorFechaDto>> ComprasPorFecha([FromQuery] DateTime fechaInicio, [FromQuery] DateTime fechaFin)
        => Ok(await _service.GetComprasPorFechaAsync(fechaInicio, fechaFin));
}
