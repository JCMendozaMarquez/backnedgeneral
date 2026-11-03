using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BackendGeneral.Api.Services;

namespace BackendGeneral.Api.Controllers;

[ApiController]
[Route("api/reportes")]
[Authorize(Policy = "reportes.ver")]
public class ReportesController : ControllerBase
{
    private readonly ReportesService _svc;
    public ReportesController(ReportesService svc) => _svc = svc;

    // ==================================================
    // DASHBOARD (resumen ejecutivo)
    // ==================================================
    [HttpGet("dashboard")]
    public Task<Dictionary<string, object?>> Dashboard(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta) =>
        _svc.DashboardAsync(desde, hasta);

    // ==================================================
    // VENTAS POR DÍA
    // ==================================================
    [HttpGet("ventas-diarias")]
    public Task<List<Dictionary<string, object?>>> VentasDiarias(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta) =>
        _svc.VentasDiariasAsync(desde, hasta);

    // ==================================================
    // RENTABILIDAD
    // ==================================================
    [HttpGet("rentabilidad")]
    public Task<List<Dictionary<string, object?>>> Rentabilidad(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta) =>
        _svc.RentabilidadDiariaAsync(desde, hasta);

    // ==================================================
    // PRODUCTOS MÁS VENDIDOS
    // ==================================================
    [HttpGet("productos-mas-vendidos")]
    public Task<List<Dictionary<string, object?>>> ProductosMasVendidos(
        [FromQuery] int limite = 20) =>
        _svc.ProductosMasVendidosAsync(limite);

    // ==================================================
    // PEDIDOS RESUMEN (filtrable)
    // ==================================================
    [HttpGet("pedidos")]
    public Task<List<Dictionary<string, object?>>> Pedidos(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        [FromQuery] string? estado,
        [FromQuery] string? estadoPago) =>
        _svc.PedidosResumenAsync(desde, hasta, estado, estadoPago);

    // ==================================================
    // COSTOS Y MÁRGENES
    // ==================================================
    [HttpGet("costos-productos")]
    public Task<List<Dictionary<string, object?>>> Costos() =>
        _svc.CostosProductosAsync();

    // ==================================================
    // CAJA POR TURNO
    // ==================================================
    [HttpGet("caja")]
    public Task<List<Dictionary<string, object?>>> Caja(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta) =>
        _svc.CajaTurnosAsync(desde, hasta);

    // ==================================================
    // STOCK
    // ==================================================
    [HttpGet("stock")]
    public Task<List<Dictionary<string, object?>>> Stock(
        [FromQuery] bool soloBajos = false) =>
        _svc.StockAsync(soloBajos);
}