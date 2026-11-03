// Models/Pago.cs
public class Pago
{
    public Guid Id { get; set; }
    public Guid PedidoId { get; set; }
    public Guid? TurnoId { get; set; }
    public Guid? UsuarioId { get; set; }
    public Guid? PagoOrigenId { get; set; }
    public string Tipo { get; set; } = "cobro";
    public string Metodo { get; set; } = default!;
    public decimal Monto { get; set; }
    public string Estado { get; set; } = "pendiente";
    public string? Proveedor { get; set; }
    public string? ReferenciaExterna { get; set; }
    public string? QrPayload { get; set; }
    public DateTime? ExpiraEn { get; set; }
    public DateTime? VerificadoEn { get; set; }
    public string? VerificadoPor { get; set; }
    public string Datos { get; set; } = "{}";
    public DateTime CreatedAt { get; set; }
}