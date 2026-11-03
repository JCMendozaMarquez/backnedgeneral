namespace BackendGeneral.Api.DTOs.Inventario;

// ==================== MOVIMIENTOS ====================
public record CrearMovimientoDto(
    Guid ArticuloId,
    string Tipo,                 // entrada | salida | ajuste | merma | consumo
    decimal Cantidad,            // con signo según el tipo (ver validación en el service)
    decimal? CostoUnitario,
    Guid? TerceroId,             // proveedor (solo entradas)
    Guid? PedidoId,              // consumo por venta
    Guid? GastoId,               // pago de la compra
    string? Referencia);         // n.º factura / motivo

public record MovimientoDto(
    Guid Id,
    Guid ArticuloId,
    string Tipo,
    decimal Cantidad,
    decimal? CostoUnitario,
    Guid? TerceroId,
    Guid? PedidoId,
    Guid? GastoId,
    Guid? UsuarioId,
    string? Referencia,
    DateTime CreatedAt);