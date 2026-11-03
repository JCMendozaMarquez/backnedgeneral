using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendGeneral.Api.Models;

[Table("mensajes")]
public class Mensaje
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("canal")]
    [MaxLength(20)]
    public string Canal { get; set; } = "whatsapp";

    [Column("direccion")]
    [MaxLength(20)]
    public string Direccion { get; set; } = default!;

    [Column("telefono")]
    public string? Telefono { get; set; }

    [Column("pedido_id")]
    public Guid? PedidoId { get; set; }

    [Column("tercero_id")]
    public Guid? TerceroId { get; set; }

    [Column("tipo")]
    [MaxLength(20)]
    public string Tipo { get; set; } = "pedido";

    [Column("contenido")]
    public string? Contenido { get; set; }

    [Column("estado")]
    [MaxLength(20)]
    public string Estado { get; set; } = "pendiente";

    [Column("id_externo")]
    public string? IdExterno { get; set; }

    [Column("datos", TypeName = "jsonb")]
    public string Datos { get; set; } = "{}";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("enviado_en")]
    public DateTime? EnviadoEn { get; set; }
}