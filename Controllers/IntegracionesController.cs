using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BackendGeneral.Api.DTOs.Integraciones;
using BackendGeneral.Api.Models;
using BackendGeneral.Api.Services;

namespace BackendGeneral.Api.Controllers;

[ApiController]
[Route("api/integraciones")]
[Authorize]
public class IntegracionesController : ControllerBase
{
    private readonly IntegracionesService _svc;
    private readonly IConfiguration _config;
    private readonly ILogger<IntegracionesController> _log;

    public IntegracionesController(
        IntegracionesService svc,
        IConfiguration config,
        ILogger<IntegracionesController> log)
    {
        _svc = svc;
        _config = config;
        _log = log;
    }

    // ==================================================
    // MENSAJES (admin)
    // ==================================================
    [HttpGet("mensajes")]
    [Authorize(Policy = "mensajes.gestionar")]
    public Task<List<Mensaje>> Listar(
        [FromQuery] string? canal,
        [FromQuery] string? estado,
        [FromQuery] Guid? pedidoId,
        [FromQuery] string? telefono,
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta) =>
        _svc.ListarAsync(canal, estado, pedidoId, telefono, desde, hasta);

    [HttpGet("mensajes/pedido/{pedidoId:guid}")]
    [Authorize(Policy = "mensajes.gestionar")]
    public Task<List<Mensaje>> PorPedido(Guid pedidoId) =>
        _svc.PorPedidoAsync(pedidoId);

    [HttpPost("mensajes")]
    [Authorize(Policy = "mensajes.gestionar")]
    public async Task<IActionResult> Crear([FromBody] CrearMensajeDto d)
    {
        try { return Ok(await _svc.CrearAsync(d)); }
        catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPatch("mensajes/{id:guid}/estado")]
    [Authorize(Policy = "mensajes.gestionar")]
    public async Task<IActionResult> ActualizarEstado(
        Guid id, [FromBody] ActualizarEstadoMensajeDto d)
    {
        var m = await _svc.ActualizarEstadoAsync(id, d);
        return m is null ? NotFound() : Ok(m);
    }

    // ==================================================
    // ENTRADA PÚBLICA (sin JWT)
    // ==================================================
    [HttpPost("mensajes/publico")]
    [AllowAnonymous]
    public async Task<IActionResult> CrearPublico([FromBody] CrearMensajePublicoDto d)
    {
        try
        {
            var m = await _svc.CrearDesdePublicoAsync(d);
            return Ok(new
            {
                mensajeId = m.Id,
                terceroId = m.TerceroId,
                telefono = m.Telefono
            });
        }
        catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
        catch (Exception ex)
        {
            _log.LogError(ex, "Error en CrearPublico");
            return StatusCode(500, new { error = "Error al guardar el mensaje." });
        }
    }

    // ==================================================
    // CONVERTIR MENSAJE → PEDIDO (admin)
    // ==================================================
    [HttpPost("mensajes/{id:guid}/convertir-pedido")]
    [Authorize(Policy = "mensajes.gestionar")]
    public async Task<IActionResult> ConvertirAPedido(
        Guid id, [FromBody] ConvertirPedidoDto? d)
    {
        try
        {
            var pedido = await _svc.ConvertirMensajeEnPedidoAsync(id, d);
            return pedido is null
                ? NotFound()
                : Ok(new { pedidoId = pedido.Id, numero = pedido.Numero });
        }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
catch (Exception ex)
{
    _log.LogError(ex,
        "Error convirtiendo mensaje {MensajeId}: {FullError}",
        id,
        ex.ToString());

    return StatusCode(500, new
    {
        error = "Error al convertir el mensaje en pedido.",
        detalle = ex.InnerException?.Message ?? ex.Message,
        errorInterno = ex.InnerException?.InnerException?.Message
    });
}
    }

    // ==================================================
    // WEBHOOK WhatsApp
    // ==================================================
    [HttpPost("webhook/whatsapp")]
    [AllowAnonymous]
    public async Task<IActionResult> WebhookWhatsApp(
        [FromBody] WhatsAppWebhookDto d,
        [FromHeader(Name = "X-Webhook-Secret")] string? secret)
    {
        var esperado = _config["Webhooks:WhatsAppSecret"];
        if (!string.IsNullOrEmpty(esperado) && secret != esperado)
            return Unauthorized();

        try
        {
            var m = await _svc.ProcesarWebhookWhatsAppAsync(d);
            return Ok(new { mensajeId = m.Id });
        }
        catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
        catch (Exception ex)
        {
            _log.LogError(ex, "Error en WebhookWhatsApp");
            return StatusCode(500, new { error = "Error procesando webhook." });
        }
    }
}