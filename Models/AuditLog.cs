namespace BackendGeneral.Api.Models;

public class AuditLog
{
    public long Id { get; set; }
    public Guid? UserId { get; set; }
    public Usuario? Usuario { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? EntityName { get; set; }
    public Guid? EntityId { get; set; }
    public string? Description { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}