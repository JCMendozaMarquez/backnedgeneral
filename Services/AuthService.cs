using BackendGeneral.Api.Data;
using BackendGeneral.Api.DTOs;
using BackendGeneral.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BackendGeneral.Api.Services;

public class AuthService
{
    private readonly AppDbContext _db;
    private readonly JwtService _jwt;

    private const string DefaultRoleCode = "OPERATOR";

    public AuthService(AppDbContext db, JwtService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    // =====================================================
    // Helper: permisos del usuario (LINQ puro, sin SQL raw)
    // =====================================================
    private async Task<List<string>> GetPermissionsAsync(Guid userId)
    {
        return await _db.UsuarioRoles
            .Where(ur => ur.UserId == userId)
            .Join(_db.RolPermisos, ur => ur.RoleId, rp => rp.RoleId, (ur, rp) => rp)
            .Join(_db.Permisos, rp => rp.PermissionId, p => p.Id, (rp, p) => p)
            .Where(p => p.IsActive)
            .Select(p => p.Code)
            .Distinct()
            .ToListAsync();
    }

    // =====================================================
    // Helper: resolver rol por nombre/código/guid (flexible)
    // =====================================================
    private async Task<Rol?> ResolveRoleAsync(string? roleInput)
    {
        if (string.IsNullOrWhiteSpace(roleInput))
        {
            return await _db.Roles
                .FirstOrDefaultAsync(r => r.Code == DefaultRoleCode && r.IsActive);
        }

        var value = roleInput.Trim();

        var byCode = await _db.Roles
            .FirstOrDefaultAsync(r => r.IsActive && r.Code.ToLower() == value.ToLower());
        if (byCode != null) return byCode;

        var byName = await _db.Roles
            .FirstOrDefaultAsync(r => r.IsActive && r.Name.ToLower() == value.ToLower());
        if (byName != null) return byName;

        if (Guid.TryParse(value, out var guid))
        {
            var byId = await _db.Roles
                .FirstOrDefaultAsync(r => r.IsActive && r.Id == guid);
            if (byId != null) return byId;
        }

        return await _db.Roles
            .FirstOrDefaultAsync(r => r.Code == DefaultRoleCode && r.IsActive);
    }

    // =====================================================
    // LOGIN
    // =====================================================
    public async Task<(bool ok, string? error, LoginResponseDto? data)> LoginAsync(
        LoginRequestDto dto, string? ip, string? userAgent)
    {
        var user = await _db.Usuarios
            .FirstOrDefaultAsync(u => u.Email == dto.EmailOrUsername
                                   || u.Username == dto.EmailOrUsername);

        var attempt = new LoginAttempt
        {
            UserId = user?.Id,
            UsernameOrEmail = dto.EmailOrUsername,
            Success = false,
            IpAddress = ip,
            UserAgent = userAgent,
        };
        _db.LoginAttempts.Add(attempt);

        if (user == null)
        {
            await _db.SaveChangesAsync();
            return (false, "Credenciales inválidas", null);
        }

        if (!user.IsActive || user.IsLocked)
        {
            await _db.SaveChangesAsync();
            return (false, "Usuario inactivo o bloqueado", null);
        }

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            user.FailedLoginAttempts++;
            if (user.FailedLoginAttempts >= 5)
                user.IsLocked = true;

            await _db.SaveChangesAsync();
            return (false, "Credenciales inválidas", null);
        }

        user.FailedLoginAttempts = 0;
        user.LastLoginAt = DateTime.UtcNow;
        attempt.Success = true;

        // ✅ Sin SQL raw
        var permissions = await GetPermissionsAsync(user.Id);

        var accessToken = _jwt.GenerateAccessToken(user, permissions);
        var (rawRefresh, hashRefresh) = _jwt.GenerateRefreshToken();

        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = hashRefresh,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDays),
        });

        _db.AuditLogs.Add(new AuditLog
        {
            UserId = user.Id,
            Action = "LOGIN",
            EntityName = "users",
            EntityId = user.Id,
            Description = "Inicio de sesión exitoso",
            IpAddress = ip,
            UserAgent = userAgent,
        });

        await _db.SaveChangesAsync();

        return (true, null, new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = rawRefresh,
            ExpiresIn = _jwt.AccessTokenMinutes * 60,
            Permissions = permissions,
            User = new UserInfoDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
            },
        });
    }

    // =====================================================
    // REGISTER
    // =====================================================
    public async Task<(bool ok, string? error, LoginResponseDto? data)> RegisterAsync(
        RegisterRequestDto dto, string? ip, string? userAgent)
    {
        if (await _db.Usuarios.AnyAsync(u => u.Email == dto.Email || u.Username == dto.Username))
            return (false, "Email o username ya registrado", null);

        var user = new Usuario
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Phone = dto.Phone,
            IsActive = true,
            IsLocked = false,
        };

        _db.Usuarios.Add(user);
        await _db.SaveChangesAsync();

        // Asignar rol (flexible: código, nombre, guid, vacío → OPERATOR)
        var role = await ResolveRoleAsync(dto.DefaultRoleCode);
        if (role != null)
        {
            var already = await _db.UsuarioRoles
                .AnyAsync(ur => ur.UserId == user.Id && ur.RoleId == role.Id);

            if (!already)
            {
                _db.UsuarioRoles.Add(new UsuarioRol
                {
                    UserId = user.Id,
                    RoleId = role.Id,
                });
                await _db.SaveChangesAsync();
            }
        }

        // Auditoría
        _db.AuditLogs.Add(new AuditLog
        {
            UserId = user.Id,
            Action = "REGISTER",
            EntityName = "users",
            EntityId = user.Id,
            Description = $"Registro de nuevo usuario (rol: {role?.Code ?? "ninguno"})",
            IpAddress = ip,
            UserAgent = userAgent,
        });
        await _db.SaveChangesAsync();

        // ✅ Sin SQL raw
        var permissions = await GetPermissionsAsync(user.Id);

        var accessToken = _jwt.GenerateAccessToken(user, permissions);
        var (rawRefresh, hashRefresh) = _jwt.GenerateRefreshToken();

        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = hashRefresh,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDays),
        });
        await _db.SaveChangesAsync();

        return (true, null, new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = rawRefresh,
            ExpiresIn = _jwt.AccessTokenMinutes * 60,
            Permissions = permissions,
            User = new UserInfoDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
            },
        });
    }

    // =====================================================
    // REFRESH
    // =====================================================
    public async Task<(bool ok, string? error, LoginResponseDto? data)> RefreshAsync(
        string rawRefreshToken, string? ip, string? userAgent)
    {
        var hash = _jwt.HashToken(rawRefreshToken);

        var stored = await _db.RefreshTokens
            .Include(t => t.Usuario)
            .FirstOrDefaultAsync(t => t.TokenHash == hash);

        if (stored == null || stored.RevokedAt != null || stored.ExpiresAt < DateTime.UtcNow)
            return (false, "Refresh token inválido o expirado", null);

        var user = stored.Usuario;
        if (user == null || !user.IsActive || user.IsLocked)
            return (false, "Usuario inactivo", null);

        stored.RevokedAt = DateTime.UtcNow;

        var (rawNew, hashNew) = _jwt.GenerateRefreshToken();
        var newToken = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = hashNew,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDays),
        };
        _db.RefreshTokens.Add(newToken);
        await _db.SaveChangesAsync();

        stored.ReplacedByTokenId = newToken.Id;
        await _db.SaveChangesAsync();

        // ✅ Sin SQL raw
        var permissions = await GetPermissionsAsync(user.Id);
        var access = _jwt.GenerateAccessToken(user, permissions);

        return (true, null, new LoginResponseDto
        {
            AccessToken = access,
            RefreshToken = rawNew,
            ExpiresIn = _jwt.AccessTokenMinutes * 60,
            Permissions = permissions,
            User = new UserInfoDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
            },
        });
    }

    // =====================================================
    // LOGOUT
    // =====================================================
    public async Task LogoutAsync(string rawRefreshToken)
    {
        var hash = _jwt.HashToken(rawRefreshToken);
        var stored = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash);
        if (stored != null && stored.RevokedAt == null)
        {
            stored.RevokedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }
}