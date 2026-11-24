namespace BackendGeneral.Api.DTOs.Integraciones;

public record ConvertirPedidoDto(
    string? Nota,
    string? TipoServicio,
    List<ItemPedidoDto>? Items);