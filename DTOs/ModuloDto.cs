namespace BackendGeneral.Api.DTOs;

public class ModuloDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? Route { get; set; }
    public Guid? ParentId { get; set; }
    public int OrderIndex { get; set; }
    public bool IsMenuVisible { get; set; }
    public bool IsActive { get; set; }
}

public class CrearModuloDto
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? Route { get; set; }
    public Guid? ParentId { get; set; }
    public int OrderIndex { get; set; } = 0;
    public bool IsMenuVisible { get; set; } = true;
}

public class ActualizarModuloDto
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? Route { get; set; }
    public Guid? ParentId { get; set; }
    public int OrderIndex { get; set; }
    public bool IsMenuVisible { get; set; }
    public bool IsActive { get; set; }
}