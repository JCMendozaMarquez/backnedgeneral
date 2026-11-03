using BackendGeneral.Api.Data;
using BackendGeneral.Api.DTOs;
using BackendGeneral.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendGeneral.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _auth;
    private readonly AppDbContext _db;

    public AuthController(AuthService auth, AppDbContext db)
    {
        _auth = auth;
        _db   = db;
    }

    private string? Ip => HttpContext.Connection.RemoteIpAddress?.ToString();
    private string? UA => Request.Headers["User-Agent"].ToString();

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        var (ok, error, data) = await _auth.LoginAsync(dto, Ip, UA);
        if (!ok) return Unauthorized(new { message = error });
        return Ok(data);
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
    {
        var (ok, error, data) = await _auth.RegisterAsync(dto, Ip, UA);
        if (!ok) return Conflict(new { message = error });
        return Ok(data);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto?.RefreshToken))
            return BadRequest(new { message = "Refresh token requerido." });

        var (ok, error, data) = await _auth.RefreshAsync(dto.RefreshToken, Ip, UA);
        if (!ok) return Unauthorized(new { message = error });
        return Ok(data);
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout([FromBody] RefreshRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto?.RefreshToken))
            return NoContent();

        await _auth.LogoutAsync(dto.RefreshToken);
        return NoContent();
    }

}