namespace BackendGeneral.Api.Models;

public class RolPermiso
{
    public Guid RoleId { get; set; }
    public Rol Rol { get; set; } = null!;
    public Guid PermissionId { get; set; }
    public Permiso Permiso { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}