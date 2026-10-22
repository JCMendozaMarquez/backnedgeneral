using BackendGeneral.Api.Data;
using BackendGeneral.Api.DTOs;
using BackendGeneral.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BackendGeneral.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Requiere permiso para gestionar usuarios
public class UsuariosController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<UsuariosController> _logger;

    public UsuariosController(AppDbContext db, ILogger<UsuariosController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UsuarioDto>>> GetAll()
    {
        var list = await _db.Usuarios.AsNoTracking()
            .Select(u => new UsuarioDto
            {
                Id = u.Id, Username = u.Username, Email = u.Email,
                FirstName = u.FirstName, LastName = u.LastName,
                Phone = u.Phone, AvatarUrl = u.AvatarUrl,
                IsActive = u.IsActive, IsLocked = u.IsLocked,
                LastLoginAt = u.LastLoginAt, CreatedAt = u.CreatedAt
            }).ToListAsync();
        return Ok(list);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UsuarioDto>> GetById(Guid id)
    {
        var u = await _db.Usuarios.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (u == null) return NotFound();
        return Ok(new UsuarioDto
        {
            Id = u.Id, Username = u.Username, Email = u.Email,
            FirstName = u.FirstName, LastName = u.LastName,
            Phone = u.Phone, AvatarUrl = u.AvatarUrl,
            IsActive = u.IsActive, IsLocked = u.IsLocked,
            LastLoginAt = u.LastLoginAt, CreatedAt = u.CreatedAt
        });
    }

    [HttpPost]
    public async Task<ActionResult<UsuarioDto>> Create(CrearUsuarioDto dto)
    {
        if (await _db.Usuarios.AnyAsync(u => u.Email == dto.Email || u.Username == dto.Username))
            return Conflict("Ya existe un usuario con ese email o username");

        var user = new Usuario
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Phone = dto.Phone
        };

        _db.Usuarios.Add(user);
        await _db.SaveChangesAsync();

        var result = new UsuarioDto
        {
            Id = user.Id, Username = user.Username, Email = user.Email,
            FirstName = user.FirstName, LastName = user.LastName,
            Phone = user.Phone, IsActive = user.IsActive,
            IsLocked = user.IsLocked, CreatedAt = user.CreatedAt
        };
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, ActualizarUsuarioDto dto)
    {
        var user = await _db.Usuarios.FindAsync(id);
        if (user == null) return NotFound();

        user.Email = dto.Email;
        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.Phone = dto.Phone;
        user.IsActive = dto.IsActive;
        user.IsLocked = dto.IsLocked;
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        // No permitir que un admin se elimine a sí mismo
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (currentUserId == id.ToString())
            return BadRequest(new { message = "No puedes eliminarte a ti mismo." });

        var user = await _db.Usuarios.FindAsync(id);
        if (user == null) return NotFound();

        _db.Usuarios.Remove(user);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("{id:guid}/roles")]
    public async Task<IActionResult> GetRoles(Guid id)
    {
        var roles = await _db.UsuarioRoles
            .Where(ur => ur.UserId == id)
            .Select(ur => new { ur.Rol.Id, ur.Rol.Code, ur.Rol.Name })
            .ToListAsync();

        return Ok(roles);
    }

    [HttpPut("{id:guid}/roles")]
    public async Task<IActionResult> ReplaceRoles(Guid id, AsignarRolesDto dto)
    {
        var user = await _db.Usuarios.FindAsync(id);
        if (user == null) return NotFound();

        //  No permitir que un admin se quite ADMIN a sí mismo
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (currentUserId == id.ToString())
        {
            var adminRole = await _db.Roles.FirstOrDefaultAsync(r => r.Code == "ADMIN");
            if (adminRole != null && !dto.RoleIds.Contains(adminRole.Id))
                return BadRequest(new { message = "No puedes quitarte el rol ADMIN a ti mismo." });
        }

        //  Validar que todos los roles existen y están activos
        var rolesValidos = await _db.Roles
            .Where(r => dto.RoleIds.Contains(r.Id) && r.IsActive)
            .Select(r => r.Id)
            .ToListAsync();

        if (rolesValidos.Count != dto.RoleIds.Distinct().Count())
            return BadRequest(new { message = "Uno o más roles no existen o están inactivos." });

        // Reemplazar roles
        var actuales = await _db.UsuarioRoles.Where(ur => ur.UserId == id).ToListAsync();
        _db.UsuarioRoles.RemoveRange(actuales);

        foreach (var rid in dto.RoleIds.Distinct())
        {
            _db.UsuarioRoles.Add(new UsuarioRol
            {
                UserId = id,
                RoleId = rid,
            });
        }

        await _db.SaveChangesAsync();

        //  Revocar refresh tokens activos para forzar re-login
        var tokensActivos = await _db.RefreshTokens
            .Where(rt => rt.UserId == id && rt.RevokedAt == null)
            .ToListAsync();

        foreach (var rt in tokensActivos)
            rt.RevokedAt = DateTime.UtcNow;

        if (tokensActivos.Count > 0)
            await _db.SaveChangesAsync();

        _logger.LogInformation(
            "Roles actualizados para {Usuario}. Tokens revocados: {Count}",
            user.Username, tokensActivos.Count);

        return NoContent();
    }
}