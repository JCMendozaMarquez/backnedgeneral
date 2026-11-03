// Models/PedidoItem.cs
public class PedidoItem
{
    public Guid Id { get; set; }
    public Guid PedidoId { get; set; }
    public Guid? ArticuloId { get; set; }
    public string? Descripcion { get; set; }
    public decimal Cantidad { get; set; }
    public decimal? PrecioUnitario { get; set; }
    public decimal? CostoUnitario { get; set; }
    public decimal Descuento { get; set; }
    public string Opciones { get; set; } = "[]";
    public string? Nota { get; set; }
    public decimal Total { get; set; } // generated always → se marca ValueGeneratedOnAddOrUpdate
}