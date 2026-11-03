namespace BackendGeneral.Api.DTOs.Catalogo;

// ==================== CATEGORÍA ====================
public record CategoriaDto(
    Guid Id,
    string Tipo,
    string Nombre,
    int Orden,
    string[] VisibleEn,
    bool Activo);

public record CrearCategoriaDto(
    string Tipo,
    string Nombre,
    int Orden,
    string[]? VisibleEn);

public record ActualizarCategoriaDto(
    string Tipo,
    string Nombre,
    int Orden,
    string[]? VisibleEn,
    bool Activo);

// ==================== ARTÍCULO ====================
public record ArticuloDto(
    Guid Id,
    Guid? CategoriaId,
    string Tipo,
    string? Codigo,
    string Nombre,
    string? Descripcion,
    string Unidad,
    decimal PrecioVenta,
    decimal CostoActual,
    bool ControlaStock,
    decimal StockMinimo,
    string Opciones,
    string Atributos,
    string? ImagenUrl,
    bool Disponible,
    string[] VisibleEn,
    bool Activo);

public record CrearArticuloDto(
    Guid? CategoriaId,
    string Tipo,
    string? Codigo,
    string Nombre,
    string? Descripcion,
    string Unidad,
    decimal PrecioVenta,
    decimal CostoActual,
    bool ControlaStock,
    decimal StockMinimo,
    string? Opciones,
    string? Atributos,
    string? ImagenUrl,
    string[]? VisibleEn);

public record ActualizarArticuloDto(
    Guid? CategoriaId,
    string? Codigo,
    string Nombre,
    string? Descripcion,
    string Unidad,
    decimal PrecioVenta,
    bool ControlaStock,
    decimal StockMinimo,
    string? Opciones,
    string? Atributos,
    string? ImagenUrl,
    string[]? VisibleEn);

// ==================== RECETA ====================
public record RecetaItemDto(Guid MaterialId, decimal Cantidad);
public record CrearRecetaDto(Guid ArticuloId, List<RecetaItemDto> Materiales);

// ==================== PROMOCIÓN ====================
public record PromocionDto(
    Guid Id,
    string Nombre,
    string Tipo,
    decimal Valor,
    string Reglas,
    DateTime? VigenteDesde,
    DateTime? VigenteHasta,
    bool RequiereAutorizacion,
    bool Activo);

public record CrearPromocionDto(
    string Nombre,
    string Tipo,
    decimal Valor,
    string? Reglas,
    DateTime? VigenteDesde,
    DateTime? VigenteHasta,
    bool RequiereAutorizacion);