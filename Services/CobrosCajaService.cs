using Microsoft.EntityFrameworkCore;
using BackendGeneral.Api.Data;
using BackendGeneral.Api.DTOs.CobrosCaja;
using BackendGeneral.Api.Models;

namespace BackendGeneral.Api.Services;

public class CobrosCajaService
{
    private readonly AppDbContext _db;
    private readonly DbViewsService _views;

    public CobrosCajaService(AppDbContext db, DbViewsService views)
    {
        _db = db;
        _views = views;
    }

    // ==================================================
    // TURNOS DE CAJA
    // ==================================================
    public Task<TurnoCaja?> TurnoAbiertoAsync(Guid usuarioId) =>
        _db.TurnosCaja
           .Where(t => t.UsuarioId == usuarioId && t.CerradoEn == null)
           .OrderByDescending(t => t.AbiertoEn)
           .FirstOrDefaultAsync();

    public Task<List<TurnoCaja>> ListarTurnosAsync(DateTime? desde, DateTime? hasta)
    {
        var q = _db.TurnosCaja.AsQueryable();
        if (desde is not null) q = q.Where(t => t.AbiertoEn >= desde);
        if (hasta is not null) q = q.Where(t => t.AbiertoEn <= hasta);
        return q.OrderByDescending(t => t.AbiertoEn).ToListAsync();
    }

    public async Task<TurnoCaja> AbrirTurnoAsync(Guid usuarioId, AbrirTurnoDto d)
    {
        // Validar que no tenga ya un turno abierto (el índice único lo impide en BD)
        var existente = await TurnoAbiertoAsync(usuarioId);
        if (existente is not null)
            throw new InvalidOperationException(
                $"Ya tienes un turno abierto (id {existente.Id}). Ciérralo antes de abrir otro.");

        var t = new TurnoCaja
        {
            UsuarioId = usuarioId,
            AbiertoEn = DateTime.UtcNow,
            FondoInicial = d.FondoInicial,
            Nota = d.Nota
        };
        _db.TurnosCaja.Add(t);
        await _db.SaveChangesAsync();
        return t;
    }

    public async Task<CerrarTurnoResultDto> CerrarTurnoAsync(Guid turnoId, CerrarTurnoDto d)
    {
        // Llama a la función Postgres fn_cerrar_turno que devuelve la diferencia.
        var diferencia = await _views.ScalarAsync<decimal>(
            "SELECT fn_cerrar_turno(@turno, @contado)",
            ("turno", turnoId),
            ("contado", d.EfectivoContado));

        // Recargamos el turno ya cerrado para devolver efectivo_esperado
        var t = await _db.TurnosCaja.FindAsync(turnoId);
        if (t is null)
            throw new InvalidOperationException($"Turno {turnoId} no encontrado.");

        await _db.Entry(t).ReloadAsync();

        return new CerrarTurnoResultDto(
            TurnoId: t.Id,
            EfectivoEsperado: t.EfectivoEsperado ?? 0m,
            EfectivoContado: t.EfectivoContado ?? d.EfectivoContado,
            Diferencia: diferencia);
    }

    // ==================================================
    // PAGOS
    // ==================================================
    public async Task<Pago> RegistrarPagoAsync(Guid usuarioId, CrearPagoDto d)
    {
        // Turno abierto del usuario (si existe)
        var turno = await TurnoAbiertoAsync(usuarioId);

        var p = new Pago
        {
            PedidoId = d.PedidoId,
            TurnoId = turno?.Id,
            UsuarioId = usuarioId,
            Tipo = "cobro",
            Metodo = d.Metodo,
            Monto = d.Monto,
            Estado = "pendiente",
            Proveedor = d.Proveedor,
            ReferenciaExterna = d.ReferenciaExterna,
            Datos = string.IsNullOrWhiteSpace(d.Datos) ? "{}" : d.Datos,
            CreatedAt = DateTime.UtcNow
        };
        _db.Pagos.Add(p);
        await _db.SaveChangesAsync();
        return p;
    }

    public async Task<Pago?> ConfirmarPagoAsync(Guid pagoId, ConfirmarPagoDto d)
    {
        var p = await _db.Pagos.FindAsync(pagoId);
        if (p is null) return null;

        p.Estado = "confirmado";
        p.VerificadoEn = DateTime.UtcNow;
        p.VerificadoPor = d.VerificadoPor;
        if (!string.IsNullOrWhiteSpace(d.ReferenciaExterna))
            p.ReferenciaExterna = d.ReferenciaExterna;

        await _db.SaveChangesAsync();
        return p;
    }

