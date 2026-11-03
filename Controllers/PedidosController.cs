using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BackendGeneral.Api.DTOs.Pedidos;
using BackendGeneral.Api.Models;
using BackendGeneral.Api.Services;

namespace BackendGeneral.Api.Controllers;

[ApiController]
[Route("api/pedidos")]
[Authorize]
public class PedidosController : ControllerBase
{
    private readonly PedidosService _svc;
    public PedidosController(PedidosService svc) => _svc = svc;

    private Guid? UsuarioActual() =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    // ==================================================
    // LISTAR / RESUMEN
    // ==================================================
    [HttpGet]
    [Authorize(Policy = "pedidos.ver")]
    public Task<List<Pedido>> Get(
        [FromQuery] string? estado,
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta) =>
        _svc.ListarAsync(estado, desde, hasta);

    [HttpGet("resumen")]
    [Authorize(Policy = "pedidos.ver")]
    public Task<List<Dictionary<string, object?>>> Resumen(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta) =>
        _svc.ResumenAsync(desde, hasta);

    // ==================================================
    // DETALLE (entidad o vista)
    // ==================================================
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "pedidos.ver")]
    public async Task<IActionResult> GetDetalle(Guid id)
    {
        var p = await _svc.ObtenerAsync(id);
        return p is null ? NotFound() : Ok(p);
    }

    [HttpGet("{id:guid}/vista")]
    [Authorize(Policy = "pedidos.ver")]
    public async Task<IActionResult> GetDetalleVista(Guid id)
    {
        var d = await _svc.DetalleAsync(id);
        return d is null ? NotFound() : Ok(d);
    }

    // ==================================================
    // CREAR / ESTADO / ANULAR
    // ==================================================
    [HttpPost]
    [Authorize(Policy = "pedidos.crear")]
    public async Task<IActionResult> Crear([FromBody] CrearPedidoDto d) =>
        Ok(await _svc.CrearAsync(d, UsuarioActual()));

    [HttpPatch("{id:guid}/estado")]
    [Authorize(Policy = "pedidos.ver")]
    public async Task<IActionResult> CambiarEstado(Guid id, [FromBody] CambiarEstadoDto d)
    {
        var p = await _svc.CambiarEstadoAsync(id, d.Estado);
        return p is null ? NotFound() : Ok(p);
    }

    [HttpPost("{id:guid}/anular")]
    [Authorize(Policy = "pedidos.anular")]
    public async Task<IActionResult> Anular(Guid id, [FromBody] AnularPedidoDto d)
    {
        var p = await _svc.AnularAsync(id, d.Motivo);
        return p is null ? NotFound() : Ok(p);
    }

    // ==================================================
    // SEGUIMIENTO PÚBLICO
    // ==================================================
    [HttpGet("seguimiento/{codigo}")]
    [AllowAnonymous]
    public async Task<IActionResult> Seguimiento(string codigo)
    {
        var d = await _svc.SeguimientoAsync(codigo);
        return d is null ? NotFound() : Ok(d);
    }
}