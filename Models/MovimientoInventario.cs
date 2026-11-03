using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendGeneral.Api.Models;

[Table("movimientos_inventario")]
public class MovimientoInventario
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("articulo_id")]
    public Guid ArticuloId { get; set; }

    [Column("tipo")]
    [MaxLength(20)]
    public string Tipo { get; set; } = default!;

    [Column("cantidad", TypeName = "numeric(14,3)")]
    public decimal Cantidad { get; set; }

    [Column("costo_unitario", TypeName = "numeric(12,4)")]
    public decimal? CostoUnitario { get; set; }

    [Column("tercero_id")]
    public Guid? TerceroId { get; set; }

    [Column("pedido_id")]
    public Guid? PedidoId { get; set; }

    [Column("gasto_id")]
    public Guid? GastoId { get; set; }

    [Column("usuario_id")]
    public Guid? UsuarioId { get; set; }

    [Column("referencia")]
    public string? Referencia { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}