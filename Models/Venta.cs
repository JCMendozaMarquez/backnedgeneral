// Models/Venta.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendGeneral.Api.Models
{
    [Table("ventas")]
    public class Venta
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("usuario_id")]
        public Guid UsuarioId { get; set; }

        [Column("numero")]
        [MaxLength(50)]
        public string Numero { get; set; } = string.Empty;

        [Column("cliente")]
        [MaxLength(200)]
        public string Cliente { get; set; } = string.Empty;

        [Column("total")]
        public decimal Total { get; set; }

        [Column("estado")]
        [MaxLength(20)]
        public string Estado { get; set; } = "PENDIENTE";

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navegación
        public Usuario? Usuario { get; set; }
    }
}