// DTO/ActualizarVentaDto.cs
using System.ComponentModel.DataAnnotations;

namespace BackendGeneral.Api.DTO
{
    public class ActualizarVentaDto
    {
        [MaxLength(50)]
        public string? Numero { get; set; }

        [MaxLength(200)]
        public string? Cliente { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? Total { get; set; }

        [MaxLength(20)]
        public string? Estado { get; set; }
    }
}