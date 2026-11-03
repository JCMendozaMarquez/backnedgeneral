namespace BackendGeneral.Api.DTOs.CobrosCaja;

// ==================== TURNO DE CAJA ====================
public record AbrirTurnoDto(decimal FondoInicial, string? Nota);

public record CerrarTurnoDto(decimal EfectivoContado);

public record TurnoCajaDto(
    Guid Id,
    Guid? UsuarioId,
    DateTime AbiertoEn,
    DateTime? CerradoEn,
    decimal FondoInicial,
    decimal? EfectivoEsperado,
    decimal? EfectivoContado,
    string? Nota);

public record CerrarTurnoResultDto(
    Guid TurnoId,
    decimal EfectivoEsperado,
    decimal EfectivoContado,
    decimal Diferencia);

// ==================== PAGOS ====================
public record CrearPagoDto(
    Guid PedidoId,
    string Metodo,              // efectivo | qr | tarjeta | transferencia | otro
    decimal Monto,
    string? Proveedor,
    string? ReferenciaExterna,
    string? Datos);

public record ConfirmarPagoDto(
    string VerificadoPor,       // manual | banco | webhook
    string? ReferenciaExterna);

public record ReembolsoDto(
    Guid PagoOrigenId,
    decimal Monto,
    string Motivo);

public record PagoDto(
    Guid Id,
    Guid PedidoId,
    Guid? TurnoId,
    Guid? UsuarioId,
    Guid? PagoOrigenId,
    string Tipo,
    string Metodo,
    decimal Monto,
    string Estado,
    string? Proveedor,
    string? ReferenciaExterna,
    DateTime? VerificadoEn,
    string? VerificadoPor,
    DateTime CreatedAt);

// ==================== GASTOS ====================
public record CrearGastoDto(
    Guid? TurnoId,
    Guid? TerceroId,
    string Categoria,           // otro | alquiler | sueldos | servicios | retiro | compra_inventario
    string Concepto,
    decimal Monto,
    string Metodo,              // efectivo | qr | tarjeta | transferencia | otro
    DateTime? Fecha,
    string? Datos);

public record GastoDto(
    Guid Id,
    Guid? TurnoId,
    Guid? TerceroId,
    Guid? UsuarioId,
    string Categoria,
    string Concepto,
    decimal Monto,
    string Metodo,
    DateTime Fecha,
    DateTime CreatedAt);