using BackendGeneral.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BackendGeneral.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // ─── Tablas existentes ───
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Permiso> Permisos => Set<Permiso>();
    public DbSet<Modulo> Modulos => Set<Modulo>();
    public DbSet<UsuarioRol> UsuarioRoles => Set<UsuarioRol>();
    public DbSet<RolPermiso> RolPermisos => Set<RolPermiso>();
    public DbSet<RolModulo> RolModulos => Set<RolModulo>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // ─── Nuevas tablas ───
    public DbSet<LoginAttempt> LoginAttempts => Set<LoginAttempt>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();

    // ─── Ventas ───
    public DbSet<Venta> Ventas => Set<Venta>();

    // ─── Proyectos / Curva S ───
public DbSet<Project> Projects => Set<Project>();
public DbSet<ProjectTask> ProjectTasks => Set<ProjectTask>();
public DbSet<ProjectSCurvePoint> ProjectSCurvePoints => Set<ProjectSCurvePoint>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        // USERS
        mb.Entity<Usuario>(e =>
        {
            e.ToTable("users");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Username).HasColumnName("username").HasMaxLength(50).IsRequired();
            e.Property(x => x.Email).HasColumnName("email").HasMaxLength(150).IsRequired();
            e.Property(x => x.PasswordHash).HasColumnName("password_hash").IsRequired();
            e.Property(x => x.FirstName).HasColumnName("first_name").HasMaxLength(100).IsRequired();
            e.Property(x => x.LastName).HasColumnName("last_name").HasMaxLength(100);
            e.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(30);
            e.Property(x => x.AvatarUrl).HasColumnName("avatar_url");
            e.Property(x => x.IsActive).HasColumnName("is_active");
            e.Property(x => x.IsLocked).HasColumnName("is_locked");
            e.Property(x => x.FailedLoginAttempts).HasColumnName("failed_login_attempts");
            e.Property(x => x.LastLoginAt).HasColumnName("last_login_at");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            e.HasIndex(x => x.Email).IsUnique();
            e.HasIndex(x => x.Username).IsUnique();
        });

        // ROLES
        mb.Entity<Rol>(e =>
        {
            e.ToTable("roles");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            e.Property(x => x.Code).HasColumnName("code").HasMaxLength(100).IsRequired();
            e.Property(x => x.Description).HasColumnName("description");
            e.Property(x => x.IsActive).HasColumnName("is_active");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            e.HasIndex(x => x.Code).IsUnique();
        });

        // PERMISSIONS
        mb.Entity<Permiso>(e =>
        {
            e.ToTable("permissions");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
            e.Property(x => x.Code).HasColumnName("code").HasMaxLength(150).IsRequired();
            e.Property(x => x.Description).HasColumnName("description");
            e.Property(x => x.IsActive).HasColumnName("is_active");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            e.HasIndex(x => x.Code).IsUnique();
        });

        // MODULES
        mb.Entity<Modulo>(e =>
        {
            e.ToTable("modules");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            e.Property(x => x.Code).HasColumnName("code").HasMaxLength(100).IsRequired();
            e.Property(x => x.Description).HasColumnName("description");
            e.Property(x => x.Icon).HasColumnName("icon").HasMaxLength(100);
            e.Property(x => x.Route).HasColumnName("route").HasMaxLength(255);
            e.Property(x => x.ParentId).HasColumnName("parent_id");
            e.Property(x => x.OrderIndex).HasColumnName("order_index");
            e.Property(x => x.IsMenuVisible).HasColumnName("is_menu_visible");
            e.Property(x => x.IsActive).HasColumnName("is_active");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            e.HasIndex(x => x.Code).IsUnique();
            e.HasOne(x => x.Parent).WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.SetNull);
        });

        // USER_ROLES
        mb.Entity<UsuarioRol>(e =>
        {
            e.ToTable("user_roles");
            e.HasKey(x => new { x.UserId, x.RoleId });
            e.Property(x => x.UserId).HasColumnName("user_id");
            e.Property(x => x.RoleId).HasColumnName("role_id");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.HasOne(x => x.Usuario).WithMany(x => x.UsuarioRoles)
                .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Rol).WithMany(x => x.UsuarioRoles)
                .HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Cascade);
        });

        // ROLE_PERMISSIONS
        mb.Entity<RolPermiso>(e =>
        {
            e.ToTable("role_permissions");
            e.HasKey(x => new { x.RoleId, x.PermissionId });
            e.Property(x => x.RoleId).HasColumnName("role_id");
            e.Property(x => x.PermissionId).HasColumnName("permission_id");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.HasOne(x => x.Rol).WithMany(x => x.RolPermisos)
                .HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Permiso).WithMany(x => x.RolPermisos)
                .HasForeignKey(x => x.PermissionId).OnDelete(DeleteBehavior.Cascade);
        });

        // ROLE_MODULES
        mb.Entity<RolModulo>(e =>
        {
            e.ToTable("role_modules");
            e.HasKey(x => new { x.RoleId, x.ModuleId });
            e.Property(x => x.RoleId).HasColumnName("role_id");
            e.Property(x => x.ModuleId).HasColumnName("module_id");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.HasOne(x => x.Rol).WithMany(x => x.RolModulos)
                .HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Modulo).WithMany(x => x.RolModulos)
                .HasForeignKey(x => x.ModuleId).OnDelete(DeleteBehavior.Cascade);
        });

        // REFRESH_TOKENS
        mb.Entity<RefreshToken>(e =>
        {
            e.ToTable("refresh_tokens");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.UserId).HasColumnName("user_id");
            e.Property(x => x.TokenHash).HasColumnName("token_hash").IsRequired();
            e.Property(x => x.ExpiresAt).HasColumnName("expires_at");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.RevokedAt).HasColumnName("revoked_at");
            e.Property(x => x.ReplacedByTokenId).HasColumnName("replaced_by_token_id");
            e.HasIndex(x => x.TokenHash).IsUnique();
            e.HasOne(x => x.Usuario).WithMany(x => x.RefreshTokens)
                .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.ReplacedByToken).WithMany()
                .HasForeignKey(x => x.ReplacedByTokenId).OnDelete(DeleteBehavior.SetNull);
        });

        // AUDIT_LOGS
        mb.Entity<AuditLog>(e =>
        {
            e.ToTable("audit_logs");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.UserId).HasColumnName("user_id");
            e.Property(x => x.Action).HasColumnName("action").HasMaxLength(100).IsRequired();
            e.Property(x => x.EntityName).HasColumnName("entity_name").HasMaxLength(100);
            e.Property(x => x.EntityId).HasColumnName("entity_id");
            e.Property(x => x.Description).HasColumnName("description");
            e.Property(x => x.IpAddress).HasColumnName("ip_address").HasMaxLength(45);
            e.Property(x => x.UserAgent).HasColumnName("user_agent");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.HasOne(x => x.Usuario).WithMany(x => x.AuditLogs)
                .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull);
        });

        // LOGIN_ATTEMPTS
        mb.Entity<LoginAttempt>(e =>
        {
            e.ToTable("login_attempts");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.UserId).HasColumnName("user_id");
            e.Property(x => x.UsernameOrEmail).HasColumnName("username_or_email").HasMaxLength(200).IsRequired();
            e.Property(x => x.Success).HasColumnName("success");
            e.Property(x => x.IpAddress).HasColumnName("ip_address").HasMaxLength(45);
            e.Property(x => x.UserAgent).HasColumnName("user_agent");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.HasOne(x => x.Usuario).WithMany()
                .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull);
        });

        // PASSWORD_RESET_TOKENS
        mb.Entity<PasswordResetToken>(e =>
        {
            e.ToTable("password_reset_tokens");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.UserId).HasColumnName("user_id");
            e.Property(x => x.TokenHash).HasColumnName("token_hash").IsRequired();
            e.Property(x => x.ExpiresAt).HasColumnName("expires_at");
            e.Property(x => x.UsedAt).HasColumnName("used_at");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.HasIndex(x => x.TokenHash).IsUnique();
            e.HasOne(x => x.Usuario).WithMany()
                .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        // SYSTEM_SETTINGS
        mb.Entity<SystemSetting>(e =>
        {
            e.ToTable("system_settings");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Key).HasColumnName("key").HasMaxLength(150).IsRequired();
            e.Property(x => x.Value).HasColumnName("value");
            e.Property(x => x.Description).HasColumnName("description");
            e.Property(x => x.IsPublic).HasColumnName("is_public");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            e.HasIndex(x => x.Key).IsUnique();
        });

        // ═════════════════════════════════════════════════════════
        // NUEVO: VENTAS
        // ═════════════════════════════════════════════════════════
        mb.Entity<Venta>(e =>
        {
            e.ToTable("ventas");
            e.HasKey(x => x.Id);

            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.UsuarioId).HasColumnName("usuario_id");
            e.Property(x => x.Numero).HasColumnName("numero").HasMaxLength(50).IsRequired();
            e.Property(x => x.Cliente).HasColumnName("cliente").HasMaxLength(200).IsRequired();
            e.Property(x => x.Total).HasColumnName("total").HasColumnType("numeric");
            e.Property(x => x.Estado).HasColumnName("estado").HasMaxLength(20).IsRequired();
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            e.HasIndex(x => x.Numero).IsUnique();
            e.HasIndex(x => x.UsuarioId);

            e.HasOne(x => x.Usuario).WithMany()
                .HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Restrict);
        });

        // ═════════════════════════════════════════════════════════
