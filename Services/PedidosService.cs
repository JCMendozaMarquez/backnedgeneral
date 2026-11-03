using Microsoft.EntityFrameworkCore;
using BackendGeneral.Api.Data;
using BackendGeneral.Api.DTOs.Pedidos;
using BackendGeneral.Api.Models;

namespace BackendGeneral.Api.Services;

public class PedidosService
{
    private readonly AppDbContext _db;
    private readonly DbViewsService _views;

    public PedidosService(AppDbContext db, DbViewsService views)
    {
        _db = db;
        _views = views;
    }

    // ==================================================
    // CREAR
    // ==================================================
    public async Task<Pedido> CrearAsync(CrearPedidoDto d, Guid? usuarioId)
    {
        // Turno abierto del usuario (si existe). Ojo: usa el DbSet de turnos_caja.
        // Si todavía no creaste el modelo TurnoCaja, deja turno = null.
        TurnoCaja? turno = null;
        if (usuarioId is not null && _db.Model.FindEntityType(typeof(TurnoCaja)) is not null)
        {
            turno = await _db.TurnosCaja
                .FirstOrDefaultAsync(t => t.UsuarioId == usuarioId && t.CerradoEn == null);
        }

        var pedido = new Pedido
        {
            TerceroId = d.TerceroId,
            UsuarioId = usuarioId,
            TurnoId = turno?.Id,
            TipoServicio = d.TipoServicio,
            Mesa = d.Mesa,
            Origen = d.Origen,
            ReferenciaExterna = d.ReferenciaExterna,
            Descuento = d.Descuento,
            Nota = d.Nota,
            Datos = string.IsNullOrWhiteSpace(d.Datos) ? "{}" : d.Datos,
            Estado = "pendiente",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Pedidos.Add(pedido);

        foreach (var i in d.Items)
        {
            _db.PedidoItems.Add(new PedidoItem
            {
                PedidoId = pedido.Id,
                ArticuloId = i.ArticuloId,
                Descripcion = i.Descripcion,
                Cantidad = i.Cantidad,
                PrecioUnitario = i.PrecioUnitario,
                Descuento = i.Descuento,
                Opciones = string.IsNullOrWhiteSpace(i.Opciones) ? "[]" : i.Opciones,
                Nota = i.Nota
            });
        }

        await _db.SaveChangesAsync();

        // El trigger de Postgres recalcula subtotal/descuento/total en pedidos.
        // Recargamos para devolver valores frescos.
        await _db.Entry(pedido).ReloadAsync();
        return pedido;
    }

    // ==================================================
    // LISTAR
    // ==================================================
    public Task<List<Pedido>> ListarAsync(string? estado, DateTime? desde, DateTime? hasta)
    {
        var q = _db.Pedidos.AsQueryable();
        if (!string.IsNullOrWhiteSpace(estado))
            q = q.Where(p => p.Estado == estado);
        if (desde is not null)
            q = q.Where(p => p.CreatedAt >= desde);
        if (hasta is not null)
            q = q.Where(p => p.CreatedAt <= hasta);

        return q.OrderByDescending(p => p.CreatedAt).ToListAsync();
    }

    public async Task<Pedido?> ObtenerAsync(Guid id) =>
        await _db.Pedidos
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == id);

    // ==================================================
    // CAMBIAR ESTADO / ANULAR
    // ==================================================
    public async Task<Pedido?> CambiarEstadoAsync(Guid id, string nuevoEstado)
    {
        var p = await _db.Pedidos.FindAsync(id);
        if (p is null) return null;
        p.Estado = nuevoEstado;
        p.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return p;
    }

    public async Task<Pedido?> AnularAsync(Guid id, string motivo)
    {
        var p = await _db.Pedidos.FindAsync(id);
        if (p is null) return null;
        p.Estado = "anulado";
        p.AnuladoMotivo = motivo;
        p.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return p;
    }

    // ==================================================
    // VISTAS
    // ==================================================
    public Task<List<Dictionary<string, object?>>> ResumenAsync(DateTime? desde, DateTime? hasta) =>
        _views.QueryViewAsync(
            @"SELECT * FROM v_pedidos_resumen
              WHERE (@desde::timestamptz IS NULL OR created_at >= @desde)
                AND (@hasta::timestamptz IS NULL OR created_at <= @hasta)
              ORDER BY created_at DESC",
            ("desde", desde), ("hasta", hasta));

    public async Task<Dictionary<string, object?>?> DetalleAsync(Guid id)
    {
        var rows = await _views.QueryViewAsync(
            "SELECT * FROM v_pedido_detalle WHERE id = @id",
            ("id", id));
        return rows.FirstOrDefault();
    }

    // Público: el cliente consulta por código de seguimiento
    public async Task<Dictionary<string, object?>?> SeguimientoAsync(string codigo)
    {
        var rows = await _views.QueryViewAsync(
            @"SELECT p.numero, p.estado, p.total, p.created_at,
                     (SELECT jsonb_agg(jsonb_build_object(
                         'descripcion', i.descripcion,
                         'cantidad',    i.cantidad,
                         'total',       i.total))
                        FROM pedido_items i WHERE i.pedido_id = p.id) AS items
                FROM pedidos p
               WHERE p.codigo_seguimiento = @c",
            ("c", codigo));
        return rows.FirstOrDefault();
    }
}