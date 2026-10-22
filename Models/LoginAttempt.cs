namespace BackendGeneral.Api.Models;

public class LoginAttempt
{
    public long Id { get; set; }
    public Guid? UserId { get; set; }
    public Usuario? Usuario { get; set; }
    public string UsernameOrEmail { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}