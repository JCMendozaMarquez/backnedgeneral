namespace BackendGeneral.Api.Models;

public class Modulo
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? Route { get; set; }
    public Guid? ParentId { get; set; }
    public int OrderIndex { get; set; } = 0;
    public bool IsMenuVisible { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Modulo? Parent { get; set; }
    public ICollection<Modulo> Children { get; set; } = new List<Modulo>();
    public ICollection<RolModulo> RolModulos { get; set; } = new List<RolModulo>();
}