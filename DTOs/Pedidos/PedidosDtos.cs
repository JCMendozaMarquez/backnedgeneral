namespace BackendGeneral.Api.DTOs.Pedidos;

// ==================== ITEM ====================
public record ItemPedidoDto(
    Guid? ArticuloId,
    string? Descripcion,
    decimal Cantidad,
    decimal? PrecioUnitario,
    decimal Descuento,
    string? Opciones,
    string? Nota);

// ==================== CREAR ====================
public record CrearPedidoDto(
    Guid? TerceroId,
    string TipoServicio,          // mostrador | mesa | llevar | delivery
    string? Mesa,
    string Origen,                // pos | whatsapp | web | pedidosya...
    string? ReferenciaExterna,
    decimal Descuento,
    string? Nota,
    string? Datos,
    List<ItemPedidoDto> Items);

// ==================== ANULAR ====================
public record AnularPedidoDto(string Motivo);

// ==================== CAMBIAR ESTADO ====================
public record CambiarEstadoDto(string Estado);