namespace BackendGeneral.Api.Models;

public class RolModulo
{
    public Guid RoleId { get; set; }
    public Rol Rol { get; set; } = null!;
    public Guid ModuleId { get; set; }
    public Modulo Modulo { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}