using Microsoft.EntityFrameworkCore;
using BackendGeneral.Api.Data;
using BackendGeneral.Api.DTOs.Inventario;
using BackendGeneral.Api.Models;

namespace BackendGeneral.Api.Services;

public class InventarioService
{
    private readonly AppDbContext _db;
    private readonly DbViewsService _views;

    public InventarioService(AppDbContext db, DbViewsService views)
    {
        _db = db;
        _views = views;
    }

    // ==================================================
    // CREAR MOVIMIENTO
    // ==================================================
    public async Task<MovimientoInventario> CrearMovimientoAsync(
        Guid usuarioId, CrearMovimientoDto d)
    {
        // Validar signo y tipo según las reglas del check en Postgres
        ValidarMovimiento(d.Tipo, d.Cantidad);

        var art = await _db.Articulos.FindAsync(d.ArticuloId)
            ?? throw new InvalidOperationException("Artículo no encontrado.");

        if (!art.ControlaStock && d.Tipo != "ajuste")
            throw new InvalidOperationException(
                "Este artículo no controla stock. Solo se permiten ajustes.");

        var m = new MovimientoInventario
        {
            ArticuloId = d.ArticuloId,
            Tipo = d.Tipo,
            Cantidad = d.Cantidad,
            CostoUnitario = d.CostoUnitario,
            TerceroId = d.TerceroId,
            PedidoId = d.PedidoId,
            GastoId = d.GastoId,
            UsuarioId = usuarioId,
            Referencia = d.Referencia,
            CreatedAt = DateTime.UtcNow
        };
        _db.MovimientosInventario.Add(m);
        await _db.SaveChangesAsync();

        // Si es entrada con costo, el trigger trg_mov_costo actualiza
        // articulos.costo_actual automáticamente. Recargamos para devolver
        // el costo ya actualizado.
        if (d.Tipo == "entrada" && (d.CostoUnitario ?? 0) > 0)
            await _db.Entry(art).ReloadAsync();

        return m;
    }

    private static void ValidarMovimiento(string tipo, decimal cantidad)
    {
        switch (tipo)
        {
            case "entrada":
                if (cantidad <= 0) throw new ArgumentException("Entrada debe ser > 0.");
                break;
            case "salida":
            case "merma":
            case "consumo":
                if (cantidad >= 0) throw new ArgumentException(
                    $"{tipo} debe ser < 0.");
                break;
            case "ajuste":
                if (cantidad == 0) throw new ArgumentException("Ajuste debe ser ≠ 0.");
                break;
            default:
                throw new ArgumentException($"Tipo inválido: {tipo}");
        }
    }

    // ==================================================
    // LISTAR MOVIMIENTOS
    // ==================================================
    public Task<List<MovimientoInventario>> ListarMovimientosAsync(
        Guid? articuloId, string? tipo, DateTime? desde, DateTime? hasta)
    {
        var q = _db.MovimientosInventario.AsQueryable();
        if (articuloId is not null) q = q.Where(m => m.ArticuloId == articuloId);
        if (!string.IsNullOrWhiteSpace(tipo)) q = q.Where(m => m.Tipo == tipo);
        if (desde is not null) q = q.Where(m => m.CreatedAt >= desde);
        if (hasta is not null) q = q.Where(m => m.CreatedAt <= hasta);

        return q.OrderByDescending(m => m.CreatedAt).ToListAsync();
    }

    // ==================================================
    // VISTA v_stock
    // ==================================================
    public Task<List<Dictionary<string, object?>>> StockAsync(bool soloBajos = false)
    {
        var sql = soloBajos
            ? "SELECT * FROM v_stock WHERE bajo_minimo = true ORDER BY nombre"
            : "SELECT * FROM v_stock ORDER BY nombre";
        return _views.QueryViewAsync(sql);
    }

    public async Task<Dictionary<string, object?>?> StockArticuloAsync(Guid articuloId)
    {
        var rows = await _views.QueryViewAsync(
            "SELECT * FROM v_stock WHERE articulo_id = @id", ("id", articuloId));
        return rows.FirstOrDefault();
    }

    // ==================================================
    // CONSUMO AUTOMÁTICO POR PEDIDO
    // ==================================================
    /// <summary>
    /// Ejecuta fn_consumir_inventario(pedido). Devuelve cuántos
    /// movimientos creó (0 si ya se había ejecutado antes: es idempotente).
    /// </summary>
    public Task<int> ConsumirInventarioPedidoAsync(Guid pedidoId) =>
        _views.ScalarAsync<int>(
            "SELECT fn_consumir_inventario(@pedido)", ("pedido", pedidoId));
}