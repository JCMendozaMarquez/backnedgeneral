using System.IdentityModel.Tokens.Jwt;
using BackendGeneral.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendGeneral.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MeController : ControllerBase
{
    private readonly AppDbContext _db;
    public MeController(AppDbContext db) => _db = db;

    private Guid CurrentUserId
    {
        get
        {
            var sub = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                   ?? User.FindFirst("sub")?.Value
                   ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
        }
    }

    private bool IsSuperAdmin =>
        User.Claims.Any(c => c.Type == "permission" && c.Value == "super.admin");

    [HttpGet("menu")]
    public async Task<IActionResult> GetMenu()
    {
        var userId = CurrentUserId;
        if (userId == Guid.Empty) return Unauthorized();

        // El super admin ve TODOS los módulos activos
        if (IsSuperAdmin)
        {
            var allModules = await _db.Modulos.AsNoTracking()
                .Where(m => m.IsActive && m.IsMenuVisible)
                .OrderBy(m => m.OrderIndex)
                .Select(m => new
                {
                    ModuleId = m.Id,
                    m.Name,
                    m.Code,
                    m.Icon,
                    m.Route,
                    m.ParentId,
                    m.OrderIndex,
                })
                .ToListAsync();
            return Ok(allModules);
        }

        // Los demás ven solo los que su rol permite
        var modulos = await _db.Database
            .SqlQueryRaw<MenuRow>(@"
                SELECT module_id AS ""ModuleId"", name AS ""Name"", code AS ""Code"",
                       icon AS ""Icon"", route AS ""Route"", parent_id AS ""ParentId"",
                       order_index AS ""OrderIndex""
                FROM v_user_modules
                WHERE user_id = {0}
                ORDER BY order_index", userId)
            .ToListAsync();

        return Ok(modulos);
    }

    [HttpGet("permissions")]
    public async Task<IActionResult> GetPermissions()
    {
        var userId = CurrentUserId;
        if (userId == Guid.Empty) return Unauthorized();

        // Super admin → todos los permisos
        if (IsSuperAdmin)
        {
            var all = await _db.Permisos.AsNoTracking()
                .Where(p => p.IsActive)
                .Select(p => new { p.Code, p.Name })
                .ToListAsync();
            return Ok(all);
        }

        var permisos = await _db.Database
            .SqlQueryRaw<PermRow>(@"
                SELECT permission_code AS ""Code"", permission_name AS ""Name""
                FROM v_user_permissions
                WHERE user_id = {0}", userId)
            .ToListAsync();

        return Ok(permisos);
    }

    public class MenuRow
    {
        public Guid ModuleId { get; set; }
        public string Name { get; set; } = "";
        public string Code { get; set; } = "";
        public string? Icon { get; set; }
        public string? Route { get; set; }
        public Guid? ParentId { get; set; }
        public int OrderIndex { get; set; }
    }

    public class PermRow
    {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
    }
}