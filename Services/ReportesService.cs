using BackendGeneral.Api.Services;

namespace BackendGeneral.Api.Services;

public class ReportesService
{
    private readonly DbViewsService _views;

    public ReportesService(DbViewsService views)
    {
        _views = views;
    }

    // ==================================================
    // VENTAS POR DÍA (v_ventas_diarias)
    // ==================================================
    public Task<List<Dictionary<string, object?>>> VentasDiariasAsync(
        DateTime? desde, DateTime? hasta) =>
        _views.QueryViewAsync(
            @"SELECT * FROM v_ventas_diarias
              WHERE (@desde::date IS NULL OR fecha >= @desde::date)
                AND (@hasta::date IS NULL OR fecha <= @hasta::date)
              ORDER BY fecha DESC",
            ("desde", desde), ("hasta", hasta));

    // ==================================================
    // RENTABILIDAD DIARIA (v_rentabilidad_diaria)
    // ==================================================
    public Task<List<Dictionary<string, object?>>> RentabilidadDiariaAsync(
        DateTime? desde, DateTime? hasta) =>
        _views.QueryViewAsync(
            @"SELECT * FROM v_rentabilidad_diaria
              WHERE (@desde::date IS NULL OR fecha >= @desde::date)
                AND (@hasta::date IS NULL OR fecha <= @hasta::date)
              ORDER BY fecha DESC",
            ("desde", desde), ("hasta", hasta));

    // ==================================================
    // PRODUCTOS MÁS VENDIDOS (v_productos_mas_vendidos)
    // ==================================================
    public Task<List<Dictionary<string, object?>>> ProductosMasVendidosAsync(
        int limite = 20) =>
        _views.QueryViewAsync(
            @"SELECT * FROM v_productos_mas_vendidos
              ORDER BY cantidad_vendida DESC
              LIMIT @limite",
            ("limite", limite));

    // ==================================================
    // PEDIDOS RESUMEN (v_pedidos_resumen)
    // ==================================================
    public Task<List<Dictionary<string, object?>>> PedidosResumenAsync(
        DateTime? desde, DateTime? hasta, string? estado, string? estadoPago) =>
        _views.QueryViewAsync(
            @"SELECT * FROM v_pedidos_resumen
              WHERE (@desde::timestamptz IS NULL OR created_at >= @desde)
                AND (@hasta::timestamptz IS NULL OR created_at <= @hasta)
                AND (@estado::text IS NULL OR estado = @estado)
                AND (@estado_pago::text IS NULL OR estado_pago = @estado_pago)
              ORDER BY created_at DESC",
            ("desde", desde), ("hasta", hasta),
            ("estado", estado), ("estado_pago", estadoPago));

    // ==================================================
    // COSTOS Y MÁRGENES (v_costo_producto)
    // ==================================================
    public Task<List<Dictionary<string, object?>>> CostosProductosAsync() =>
        _views.QueryViewAsync(
            "SELECT * FROM v_costo_producto ORDER BY margen_pct DESC NULLS LAST");

    // ==================================================
    // CAJA POR TURNO (v_caja_turnos)
    // ==================================================
    public Task<List<Dictionary<string, object?>>> CajaTurnosAsync(
        DateTime? desde, DateTime? hasta) =>
        _views.QueryViewAsync(
            @"SELECT * FROM v_caja_turnos
              WHERE (@desde::timestamptz IS NULL OR abierto_en >= @desde)
                AND (@hasta::timestamptz IS NULL OR abierto_en <= @hasta)
              ORDER BY abierto_en DESC",
            ("desde", desde), ("hasta", hasta));

    // ==================================================
    // STOCK (v_stock)
    // ==================================================
    public Task<List<Dictionary<string, object?>>> StockAsync(bool soloBajos = false)
    {
        var sql = soloBajos
            ? "SELECT * FROM v_stock WHERE bajo_minimo = true ORDER BY nombre"
            : "SELECT * FROM v_stock ORDER BY nombre";
        return _views.QueryViewAsync(sql);
    }

    // ==================================================
    // DASHBOARD: resumen ejecutivo de un rango
    // ==================================================
    public async Task<Dictionary<string, object?>> DashboardAsync(
        DateTime? desde, DateTime? hasta)
    {
        var ventas = await VentasDiariasAsync(desde, hasta);
        var rent = await RentabilidadDiariaAsync(desde, hasta);

        decimal totalVentas = ventas.Sum(r =>
            r.TryGetValue("ventas", out var v) && v is decimal d ? d : 0m);
        decimal totalCosto = ventas.Sum(r =>
            r.TryGetValue("costo_ventas", out var v) && v is decimal d ? d : 0m);
        decimal totalGastos = rent.Sum(r =>
            r.TryGetValue("otros_gastos", out var v) && v is decimal d ? d : 0m);
        decimal utilidad = totalVentas - totalCosto;
        decimal resultado = utilidad - totalGastos;
        int pedidos = ventas.Sum(r =>
            r.TryGetValue("pedidos", out var v) && v is int i ? i
            : v is long l ? (int)l : 0);

        return new Dictionary<string, object?>
        {
            ["desde"] = desde,
            ["hasta"] = hasta,
            ["pedidos"] = pedidos,
            ["ventas"] = totalVentas,
            ["costo_ventas"] = totalCosto,
            ["utilidad_bruta"] = utilidad,
            ["otros_gastos"] = totalGastos,
            ["resultado_estimado"] = resultado,
            ["margen_pct"] = totalVentas > 0
                ? Math.Round(resultado / totalVentas * 100, 1)
                : 0m
        };
    }
}