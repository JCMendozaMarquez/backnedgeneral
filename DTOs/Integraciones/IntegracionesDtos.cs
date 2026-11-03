namespace BackendGeneral.Api.DTOs.Integraciones;

// ==================== MENSAJE ====================
public record CrearMensajeDto(
    string Canal,              // whatsapp | sms | email
    string Direccion,          // entrante | saliente
    string? Telefono,
    Guid? PedidoId,
    Guid? TerceroId,
    string Tipo,               // pedido | comprobante | estado | promocion | otro
    string? Contenido,
    string? IdExterno,
    string? Datos);            // jsonb como texto (opcional)

public record ActualizarEstadoMensajeDto(
    string Estado,             // pendiente | enviado | entregado | leido | fallido
    string? IdExterno);

public record MensajeDto(
    Guid Id,
    string Canal,
    string Direccion,
    string? Telefono,
    Guid? PedidoId,
    Guid? TerceroId,
    string Tipo,
    string? Contenido,
    string Estado,
    string? IdExterno,
    string Datos,
    DateTime CreatedAt,
    DateTime? EnviadoEn);

// ==================== WEBHOOK WHATSAPP ====================
public record WhatsAppWebhookDto(
    string? IdExterno,
    string Telefono,
    string Contenido,
    string? Nombre);