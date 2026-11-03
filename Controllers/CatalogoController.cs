using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BackendGeneral.Api.Services;
using BackendGeneral.Api.DTOs.Catalogo;
using BackendGeneral.Api.Models;

namespace BackendGeneral.Api.Controllers;

[ApiController]
[Route("api/catalogo")]
[Authorize]
public class CatalogoController : ControllerBase
{
    private readonly CatalogoService _svc;
    public CatalogoController(CatalogoService svc) => _svc = svc;

    // ==================================================
    // CATEGORÍAS
    // ==================================================
    [HttpGet("categorias")]
    [Authorize(Policy = "catalogo.ver")]
    public Task<List<Categoria>> GetCategorias() => _svc.ListarCategoriasAsync();

    [HttpGet("categorias/{id:guid}")]
    [Authorize(Policy = "catalogo.ver")]
    public async Task<IActionResult> GetCategoria(Guid id)
    {
        var c = await _svc.ObtenerCategoriaAsync(id);
        return c is null ? NotFound() : Ok(c);
    }

    [HttpPost("categorias")]
    [Authorize(Policy = "catalogo.gestionar")]
    public async Task<IActionResult> PostCategoria([FromBody] CrearCategoriaDto d) =>
        Ok(await _svc.CrearCategoriaAsync(d));

    [HttpPut("categorias/{id:guid}")]
    [Authorize(Policy = "catalogo.gestionar")]
    public async Task<IActionResult> PutCategoria(Guid id, [FromBody] ActualizarCategoriaDto d)
    {
        var c = await _svc.ActualizarCategoriaAsync(id, d);
        return c is null ? NotFound() : Ok(c);
    }

    [HttpDelete("categorias/{id:guid}")]
    [Authorize(Policy = "catalogo.gestionar")]
    public async Task<IActionResult> DeleteCategoria(Guid id) =>
        await _svc.EliminarCategoriaAsync(id) ? NoContent() : NotFound();

    // ==================================================
    // ARTÍCULOS
    // ==================================================
    [HttpGet("articulos")]
    [Authorize(Policy = "catalogo.ver")]
    public Task<List<Articulo>> GetArticulos(
        [FromQuery] string? tipo,
        [FromQuery] Guid? categoriaId) =>
        _svc.ListarArticulosAsync(tipo, categoriaId);

    [HttpGet("articulos/{id:guid}")]
    [Authorize(Policy = "catalogo.ver")]
    public async Task<IActionResult> GetArticulo(Guid id)
    {
        var a = await _svc.ObtenerArticuloAsync(id);
        return a is null ? NotFound() : Ok(a);
    }

    [HttpPost("articulos")]
    [Authorize(Policy = "catalogo.gestionar")]
    public async Task<IActionResult> PostArticulo([FromBody] CrearArticuloDto d) =>
        Ok(await _svc.CrearArticuloAsync(d));

    [HttpPut("articulos/{id:guid}")]
    [Authorize(Policy = "catalogo.gestionar")]
    public async Task<IActionResult> PutArticulo(Guid id, [FromBody] ActualizarArticuloDto d)
    {
        var a = await _svc.ActualizarArticuloAsync(id, d);
        return a is null ? NotFound() : Ok(a);
    }

    [HttpDelete("articulos/{id:guid}")]
    [Authorize(Policy = "catalogo.gestionar")]
    public async Task<IActionResult> DeleteArticulo(Guid id) =>
        await _svc.EliminarArticuloAsync(id) ? NoContent() : NotFound();

    // ==================================================
    // RECETAS
    // ==================================================
    [HttpGet("articulos/{id:guid}/receta")]
    [Authorize(Policy = "catalogo.ver")]
    public Task<List<Receta>> GetReceta(Guid id) => _svc.ObtenerRecetaAsync(id);

    [HttpPut("articulos/{id:guid}/receta")]
    [Authorize(Policy = "catalogo.gestionar")]
    public async Task<IActionResult> PutReceta(Guid id, [FromBody] List<RecetaItemDto> materiales)
    {
        var dto = new CrearRecetaDto(id, materiales);
        return Ok(await _svc.ReemplazarRecetaAsync(dto));
    }

    // ==================================================
    // PROMOCIONES
    // ==================================================
    [HttpGet("promociones")]
    [Authorize(Policy = "catalogo.ver")]
    public Task<List<Promocion>> GetPromos() => _svc.ListarPromocionesAsync();

    [HttpPost("promociones")]
    [Authorize(Policy = "catalogo.gestionar")]
    public async Task<IActionResult> PostPromo([FromBody] CrearPromocionDto d) =>
        Ok(await _svc.CrearPromocionAsync(d));

    [HttpDelete("promociones/{id:guid}")]
    [Authorize(Policy = "catalogo.gestionar")]
    public async Task<IActionResult> DeletePromo(Guid id) =>
        await _svc.EliminarPromocionAsync(id) ? NoContent() : NotFound();

    // ==================================================
    // VISTAS (público / reportes)
    // ==================================================
    [HttpGet("publico")]
    [AllowAnonymous]
    public Task<List<Dictionary<string, object?>>> GetCatalogoPublico(
        [FromQuery] string canal = "web") => _svc.CatalogoAsync(canal);

    [HttpGet("costos")]
    [Authorize(Policy = "catalogo.ver")]
    public Task<List<Dictionary<string, object?>>> GetCostos() => _svc.CostoProductosAsync();
}