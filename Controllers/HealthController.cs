using BackendGeneral.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendGeneral.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly AppDbContext _db;
    public HealthController(AppDbContext db) => _db = db;

    /// <summary>
    /// Verifica que la API responde y que PostgreSQL está accesible.
    /// </summary>
    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok(new
        {
            status = "ok",
            timestamp = DateTime.UtcNow,
            mensaje = "BackendGeneral API funcionando 🚀"
        });
    }

    /// <summary>
    /// Prueba la conexión real a PostgreSQL.
    /// </summary>
    [HttpGet("db")]
    public async Task<IActionResult> CheckDb()
    {
        try
        {
            var puedeConectar = await _db.Database.CanConnectAsync();
            return Ok(new
            {
                conectado = puedeConectar,
                baseDatos = _db.Database.GetDbConnection().Database,
                servidor = _db.Database.GetDbConnection().DataSource
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                conectado = false,
                error = ex.Message
            });
        }
    }
}