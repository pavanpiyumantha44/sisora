using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sisora.API.Models.DTOs.Requests;
using Sisora.API.Services.Interfaces;

namespace Sisora.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "DriverOnly")]
public class DriverController : ControllerBase
{
    private readonly IDriverService _driverService;

    public DriverController(IDriverService driverService)
    {
        _driverService = driverService;
    }

    private Guid GetDriverId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // ── Routes ────────────────────────────────────────────
    [HttpPost("routes")]
    public async Task<IActionResult> CreateRoute([FromBody] CreateServiceRouteRequest request)
    {
        var result = await _driverService.CreateRouteAsync(GetDriverId(), request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("routes")]
    public async Task<IActionResult> GetMyRoutes()
    {
        var result = await _driverService.GetMyRoutesAsync(GetDriverId());
        return Ok(result);
    }

    [HttpPut("routes/{routeId}")]
    public async Task<IActionResult> UpdateRoute(Guid routeId, [FromBody] UpdateServiceRouteRequest request)
    {
        var result = await _driverService.UpdateRouteAsync(GetDriverId(), routeId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("routes/{routeId}")]
    public async Task<IActionResult> DeleteRoute(Guid routeId)
    {
        var result = await _driverService.DeleteRouteAsync(GetDriverId(), routeId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ── Students ──────────────────────────────────────────
    [HttpPost("routes/{routeId}/students")]
    public async Task<IActionResult> AddStudent(Guid routeId, [FromBody] AddStudentRequest request)
    {
        var result = await _driverService.AddStudentAsync(GetDriverId(), routeId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("routes/{routeId}/students")]
    public async Task<IActionResult> GetStudents(Guid routeId)
    {
        var result = await _driverService.GetStudentsAsync(GetDriverId(), routeId);
        return Ok(result);
    }

    [HttpPut("routes/{routeId}/students/{studentId}")]
    public async Task<IActionResult> UpdateStudent(Guid routeId, Guid studentId, [FromBody] UpdateStudentRequest request)
    {
        var result = await _driverService.UpdateStudentAsync(GetDriverId(), routeId, studentId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("routes/{routeId}/students/{studentId}")]
    public async Task<IActionResult> RemoveStudent(Guid routeId, Guid studentId)
    {
        var result = await _driverService.RemoveStudentAsync(GetDriverId(), routeId, studentId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ── Invite Codes ──────────────────────────────────────
    [HttpPost("routes/{routeId}/students/{studentId}/regenerate-code")]
    public async Task<IActionResult> RegenerateInviteCode(Guid routeId, Guid studentId)
    {
        var result = await _driverService.RegenerateInviteCodeAsync(GetDriverId(), routeId, studentId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}