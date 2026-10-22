namespace BackendGeneral.Api.Models;

public class UsuarioRol
{
    public Guid UserId { get; set; }
    public Usuario Usuario { get; set; } = null!;
    public Guid RoleId { get; set; }
    public Rol Rol { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}