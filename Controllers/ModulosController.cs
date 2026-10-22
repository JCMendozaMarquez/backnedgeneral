using BackendGeneral.Api.Data;
using BackendGeneral.Api.DTOs;
using BackendGeneral.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendGeneral.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class ModulosController : ControllerBase
{
    private readonly AppDbContext _db;
    public ModulosController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ModuloDto>>> GetAll()
    {
        var list = await _db.Modulos.AsNoTracking()
            .OrderBy(m => m.OrderIndex)
            .Select(m => new ModuloDto
            {
                Id = m.Id, Name = m.Name, Code = m.Code, Description = m.Description,
                Icon = m.Icon, Route = m.Route, ParentId = m.ParentId,
                OrderIndex = m.OrderIndex, IsMenuVisible = m.IsMenuVisible, IsActive = m.IsActive
            }).ToListAsync();
        return Ok(list);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ModuloDto>> GetById(Guid id)
    {
        var m = await _db.Modulos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (m == null) return NotFound();
        return Ok(new ModuloDto
        {
            Id = m.Id, Name = m.Name, Code = m.Code, Description = m.Description,
            Icon = m.Icon, Route = m.Route, ParentId = m.ParentId,
            OrderIndex = m.OrderIndex, IsMenuVisible = m.IsMenuVisible, IsActive = m.IsActive
        });
    }

    [HttpPost]
    public async Task<ActionResult<ModuloDto>> Create(CrearModuloDto dto)
    {
        // Fix 1: case-insensitive + objeto anónimo para que el front lea data.message
        if (await _db.Modulos.AnyAsync(m => m.Code.ToLower() == dto.Code.ToLower()))
            return Conflict(new { message = "Ya existe un módulo con ese código" });

        var modulo = new Modulo
        {
            Name = dto.Name, Code = dto.Code, Description = dto.Description,
            Icon = dto.Icon, Route = dto.Route, ParentId = dto.ParentId,
            OrderIndex = dto.OrderIndex, IsMenuVisible = dto.IsMenuVisible
        };
        _db.Modulos.Add(modulo);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = modulo.Id }, modulo.Id);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, ActualizarModuloDto dto)
    {
        var modulo = await _db.Modulos.FindAsync(id);
        if (modulo == null) return NotFound();

        // Fix 2: no puede ser su propio padre
        if (dto.ParentId == id)
            return BadRequest(new { message = "Un módulo no puede ser su propio padre" });

        // Validar que el padre existe (si viene uno)
        if (dto.ParentId.HasValue)
        {
            var existePadre = await _db.Modulos.AnyAsync(m => m.Id == dto.ParentId);
            if (!existePadre)
                return BadRequest(new { message = "El módulo padre no existe" });
        }

        modulo.Name = dto.Name;
        // modulo.Code = dto.Code;  ← Fix 3: NO tocar Code, es inmutable
        modulo.Description = dto.Description;
        modulo.Icon = dto.Icon;
        modulo.Route = dto.Route;
        modulo.ParentId = dto.ParentId;
        modulo.OrderIndex = dto.OrderIndex;
        modulo.IsMenuVisible = dto.IsMenuVisible;
        modulo.IsActive = dto.IsActive;
        modulo.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var modulo = await _db.Modulos.FindAsync(id);
        if (modulo == null) return NotFound();

        // Fix 4: bloquear si tiene submódulos
        var tieneHijos = await _db.Modulos.AnyAsync(m => m.ParentId == id);
        if (tieneHijos)
            return Conflict(new { message = "No se puede eliminar: el módulo tiene submódulos" });

        // Fix 5: bloquear si está asignado a roles
        var asignado = await _db.RolModulos.AnyAsync(rm => rm.ModuleId == id);
        if (asignado)
            return Conflict(new { message = "No se puede eliminar: el módulo está asignado a roles" });

        _db.Modulos.Remove(modulo);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}