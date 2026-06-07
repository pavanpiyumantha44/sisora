using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sisora.API.Models.DTOs.Requests;
using Sisora.API.Services.Interfaces;

namespace Sisora.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TripController : ControllerBase
{
    private readonly ITripService _tripService;

    public TripController(ITripService tripService)
    {
        _tripService = tripService;
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // ── Driver: Start Trip ────────────────────────────────
    [HttpPost("routes/{routeId}/start")]
    [Authorize(Policy = "DriverOnly")]
    public async Task<IActionResult> StartTrip(Guid routeId, [FromBody] StartTripRequest request)
    {
        var result = await _tripService.StartTripAsync(GetUserId(), routeId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ── Driver: End Trip ──────────────────────────────────
    [HttpPost("{tripId}/end")]
    [Authorize(Policy = "DriverOnly")]
    public async Task<IActionResult> EndTrip(Guid tripId)
    {
        var result = await _tripService.EndTripAsync(GetUserId(), tripId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ── Driver: Update Location ───────────────────────────
    [HttpPost("{tripId}/location")]
    [Authorize(Policy = "DriverOnly")]
    public async Task<IActionResult> UpdateLocation(Guid tripId, [FromBody] UpdateLocationRequest request)
    {
        var result = await _tripService.UpdateLocationAsync(tripId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ── Driver: Record Student Event ──────────────────────
    [HttpPost("{tripId}/events")]
    [Authorize(Policy = "DriverOnly")]
    public async Task<IActionResult> RecordTripEvent(Guid tripId, [FromBody] TripEventRequest request)
    {
        var result = await _tripService.RecordTripEventAsync(GetUserId(), tripId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ── Parent + Driver: Get Live Location ────────────────
    [HttpGet("{tripId}/location")]
    [Authorize]
    public async Task<IActionResult> GetLiveLocation(Guid tripId)
    {
        var result = await _tripService.GetLiveLocationAsync(tripId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ── Parent + Driver: Get Active Trip ──────────────────
    [HttpGet("routes/{routeId}/active")]
    [Authorize]
    public async Task<IActionResult> GetActiveTrip(Guid routeId)
    {
        var result = await _tripService.GetActiveTripAsync(routeId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    // ── Parent + Driver: Get Trip History ─────────────────
    [HttpGet("routes/{routeId}/history")]
    [Authorize]
    public async Task<IActionResult> GetTripHistory(Guid routeId)
    {
        var result = await _tripService.GetTripHistoryAsync(routeId);
        return Ok(result);
    }
}