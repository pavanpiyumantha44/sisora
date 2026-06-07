using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sisora.API.Models.DTOs.Requests;
using Sisora.API.Services.Interfaces;

namespace Sisora.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // ── Driver Register ───────────────────────────────────
    [HttpPost("driver/register")]
    public async Task<IActionResult> RegisterDriver([FromBody] RegisterDriverRequest request)
    {
        var result = await _authService.RegisterDriverAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ── Parent Register ───────────────────────────────────
    [HttpPost("parent/register")]
    public async Task<IActionResult> RegisterParent([FromBody] RegisterParentRequest request)
    {
        var result = await _authService.RegisterParentAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ── Driver Login ──────────────────────────────────────
    [HttpPost("driver/login")]
    public async Task<IActionResult> LoginDriver([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginDriverAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ── Parent Login ──────────────────────────────────────
    [HttpPost("parent/login")]
    public async Task<IActionResult> LoginParent([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginParentAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ── Admin Login ───────────────────────────────────────
    [HttpPost("admin/login")]
    public async Task<IActionResult> LoginAdmin([FromBody] AdminLoginRequest request)
    {
        var result = await _authService.LoginAdminAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ── Refresh Token ─────────────────────────────────────
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ── Revoke Token (Logout) ─────────────────────────────
    [HttpPost("revoke")]
    public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RevokeTokenAsync(request.RefreshToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }
    
    [HttpPost("fcm-token")]
    [Authorize]
    public async Task<IActionResult> RegisterFcmToken([FromBody] RegisterFcmTokenRequest request)
    {
        var result = await _authService.RegisterFcmTokenAsync(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!,
            User.FindFirstValue(ClaimTypes.Role)!,
            request.Token);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}