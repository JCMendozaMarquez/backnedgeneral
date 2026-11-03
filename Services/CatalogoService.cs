using Microsoft.EntityFrameworkCore;
using BackendGeneral.Api.Data;
using BackendGeneral.Api.DTOs.Catalogo;
namespace BackendGeneral.Api.Services;
using BackendGeneral.Api.Models;

public class CatalogoService
{
    private readonly AppDbContext _db;
    private readonly DbViewsService _views;

    public CatalogoService(AppDbContext db, DbViewsService views)
    {
        _db = db;
        _views = views;
    }

    // ==================================================
    // CATEGORÍAS
    // ==================================================
    public Task<List<Categoria>> ListarCategoriasAsync() =>
        _db.Categorias
           .OrderBy(c => c.Orden).ThenBy(c => c.Nombre)
           .ToListAsync();

    public Task<Categoria?> ObtenerCategoriaAsync(Guid id) =>
        _db.Categorias.FirstOrDefaultAsync(c => c.Id == id);

    public async Task<Categoria> CrearCategoriaAsync(CrearCategoriaDto d)
    {
        var c = new Categoria
        {
            Tipo = d.Tipo,
            Nombre = d.Nombre,
            Orden = d.Orden,
            VisibleEn = d.VisibleEn ?? new[] { "pos", "web", "whatsapp" },
            Activo = true
        };
        _db.Categorias.Add(c);
        await _db.SaveChangesAsync();
        return c;
    }

    public async Task<Categoria?> ActualizarCategoriaAsync(Guid id, ActualizarCategoriaDto d)
    {
        var c = await _db.Categorias.FindAsync(id);
        if (c is null) return null;

        c.Tipo = d.Tipo;
        c.Nombre = d.Nombre;
        c.Orden = d.Orden;
        if (d.VisibleEn is not null) c.VisibleEn = d.VisibleEn;
        c.Activo = d.Activo;

        await _db.SaveChangesAsync();
        return c;
    }

    public async Task<bool> EliminarCategoriaAsync(Guid id)
    {
        var c = await _db.Categorias.FindAsync(id);
        if (c is null) return false;
        c.Activo = false;               // borrado lógico
        await _db.SaveChangesAsync();
        return true;
    }

    // ==================================================
    // ARTÍCULOS
    // ==================================================
    public Task<List<Articulo>> ListarArticulosAsync(string? tipo = null, Guid? categoriaId = null)
    {
        var q = _db.Articulos.AsQueryable();
        if (!string.IsNullOrWhiteSpace(tipo))
            q = q.Where(a => a.Tipo == tipo);
        if (categoriaId is not null)
            q = q.Where(a => a.CategoriaId == categoriaId);

        return q.OrderBy(a => a.Nombre).ToListAsync();
    }

    public Task<Articulo?> ObtenerArticuloAsync(Guid id) =>
        _db.Articulos.FirstOrDefaultAsync(a => a.Id == id);

    public async Task<Articulo> CrearArticuloAsync(CrearArticuloDto d)
    {
        var a = new Articulo
        {
            CategoriaId = d.CategoriaId,
            Tipo = d.Tipo,
            Codigo = d.Codigo,
            Nombre = d.Nombre,
            Descripcion = d.Descripcion,
            Unidad = d.Unidad,
            PrecioVenta = d.PrecioVenta,
            CostoActual = d.CostoActual,
            ControlaStock = d.ControlaStock,
            StockMinimo = d.StockMinimo,
            Opciones = string.IsNullOrWhiteSpace(d.Opciones) ? "[]" : d.Opciones,
            Atributos = string.IsNullOrWhiteSpace(d.Atributos) ? "{}" : d.Atributos,
            ImagenUrl = d.ImagenUrl,
            VisibleEn = d.VisibleEn ?? new[] { "pos", "web", "whatsapp" },
            Disponible = true,
            Activo = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Articulos.Add(a);
        await _db.SaveChangesAsync();
        return a;
    }

    public async Task<Articulo?> ActualizarArticuloAsync(Guid id, ActualizarArticuloDto d)
    {
        var a = await _db.Articulos.FindAsync(id);
        if (a is null) return null;

        a.CategoriaId = d.CategoriaId;
        a.Codigo = d.Codigo;
        a.Nombre = d.Nombre;
        a.Descripcion = d.Descripcion;
        a.Unidad = d.Unidad;
        a.PrecioVenta = d.PrecioVenta;
        a.ControlaStock = d.ControlaStock;
        a.StockMinimo = d.StockMinimo;
        if (d.Opciones is not null) a.Opciones = d.Opciones;
        if (d.Atributos is not null) a.Atributos = d.Atributos;
        if (d.ImagenUrl is not null) a.ImagenUrl = d.ImagenUrl;
        if (d.VisibleEn is not null) a.VisibleEn = d.VisibleEn;
        a.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return a;
    }

    public async Task<bool> EliminarArticuloAsync(Guid id)
    {
        var a = await _db.Articulos.FindAsync(id);
        if (a is null) return false;
        a.Activo = false;
        await _db.SaveChangesAsync();
        return true;
    }

    // ==================================================
    // RECETAS
    // ==================================================
    public async Task<List<Receta>> ObtenerRecetaAsync(Guid articuloId) =>
        await _db.Recetas.Where(r => r.ArticuloId == articuloId).ToListAsync();

    public async Task<List<Receta>> ReemplazarRecetaAsync(CrearRecetaDto d)
    {
        // Elimina las filas anteriores de la receta
        var previas = _db.Recetas.Where(r => r.ArticuloId == d.ArticuloId);
        _db.Recetas.RemoveRange(previas);

        var nuevas = d.Materiales.Select(m => new Receta
        {
            ArticuloId = d.ArticuloId,
            MaterialId = m.MaterialId,
            Cantidad = m.Cantidad
        }).ToList();

        _db.Recetas.AddRange(nuevas);
        await _db.SaveChangesAsync();
        return nuevas;
    }

    // ==================================================
    // PROMOCIONES
    // ==================================================
    public Task<List<Promocion>> ListarPromocionesAsync() =>
        _db.Promociones.OrderByDescending(p => p.Id).ToListAsync();

    public async Task<Promocion> CrearPromocionAsync(CrearPromocionDto d)
    {
        var p = new Promocion
        {
            Nombre = d.Nombre,
            Tipo = d.Tipo,
            Valor = d.Valor,
            Reglas = string.IsNullOrWhiteSpace(d.Reglas) ? "{}" : d.Reglas,
            VigenteDesde = d.VigenteDesde,
            VigenteHasta = d.VigenteHasta,
            RequiereAutorizacion = d.RequiereAutorizacion,
            Activo = true
        };
        _db.Promociones.Add(p);
        await _db.SaveChangesAsync();
        return p;
    }

    public async Task<bool> EliminarPromocionAsync(Guid id)
    {
        var p = await _db.Promociones.FindAsync(id);
        if (p is null) return false;
        p.Activo = false;
        await _db.SaveChangesAsync();
        return true;
    }

    // ==================================================
    // VISTAS (v_catalogo, v_costo_producto)
    // ==================================================
    public Task<List<Dictionary<string, object?>>> CatalogoAsync(string canal) =>
        _views.QueryViewAsync(
            @"SELECT * FROM v_catalogo
              WHERE @canal = ANY(visible_en)
              ORDER BY categoria_orden, nombre",
            ("canal", canal));

    public Task<List<Dictionary<string, object?>>> CostoProductosAsync() =>
        _views.QueryViewAsync(
            "SELECT * FROM v_costo_producto ORDER BY margen_pct DESC NULLS LAST");
}