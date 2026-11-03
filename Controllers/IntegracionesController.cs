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
    public IntegracionesController(IntegracionesService svc) => _svc = svc;

    // ==================================================
    // MENSAJES
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
        try
        {
            return Ok(await _svc.CrearAsync(d));
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
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
    // WEBHOOK WHATSAPP (público, sin JWT)
    // ==================================================
    /// <summary>
    /// Endpoint que llamará el proveedor (Meta, Twilio, etc.) cuando
    /// llegue un mensaje. Sin autenticación de usuario: se protege por
    /// un token compartido en el header (X-Webhook-Secret) que tú
    /// configures en appsettings. Lo dejo preparado.
    /// </summary>
    [HttpPost("webhook/whatsapp")]
    [AllowAnonymous]
    public async Task<IActionResult> WebhookWhatsApp(
        [FromBody] WhatsAppWebhookDto d,
        [FromHeader(Name = "X-Webhook-Secret")] string? secret,
        [FromServices] IConfiguration config)
    {
        var esperado = config["Webhooks:WhatsAppSecret"];
        if (!string.IsNullOrEmpty(esperado) && secret != esperado)
            return Unauthorized();

        var m = await _svc.ProcesarWebhookWhatsAppAsync(d);
        return Ok(new { mensajeId = m.Id });
    }
}