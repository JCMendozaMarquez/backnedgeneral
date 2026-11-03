using Microsoft.EntityFrameworkCore;
using BackendGeneral.Api.Data;
using BackendGeneral.Api.DTOs.Integraciones;
using BackendGeneral.Api.Models;

namespace BackendGeneral.Api.Services;

public class IntegracionesService
{
    private readonly AppDbContext _db;

    public IntegracionesService(AppDbContext db)
    {
        _db = db;
    }

    // ==================================================
    // CREAR MENSAJE
    // ==================================================
    public async Task<Mensaje> CrearAsync(CrearMensajeDto d)
    {
        // Anti-duplicado: si viene id_externo y ya existe en el mismo canal,
        // devolvemos el existente (el índice único en BD lo impide igual).
        if (!string.IsNullOrWhiteSpace(d.IdExterno))
        {
            var existente = await _db.Mensajes
                .FirstOrDefaultAsync(m => m.Canal == d.Canal && m.IdExterno == d.IdExterno);
            if (existente is not null) return existente;
        }

        var m = new Mensaje
        {
            Canal = d.Canal,
            Direccion = d.Direccion,
            Telefono = d.Telefono,
            PedidoId = d.PedidoId,
            TerceroId = d.TerceroId,
            Tipo = string.IsNullOrWhiteSpace(d.Tipo) ? "otro" : d.Tipo,
            Contenido = d.Contenido,
            Estado = "pendiente",
            IdExterno = d.IdExterno,
            Datos = string.IsNullOrWhiteSpace(d.Datos) ? "{}" : d.Datos,
            CreatedAt = DateTime.UtcNow
        };

        _db.Mensajes.Add(m);
        await _db.SaveChangesAsync();
        return m;
    }

    // ==================================================
    // ACTUALIZAR ESTADO (enviado, entregado, leido, fallido)
    // ==================================================
    public async Task<Mensaje?> ActualizarEstadoAsync(Guid id, ActualizarEstadoMensajeDto d)
    {
        var m = await _db.Mensajes.FindAsync(id);
        if (m is null) return null;

        m.Estado = d.Estado;
        if (!string.IsNullOrWhiteSpace(d.IdExterno))
            m.IdExterno = d.IdExterno;
        if (d.Estado is "enviado" or "entregado" or "leido")
            m.EnviadoEn ??= DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return m;
    }

    // ==================================================
    // LISTAR
    // ==================================================
    public Task<List<Mensaje>> ListarAsync(
        string? canal, string? estado, Guid? pedidoId,
        string? telefono, DateTime? desde, DateTime? hasta)
    {
        var q = _db.Mensajes.AsQueryable();
        if (!string.IsNullOrWhiteSpace(canal)) q = q.Where(m => m.Canal == canal);
        if (!string.IsNullOrWhiteSpace(estado)) q = q.Where(m => m.Estado == estado);
        if (pedidoId is not null) q = q.Where(m => m.PedidoId == pedidoId);
        if (!string.IsNullOrWhiteSpace(telefono)) q = q.Where(m => m.Telefono == telefono);
        if (desde is not null) q = q.Where(m => m.CreatedAt >= desde);
        if (hasta is not null) q = q.Where(m => m.CreatedAt <= hasta);

        return q.OrderByDescending(m => m.CreatedAt).ToListAsync();
    }

    public Task<List<Mensaje>> PorPedidoAsync(Guid pedidoId) =>
        _db.Mensajes
           .Where(m => m.PedidoId == pedidoId)
           .OrderBy(m => m.CreatedAt)
           .ToListAsync();

    // ==================================================
    // WEBHOOK WHATSAPP (entrante)
    // ==================================================
    /// <summary>
    /// Guarda un mensaje entrante de WhatsApp. Si el número coincide con
    /// un tercero existente, lo asocia. No crea pedidos automáticamente:
    /// eso lo decide el backend según las reglas del negocio (aquí se deja
    /// el mensaje pendiente para que un operador lo procese).
    /// </summary>
    public async Task<Mensaje> ProcesarWebhookWhatsAppAsync(WhatsAppWebhookDto d)
    {
        // 1) Anti-duplicado por id_externo
        if (!string.IsNullOrWhiteSpace(d.IdExterno))
        {
            var ya = await _db.Mensajes.FirstOrDefaultAsync(
                m => m.Canal == "whatsapp" && m.IdExterno == d.IdExterno);
            if (ya is not null) return ya;
        }

        // 2) Buscar tercero por teléfono
        var tercero = await _db.Terceros
            .FirstOrDefaultAsync(t => t.Telefono == d.Telefono);

        var m = new Mensaje
        {
            Canal = "whatsapp",
            Direccion = "entrante",
            Telefono = d.Telefono,
            TerceroId = tercero?.Id,
            Tipo = "pedido",
            Contenido = d.Contenido,
            Estado = "entregado",
            IdExterno = d.IdExterno,
            Datos = "{}",
            CreatedAt = DateTime.UtcNow,
            EnviadoEn = DateTime.UtcNow
        };

        _db.Mensajes.Add(m);
        await _db.SaveChangesAsync();
        return m;
    }
}