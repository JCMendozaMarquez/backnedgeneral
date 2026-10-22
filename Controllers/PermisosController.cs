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
//[Authorize]
public class PermisosController : ControllerBase
{
    private readonly AppDbContext _db;
    public PermisosController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PermisoDto>>> GetAll()
    {
        var list = await _db.Permisos.AsNoTracking()
            .Select(p => new PermisoDto
            {
                Id = p.Id, Name = p.Name, Code = p.Code,
                Description = p.Description, IsActive = p.IsActive
            }).ToListAsync();
        return Ok(list);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PermisoDto>> GetById(Guid id)
    {
        var p = await _db.Permisos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (p == null) return NotFound();
        return Ok(new PermisoDto { Id = p.Id, Name = p.Name, Code = p.Code, Description = p.Description, IsActive = p.IsActive });
    }

    [HttpPost]
    public async Task<ActionResult<PermisoDto>> Create(CrearPermisoDto dto)
    {
        if (await _db.Permisos.AnyAsync(p => p.Code == dto.Code))
            return Conflict("Ya existe un permiso con ese código");

        var perm = new Permiso { Name = dto.Name, Code = dto.Code, Description = dto.Description };
        _db.Permisos.Add(perm);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = perm.Id }, perm.Id);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, ActualizarPermisoDto dto)
    {
        var perm = await _db.Permisos.FindAsync(id);
        if (perm == null) return NotFound();

        perm.Name = dto.Name;
        perm.Code = dto.Code;
        perm.Description = dto.Description;
        perm.IsActive = dto.IsActive;
        perm.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var perm = await _db.Permisos.FindAsync(id);
        if (perm == null) return NotFound();

        _db.Permisos.Remove(perm);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}