// NUEVO: PROYECTOS Y ACTIVIDADES
// ═════════════════════════════════════════════════════════
mb.Entity<Project>(e =>
{
    e.ToTable("projects");
    e.HasKey(x => x.Id);
    e.Property(x => x.Id).HasColumnName("id");
    e.Property(x => x.Code).HasColumnName("code").HasMaxLength(50);
    e.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
    e.Property(x => x.Description).HasColumnName("description");
    e.Property(x => x.StartDate).HasColumnName("start_date");
    e.Property(x => x.EndDate).HasColumnName("end_date");
    e.Property(x => x.Status).HasColumnName("status").HasMaxLength(30);
    e.HasIndex(x => x.Code);
});

mb.Entity<ProjectTask>(e =>
{
    e.ToTable("project_tasks");
    e.HasKey(x => x.Id);
    e.Property(x => x.Id).HasColumnName("id");
    e.Property(x => x.ProjectId).HasColumnName("project_id");
    e.Property(x => x.Code).HasColumnName("code").HasMaxLength(50);
    e.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
    e.Property(x => x.Weight).HasColumnName("weight").HasColumnType("numeric(5,2)");
    e.Property(x => x.PlannedStart).HasColumnName("planned_start");
    e.Property(x => x.PlannedEnd).HasColumnName("planned_end");
    e.Property(x => x.ActualStart).HasColumnName("actual_start");
    e.Property(x => x.ActualEnd).HasColumnName("actual_end");
    e.Property(x => x.Progress).HasColumnName("progress").HasColumnType("numeric(5,2)");
    e.Property(x => x.Status).HasColumnName("status").HasMaxLength(30);

    e.HasOne<Project>()
     .WithMany()
     .HasForeignKey(x => x.ProjectId)
     .OnDelete(DeleteBehavior.Cascade);

    e.HasIndex(x => x.ProjectId);
});

// ═════════════════════════════════════════════════════════
// NUEVO: VISTA CURVA S (sin clave, solo lectura)
// ═════════════════════════════════════════════════════════
mb.Entity<ProjectSCurvePoint>(e =>
{
    e.HasNoKey();
    e.ToView("v_project_scurve");

    e.Property(x => x.ProjectId).HasColumnName("project_id");
    e.Property(x => x.MonthStart).HasColumnName("month_start");
    e.Property(x => x.MonthKey).HasColumnName("month_key");
    e.Property(x => x.MonthLabel).HasColumnName("month_label");
    e.Property(x => x.Planned).HasColumnName("planned").HasColumnType("numeric");
    e.Property(x => x.Actual).HasColumnName("actual").HasColumnType("numeric");
});
    }
}