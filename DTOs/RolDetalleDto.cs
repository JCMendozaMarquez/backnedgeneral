namespace BackendGeneral.Api.DTOs;

public class RolDetalleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }

    public List<Guid> PermissionIds { get; set; } = new();
    public List<Guid> ModuleIds { get; set; } = new();
}

public class AsignarPermisosDto
{
    public List<Guid> PermissionIds { get; set; } = new();
}

public class AsignarModulosDto
{
    public List<Guid> ModuleIds { get; set; } = new();
}

public class AsignarRolesDto
{
    public List<Guid> RoleIds { get; set; } = new();
}