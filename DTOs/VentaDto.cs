// DTO/VentaDto.cs
namespace BackendGeneral.Api.DTO
{
    public class VentaDto
    {
        public Guid Id { get; set; }
        public Guid UsuarioId { get; set; }
        public string Numero { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}