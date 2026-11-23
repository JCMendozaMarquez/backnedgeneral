namespace BackendGeneral.Api.DTOs.Integraciones;

public record ItemPedidoDto(
    Guid? ArticuloId,
    string Descripcion,
    decimal Cantidad,
    decimal PrecioUnitario);