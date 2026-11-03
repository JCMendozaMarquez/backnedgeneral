// Models/Pedido.cs
public class Pedido
{
    public Guid Id { get; set; }
    public long Numero { get; set; }
    public Guid? TerceroId { get; set; }
    public Guid? UsuarioId { get; set; }
    public Guid? TurnoId { get; set; }
    public Guid? PromocionId { get; set; }
    public string TipoServicio { get; set; } = "mostrador";
    public string? Mesa { get; set; }
    public string Origen { get; set; } = "pos";
    public string? ReferenciaExterna { get; set; }
    public string Estado { get; set; } = "borrador";
    public decimal Subtotal { get; set; }
    public decimal Descuento { get; set; }
    public decimal Total { get; set; }
    public string? Nota { get; set; }
    public string Datos { get; set; } = "{}";
    public string? AnuladoMotivo { get; set; }
    public string? CodigoSeguimiento { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<PedidoItem> Items { get; set; } = new();
}