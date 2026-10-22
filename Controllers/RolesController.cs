using BackendGeneral.Api.Data;
using BackendGeneral.Api.DTOs;
using BackendGeneral.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendGeneral.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly AppDbContext _db;

    public RolesController(AppDbContext db) => _db = db;

    // ═══════════════════ CRUD BÁSICO ═══════════════════

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RolDto>>> GetAll()
    {
        var list = await _db.Roles
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .Select(r => new RolDto
            {
                Id = r.Id,
                Name = r.Name,
                Code = r.Code,
                Description = r.Description,
                IsActive = r.IsActive
            })
            .ToListAsync();

        return Ok(list);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RolDto>> GetById(Guid id)
    {
        var r = await _db.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (r == null)
            return NotFound("No se encontró el rol.");

        return Ok(new RolDto
        {
            Id = r.Id,
            Name = r.Name,
            Code = r.Code,
            Description = r.Description,
            IsActive = r.IsActive
        });
    }

    [HttpPost]
    public async Task<ActionResult<RolDto>> Create(CrearRolDto dto)
    {
        if (dto == null)
            return BadRequest("Los datos del rol son obligatorios.");

        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("El nombre del rol es obligatorio.");

        if (string.IsNullOrWhiteSpace(dto.Code))
            return BadRequest("El código del rol es obligatorio.");

        var name = dto.Name.Trim();
        var code = dto.Code.Trim();

        if (await _db.Roles.AnyAsync(r => r.Code == code))
            return Conflict("Ya existe un rol con ese código.");

        var rol = new Rol
        {
            Name = name,
            Code = code,
            Description = string.IsNullOrWhiteSpace(dto.Description)
                ? null
                : dto.Description.Trim()
        };

        _db.Roles.Add(rol);

        await _db.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = rol.Id },
            rol.Id
        );
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, ActualizarRolDto dto)
    {
        if (dto == null)
            return BadRequest("Los datos del rol son obligatorios.");

        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("El nombre del rol es obligatorio.");

        if (string.IsNullOrWhiteSpace(dto.Code))
            return BadRequest("El código del rol es obligatorio.");

        var rol = await _db.Roles.FindAsync(id);

        if (rol == null)
            return NotFound("No se encontró el rol.");

        var name = dto.Name.Trim();
        var code = dto.Code.Trim();

        var codeExists = await _db.Roles
            .AnyAsync(r => r.Code == code && r.Id != id);

        if (codeExists)
            return Conflict("Ya existe otro rol con ese código.");

        rol.Name = name;
        rol.Code = code;
        rol.Description = string.IsNullOrWhiteSpace(dto.Description)
            ? null
            : dto.Description.Trim();
        rol.IsActive = dto.IsActive;
        rol.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var rol = await _db.Roles.FindAsync(id);

        if (rol == null)
            return NotFound("No se encontró el rol.");

        var tienePermisos = await _db.RolPermisos
            .AnyAsync(rp => rp.RoleId == id);

        var tieneModulos = await _db.RolModulos
            .AnyAsync(rm => rm.RoleId == id);

        if (tienePermisos || tieneModulos)
        {
            return Conflict(
                "No se puede eliminar el rol porque tiene permisos o módulos asignados."
            );
        }

        _db.Roles.Remove(rol);

        await _db.SaveChangesAsync();

        return NoContent();
    }

    // ═══════════════════ DETALLE COMPLETO ═══════════════════

    [HttpGet("{id:guid}/detalle")]
    public async Task<ActionResult<RolDetalleDto>> GetDetalle(Guid id)
    {
        var rol = await _db.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (rol == null)
            return NotFound("No se encontró el rol.");

        var permIds = await _db.RolPermisos
            .AsNoTracking()
            .Where(rp => rp.RoleId == id)
            .Select(rp => rp.PermissionId)
            .ToListAsync();

        var modIds = await _db.RolModulos
            .AsNoTracking()
            .Where(rm => rm.RoleId == id)
            .Select(rm => rm.ModuleId)
            .ToListAsync();

        return Ok(new RolDetalleDto
        {
            Id = rol.Id,
            Name = rol.Name,
            Code = rol.Code,
            Description = rol.Description,
            IsActive = rol.IsActive,
            PermissionIds = permIds,
            ModuleIds = modIds
        });
    }

    // ═══════════════════ PERMISOS DEL ROL ═══════════════════

    [HttpGet("{id:guid}/permisos")]
    public async Task<IActionResult> GetPermisos(Guid id)
    {
        var rolExiste = await _db.Roles
            .AsNoTracking()
            .AnyAsync(r => r.Id == id);

        if (!rolExiste)
            return NotFound("No se encontró el rol.");

        var permisos = await _db.RolPermisos
            .AsNoTracking()
            .Where(rp => rp.RoleId == id)
            .Select(rp => new
            {
                rp.Permiso.Id,
                rp.Permiso.Code,
                rp.Permiso.Name
            })
            .OrderBy(p => p.Name)
            .ToListAsync();

        return Ok(permisos);
    }

    [HttpPut("{id:guid}/permisos")]
    public async Task<IActionResult> ReplacePermisos(
        Guid id,
        AsignarPermisosDto dto)
    {
        if (dto == null)
            return BadRequest("Los datos de permisos son obligatorios.");

        var rol = await _db.Roles.FindAsync(id);

        if (rol == null)
            return NotFound("No se encontró el rol.");

        var permissionIds = dto.PermissionIds
            .Distinct()
            .ToList();

        if (permissionIds.Count > 0)
        {
            var permisosExistentes = await _db.Permisos
                .Where(p => permissionIds.Contains(p.Id))
                .Select(p => p.Id)
                .ToListAsync();

            var permisosInvalidos = permissionIds
                .Except(permisosExistentes)
                .ToList();

            if (permisosInvalidos.Count > 0)
            {
                return BadRequest(
                    "Uno o más permisos no existen."
                );
            }
        }

        var actuales = await _db.RolPermisos
            .Where(rp => rp.RoleId == id)
            .ToListAsync();

        _db.RolPermisos.RemoveRange(actuales);

        foreach (var pid in permissionIds)
        {
            _db.RolPermisos.Add(new RolPermiso
            {
                RoleId = id,
                PermissionId = pid
            });
        }

        await _db.SaveChangesAsync();

        return NoContent();
    }

    // ═══════════════════ MÓDULOS DEL ROL ═══════════════════

    [HttpGet("{id:guid}/modulos")]
    public async Task<IActionResult> GetModulos(Guid id)
    {
        var rolExiste = await _db.Roles
            .AsNoTracking()
            .AnyAsync(r => r.Id == id);

        if (!rolExiste)
            return NotFound("No se encontró el rol.");

        var modulos = await _db.RolModulos
            .AsNoTracking()
            .Where(rm => rm.RoleId == id)
            .Select(rm => new
            {
                rm.Modulo.Id,
                rm.Modulo.Code,
                rm.Modulo.Name,
                rm.Modulo.Route
            })
            .OrderBy(m => m.Name)
            .ToListAsync();

        return Ok(modulos);
    }

    [HttpPut("{id:guid}/modulos")]
    public async Task<IActionResult> ReplaceModulos(
        Guid id,
        AsignarModulosDto dto)
    {
        if (dto == null)
            return BadRequest("Los datos de módulos son obligatorios.");

        var rol = await _db.Roles.FindAsync(id);

        if (rol == null)
            return NotFound("No se encontró el rol.");

        var moduleIds = dto.ModuleIds
            .Distinct()
            .ToList();

        if (moduleIds.Count > 0)
        {
            var modulosExistentes = await _db.Modulos
                .Where(m => moduleIds.Contains(m.Id))
                .Select(m => m.Id)
                .ToListAsync();

            var modulosInvalidos = moduleIds
                .Except(modulosExistentes)
                .ToList();

            if (modulosInvalidos.Count > 0)
            {
                return BadRequest(
                    "Uno o más módulos no existen."
                );
            }
        }

        var actuales = await _db.RolModulos
            .Where(rm => rm.RoleId == id)
            .ToListAsync();

        _db.RolModulos.RemoveRange(actuales);

        foreach (var mid in moduleIds)
        {
            _db.RolModulos.Add(new RolModulo
            {
                RoleId = id,
                ModuleId = mid
            });
        }

        await _db.SaveChangesAsync();

        return NoContent();
    }
}