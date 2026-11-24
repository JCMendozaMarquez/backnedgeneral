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
    // ACTUALIZAR ESTADO
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
    // ENTRADA PÚBLICA (sin JWT) — botón del catálogo
    // ==================================================
    public async Task<Mensaje> CrearDesdePublicoAsync(CrearMensajePublicoDto d)
    {
        var telefono = NormalizarTelefonoBolivia(d.Telefono);
        if (telefono is null)
            throw new ArgumentException("Teléfono inválido. Debe ser un celular boliviano de 8 dígitos (empieza en 6 o 7).");

        var tercero = await ObtenerOCrearTerceroAsync(telefono, d.Nombre);

        var m = new Mensaje
        {
            Canal = "whatsapp",
            Direccion = "entrante",
            Telefono = telefono,
            TerceroId = tercero.Id,
            Tipo = "pedido",
            Contenido = d.Contenido,
            Estado = "pendiente",
            Datos = string.IsNullOrWhiteSpace(d.Datos) ? "{}" : d.Datos,
            CreatedAt = DateTime.UtcNow
        };

        _db.Mensajes.Add(m);
        await _db.SaveChangesAsync();
        return m;
    }

    public static string? NormalizarTelefonoBolivia(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;

        var digits = new string(raw.Where(char.IsDigit).ToArray());

        if (digits.StartsWith("591") && digits.Length > 8)
            digits = digits.Substring(3);

        if (digits.Length != 8) return null;
        if (digits[0] != '6' && digits[0] != '7') return null;

        return digits;
    }

    // ==================================================
    // CONVERTIR MENSAJE → PEDIDO (admin)
    // ==================================================
    public async Task<Pedido?> ConvertirMensajeEnPedidoAsync(Guid mensajeId, ConvertirPedidoDto? opts = null)
    {
        var msg = await _db.Mensajes.FirstOrDefaultAsync(m => m.Id == mensajeId);
        if (msg is null) return null;
        if (msg.PedidoId.HasValue)
            throw new InvalidOperationException("Este mensaje ya está asociado a un pedido.");

        Guid? terceroId = msg.TerceroId;
        if (terceroId is null && !string.IsNullOrEmpty(msg.Telefono))
        {
            var t = await ObtenerOCrearTerceroAsync(msg.Telefono, null);
            terceroId = t.Id;
            msg.TerceroId = t.Id;
        }

        List<ItemPedidoDto> items;
        decimal total;
        if (opts?.Items is { Count: > 0 })
        {
            items = opts.Items;
            total = items.Sum(i => i.Cantidad * i.PrecioUnitario);
        }
        else
        {
            var parsed = new PedidoParser().Parse(msg.Contenido ?? "");
            if (!parsed.Ok)
                throw new InvalidOperationException(
                    "No se pudieron detectar ítems en el mensaje. Envía 'items' manualmente.");
            items = parsed.Items;
            total = parsed.Total;
        }

        var pedido = await CrearPedidoAsync(
            terceroId, items, total,
            tipoServicio: opts?.TipoServicio ?? "delivery",
            origen: "whatsapp",
            nota: opts?.Nota ?? $"Convertido desde mensaje WhatsApp ({msg.Telefono})",
            datosJson: msg.Datos);

        msg.PedidoId = pedido.Id;
        msg.Estado = "convertido";
        await _db.SaveChangesAsync();
        return pedido;
    }

    // ==================================================
    // HELPERS PRIVADOS
    // ==================================================
    private async Task<Tercero> ObtenerOCrearTerceroAsync(string telefono, string? nombre)
    {
        var existente = await _db.Terceros
            .FirstOrDefaultAsync(t => t.Telefono == telefono);

        if (existente is not null)
        {
            if (string.IsNullOrWhiteSpace(existente.Nombre) && !string.IsNullOrWhiteSpace(nombre))
            {
                existente.Nombre = nombre;
                await _db.SaveChangesAsync();
            }
            return existente;
        }

        var nuevo = new Tercero
        {
            Nombre = string.IsNullOrWhiteSpace(nombre) ? $"Cliente WA {telefono}" : nombre,
            Telefono = telefono,
            Tipo = "cliente",
            Activo = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Terceros.Add(nuevo);
        await _db.SaveChangesAsync();
        return nuevo;
    }

    private async Task<Pedido> CrearPedidoAsync(
        Guid? terceroId,
        List<ItemPedidoDto> items,
        decimal total,
        string tipoServicio,
        string origen,
        string? nota,
        string? datosJson)
    {
        var pedido = new Pedido
        {
            TerceroId = terceroId,
            Estado = "pendiente",
            Origen = origen,
            TipoServicio = tipoServicio,
            Subtotal = total,
            Total = total,
            Descuento = 0,
            Nota = nota,
            Datos = datosJson,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Pedidos.Add(pedido);
        await _db.SaveChangesAsync();

        foreach (var i in items)
        {
            _db.PedidoItems.Add(new PedidoItem
            {
                PedidoId = pedido.Id,
                ArticuloId = i.ArticuloId,
                Descripcion = i.Descripcion,
                Cantidad = i.Cantidad,
                PrecioUnitario = i.PrecioUnitario,
                Total = i.Cantidad * i.PrecioUnitario,
                Descuento = 0
            });
        }
        await _db.SaveChangesAsync();
        return pedido;
    }

    // ==================================================
    // WEBHOOK WhatsApp (entrante)
    // ==================================================
    public async Task<Mensaje> ProcesarWebhookWhatsAppAsync(WhatsAppWebhookDto d)
    {
        if (!string.IsNullOrWhiteSpace(d.IdExterno))
        {
            var ya = await _db.Mensajes.FirstOrDefaultAsync(
                m => m.Canal == "whatsapp" && m.IdExterno == d.IdExterno);
            if (ya is not null) return ya;
        }

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
            Estado = "pendiente",
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