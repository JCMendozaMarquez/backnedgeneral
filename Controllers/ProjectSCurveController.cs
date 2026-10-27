// Controllers/ProjectSCurveController.cs
using BackendGeneral.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendGeneral.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProjectSCurveController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProjectSCurveController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/ProjectSCurve
        [HttpGet]
        public async Task<IActionResult> GetProjects()
        {
            var projects = await _context.Projects
                .AsNoTracking()
                .OrderBy(p => p.Name)
                .Select(p => new
                {
                    p.Id,
                    p.Code,
                    p.Name,
                    p.Description,
                    p.StartDate,
                    p.EndDate,
                    p.Status
                })
                .ToListAsync();

            return Ok(projects);
        }

        // GET: api/ProjectSCurve/{id}/tasks
        [HttpGet("{id:guid}/tasks")]
        public async Task<IActionResult> GetTasks(Guid id)
        {
            var exists = await _context.Projects.AnyAsync(p => p.Id == id);
            if (!exists)
                return NotFound(new { message = "Proyecto no encontrado." });

            var tasks = await _context.ProjectTasks
                .AsNoTracking()
                .Where(t => t.ProjectId == id)
                .OrderBy(t => t.Code)
                .ToListAsync();

            return Ok(tasks);
        }

        // GET: api/ProjectSCurve/{id}/scurve
        [HttpGet("{id:guid}/scurve")]
        public async Task<IActionResult> GetSCurve(Guid id)
        {
            var exists = await _context.Projects.AnyAsync(p => p.Id == id);
            if (!exists)
                return NotFound(new { message = "Proyecto no encontrado." });

            var points = await _context.ProjectSCurvePoints
                .AsNoTracking()
                .Where(x => x.ProjectId == id)
                .OrderBy(x => x.MonthStart)
                .ToListAsync();

            return Ok(points);
        }
    }
}