// Controllers/VentasController.cs
using BackendGeneral.Api.Data;
using BackendGeneral.Api.DTO;
using BackendGeneral.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BackendGeneral.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VentasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VentasController(AppDbContext context)
        {
            _context = context;
        }

        private Guid GetUsuarioId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("sub")?.Value;
            return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
        }

        // GET: api/ventas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VentaDto>>> GetAll()
        {
            var ventas = await _context.Ventas
                .OrderByDescending(v => v.CreatedAt)
                .Select(v => new VentaDto
                {
                    Id = v.Id,
                    UsuarioId = v.UsuarioId,
                    Numero = v.Numero,
                    Cliente = v.Cliente,
                    Total = v.Total,
                    Estado = v.Estado,
                    CreatedAt = v.CreatedAt,
                    UpdatedAt = v.UpdatedAt
                })
                .ToListAsync();

            return Ok(ventas);
        }

        // GET: api/ventas/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<VentaDto>> GetById(Guid id)
        {
            var v = await _context.Ventas.FindAsync(id);
            if (v == null) return NotFound(new { message = "Venta no encontrada" });

            return Ok(new VentaDto
            {
                Id = v.Id,
                UsuarioId = v.UsuarioId,
                Numero = v.Numero,
                Cliente = v.Cliente,
                Total = v.Total,
                Estado = v.Estado,
                CreatedAt = v.CreatedAt,
                UpdatedAt = v.UpdatedAt
            });
        }

        // POST: api/ventas
        [HttpPost]
        public async Task<ActionResult<VentaDto>> Create([FromBody] CrearVentaDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var numeroExiste = await _context.Ventas.AnyAsync(v => v.Numero == dto.Numero);
            if (numeroExiste)
                return Conflict(new { message = $"Ya existe una venta con el número {dto.Numero}" });

            var venta = new Venta
            {
                Id = Guid.NewGuid(),
                UsuarioId = GetUsuarioId(),
                Numero = dto.Numero,
                Cliente = dto.Cliente,
                Total = dto.Total,
                Estado = string.IsNullOrWhiteSpace(dto.Estado) ? "PENDIENTE" : dto.Estado,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Ventas.Add(venta);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = venta.Id }, new VentaDto
            {
                Id = venta.Id,
                UsuarioId = venta.UsuarioId,
                Numero = venta.Numero,
                Cliente = venta.Cliente,
                Total = venta.Total,
                Estado = venta.Estado,
                CreatedAt = venta.CreatedAt,
                UpdatedAt = venta.UpdatedAt
            });
        }

        // PUT: api/ventas/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ActualizarVentaDto dto)
        {
            var venta = await _context.Ventas.FindAsync(id);
            if (venta == null) return NotFound(new { message = "Venta no encontrada" });

            if (!string.IsNullOrWhiteSpace(dto.Numero) && dto.Numero != venta.Numero)
            {
                var existe = await _context.Ventas.AnyAsync(v => v.Numero == dto.Numero && v.Id != id);
                if (existe) return Conflict(new { message = "Ya existe otra venta con ese número" });
                venta.Numero = dto.Numero;
            }

            if (!string.IsNullOrWhiteSpace(dto.Cliente)) venta.Cliente = dto.Cliente;
            if (dto.Total.HasValue) venta.Total = dto.Total.Value;
            if (!string.IsNullOrWhiteSpace(dto.Estado)) venta.Estado = dto.Estado;

            venta.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/ventas/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var venta = await _context.Ventas.FindAsync(id);
            if (venta == null) return NotFound(new { message = "Venta no encontrada" });

            _context.Ventas.Remove(venta);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}