    public async Task<Pago?> RechazarPagoAsync(Guid pagoId, string motivo)
    {
        var p = await _db.Pagos.FindAsync(pagoId);
        if (p is null) return null;

        p.Estado = "rechazado";
        // guardamos el motivo dentro de datos jsonb sin romper el json existente
        p.Datos = string.IsNullOrWhiteSpace(motivo)
            ? p.Datos
            : $"{{\"motivo_rechazo\":\"{motivo.Replace("\"", "\\\"")}\"}}";
        await _db.SaveChangesAsync();
        return p;
    }

    public Task<List<Pago>> ListarPagosPorPedidoAsync(Guid pedidoId) =>
        _db.Pagos
           .Where(p => p.PedidoId == pedidoId)
           .OrderBy(p => p.CreatedAt)
           .ToListAsync();

    public async Task<Pago> ReembolsoAsync(Guid usuarioId, ReembolsoDto d)
    {
        var original = await _db.Pagos.FindAsync(d.PagoOrigenId)
            ?? throw new InvalidOperationException("Pago origen no encontrado.");

        var turno = await TurnoAbiertoAsync(usuarioId);

        var r = new Pago
        {
            PedidoId = original.PedidoId,
            TurnoId = turno?.Id,
            UsuarioId = usuarioId,
            PagoOrigenId = original.Id,
            Tipo = "reembolso",
            Metodo = original.Metodo,
            Monto = d.Monto,
            Estado = "confirmado",
            VerificadoEn = DateTime.UtcNow,
            VerificadoPor = "manual",
            Datos = $"{{\"motivo\":\"{d.Motivo.Replace("\"", "\\\"")}\"}}",
            CreatedAt = DateTime.UtcNow
        };
        _db.Pagos.Add(r);
        await _db.SaveChangesAsync();
        return r;
    }

    // ==================================================
    // GASTOS
    // ==================================================
    public async Task<Gasto> RegistrarGastoAsync(Guid usuarioId, CrearGastoDto d)
    {
        // Si no viene turno, usa el turno abierto del usuario
        Guid? turnoId = d.TurnoId;
        if (turnoId is null)
        {
            var turno = await TurnoAbiertoAsync(usuarioId);
            turnoId = turno?.Id;
        }

        var g = new Gasto
        {
            TurnoId = turnoId,
            TerceroId = d.TerceroId,
            UsuarioId = usuarioId,
            Categoria = d.Categoria,
            Concepto = d.Concepto,
            Monto = d.Monto,
            Metodo = d.Metodo,
            Fecha = d.Fecha.HasValue
                ? DateOnly.FromDateTime(d.Fecha.Value)
                : DateOnly.FromDateTime(DateTime.UtcNow),
            Datos = string.IsNullOrWhiteSpace(d.Datos) ? "{}" : d.Datos,
            CreatedAt = DateTime.UtcNow
        };
        _db.Gastos.Add(g);
        await _db.SaveChangesAsync();
        return g;
    }

    public Task<List<Gasto>> ListarGastosAsync(DateTime? desde, DateTime? hasta, string? categoria)
    {
        var q = _db.Gastos.AsQueryable();
        if (desde is not null) q = q.Where(g => g.Fecha >= DateOnly.FromDateTime(desde.Value));
        if (hasta is not null) q = q.Where(g => g.Fecha <= DateOnly.FromDateTime(hasta.Value));
        if (!string.IsNullOrWhiteSpace(categoria)) q = q.Where(g => g.Categoria == categoria);
        return q.OrderByDescending(g => g.Fecha).ToListAsync();
    }

    // ==================================================
    // VISTA v_caja_turnos
    // ==================================================
    public Task<List<Dictionary<string, object?>>> CajaTurnosAsync(Guid? turnoId)
    {
        if (turnoId is null)
            return _views.QueryViewAsync(
                "SELECT * FROM v_caja_turnos ORDER BY abierto_en DESC");

        return _views.QueryViewAsync(
            "SELECT * FROM v_caja_turnos WHERE turno_id = @turno",
            ("turno", turnoId));
    }
}