using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BackendGeneral.Api.DTOs.Inventario;
using BackendGeneral.Api.Models;
using BackendGeneral.Api.Services;

namespace BackendGeneral.Api.Controllers;

[ApiController]
[Route("api/inventario")]
[Authorize]
public class InventarioController : ControllerBase
{
    private readonly InventarioService _svc;
    public InventarioController(InventarioService svc) => _svc = svc;

    private Guid UsuarioActual()
    {
        if (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id))
            return id;
        throw new UnauthorizedAccessException("Token sin NameIdentifier.");
    }

    // ==================================================
    // MOVIMIENTOS
    // ==================================================
    [HttpGet("movimientos")]
    [Authorize(Policy = "inventario.ver")]
    public Task<List<MovimientoInventario>> Listar(
        [FromQuery] Guid? articuloId,
        [FromQuery] string? tipo,
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta) =>
        _svc.ListarMovimientosAsync(articuloId, tipo, desde, hasta);

    [HttpPost("movimientos")]
    [Authorize(Policy = "inventario.gestionar")]
    public async Task<IActionResult> Crear([FromBody] CrearMovimientoDto d)
    {
        try
        {
            return Ok(await _svc.CrearMovimientoAsync(UsuarioActual(), d));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ==================================================
    // STOCK (v_stock)
    // ==================================================
    [HttpGet("stock")]
    [Authorize(Policy = "inventario.ver")]
    public Task<List<Dictionary<string, object?>>> Stock(
        [FromQuery] bool soloBajos = false) => _svc.StockAsync(soloBajos);

    [HttpGet("stock/{articuloId:guid}")]
    [Authorize(Policy = "inventario.ver")]
    public async Task<IActionResult> StockArticulo(Guid articuloId)
    {
        var s = await _svc.StockArticuloAsync(articuloId);
        return s is null ? NotFound() : Ok(s);
    }

    // ==================================================
    // CONSUMO AUTOMÁTICO POR PEDIDO
    // ==================================================
    [HttpPost("consumir/{pedidoId:guid}")]
    [Authorize(Policy = "inventario.gestionar")]
    public async Task<IActionResult> ConsumirPedido(Guid pedidoId)
    {
        try
        {
            var n = await _svc.ConsumirInventarioPedidoAsync(pedidoId);
            return Ok(new { pedidoId, movimientosCreados = n });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}