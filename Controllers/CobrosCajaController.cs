using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BackendGeneral.Api.DTOs.CobrosCaja;
using BackendGeneral.Api.Models;
using BackendGeneral.Api.Services;

namespace BackendGeneral.Api.Controllers;

[ApiController]
[Route("api/caja")]
[Authorize]
public class CobrosCajaController : ControllerBase
{
    private readonly CobrosCajaService _svc;
    public CobrosCajaController(CobrosCajaService svc) => _svc = svc;

    private Guid UsuarioActual()
    {
        if (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id))
            return id;
        throw new UnauthorizedAccessException("Token sin NameIdentifier.");
    }

    // ==================================================
    // TURNOS
    // ==================================================
    [HttpGet("turno/actual")]
    [Authorize(Policy = "caja.operar")]
    public async Task<IActionResult> TurnoActual()
    {
        var t = await _svc.TurnoAbiertoAsync(UsuarioActual());
        return t is null ? NoContent() : Ok(t);
    }

    [HttpGet("turnos")]
    [Authorize(Policy = "caja.operar")]
    public Task<List<TurnoCaja>> ListarTurnos(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta) =>
        _svc.ListarTurnosAsync(desde, hasta);

    [HttpPost("turnos/abrir")]
    [Authorize(Policy = "caja.operar")]
    public async Task<IActionResult> Abrir([FromBody] AbrirTurnoDto d)
    {
        try
        {
            return Ok(await _svc.AbrirTurnoAsync(UsuarioActual(), d));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpPost("turnos/{id:guid}/cerrar")]
    [Authorize(Policy = "caja.operar")]
    public async Task<IActionResult> Cerrar(Guid id, [FromBody] CerrarTurnoDto d)
    {
        try
        {
            return Ok(await _svc.CerrarTurnoAsync(id, d));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ==================================================
    // PAGOS
    // ==================================================
    [HttpPost("pagos")]
    [Authorize(Policy = "pagos.registrar")]
    public async Task<IActionResult> RegistrarPago([FromBody] CrearPagoDto d) =>
        Ok(await _svc.RegistrarPagoAsync(UsuarioActual(), d));

    [HttpPost("pagos/{id:guid}/confirmar")]
    [Authorize(Policy = "pagos.registrar")]
    public async Task<IActionResult> ConfirmarPago(Guid id, [FromBody] ConfirmarPagoDto d)
    {
        var p = await _svc.ConfirmarPagoAsync(id, d);
        return p is null ? NotFound() : Ok(p);
    }

    [HttpPost("pagos/{id:guid}/rechazar")]
    [Authorize(Policy = "pagos.registrar")]
    public async Task<IActionResult> RechazarPago(Guid id, [FromQuery] string motivo)
    {
        var p = await _svc.RechazarPagoAsync(id, motivo);
        return p is null ? NotFound() : Ok(p);
    }

    [HttpGet("pagos/pedido/{pedidoId:guid}")]
    [Authorize(Policy = "pagos.registrar")]
    public Task<List<Pago>> PagosPorPedido(Guid pedidoId) =>
        _svc.ListarPagosPorPedidoAsync(pedidoId);

    [HttpPost("pagos/reembolso")]
    [Authorize(Policy = "pagos.registrar")]
    public async Task<IActionResult> Reembolso([FromBody] ReembolsoDto d)
    {
        try
        {
            return Ok(await _svc.ReembolsoAsync(UsuarioActual(), d));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ==================================================
    // GASTOS
    // ==================================================
    [HttpPost("gastos")]
    [Authorize(Policy = "gastos.registrar")]
    public async Task<IActionResult> RegistrarGasto([FromBody] CrearGastoDto d) =>
        Ok(await _svc.RegistrarGastoAsync(UsuarioActual(), d));

    [HttpGet("gastos")]
    [Authorize(Policy = "gastos.registrar")]
    public Task<List<Gasto>> ListarGastos(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        [FromQuery] string? categoria) =>
        _svc.ListarGastosAsync(desde, hasta, categoria);

    // ==================================================
    // VISTA v_caja_turnos
    // ==================================================
    [HttpGet("caja")]
    [Authorize(Policy = "caja.operar")]
    public Task<List<Dictionary<string, object?>>> Caja([FromQuery] Guid? turnoId) =>
        _svc.CajaTurnosAsync(turnoId);
}