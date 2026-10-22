// DTO/CrearVentaDto.cs
using System.ComponentModel.DataAnnotations;

namespace BackendGeneral.Api.DTO
{
    public class CrearVentaDto
    {
        [Required(ErrorMessage = "El número de venta es obligatorio")]
        [MaxLength(50)]
        public string Numero { get; set; } = string.Empty;

        [Required(ErrorMessage = "El cliente es obligatorio")]
        [MaxLength(200)]
        public string Cliente { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "El total debe ser mayor o igual a 0")]
        public decimal Total { get; set; }

        [MaxLength(20)]
        public string Estado { get; set; } = "PENDIENTE";
    }
}