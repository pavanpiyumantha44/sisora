using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sisora.API.Models.DTOs.Requests;
using Sisora.API.Services.Interfaces;

namespace Sisora.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "ParentOnly")]
public class ParentController : ControllerBase
{
    private readonly IParentService _parentService;

    public ParentController(IParentService parentService)
    {
        _parentService = parentService;
    }

    private Guid GetParentId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // ── Redeem Invite Code ────────────────────────────────
    [HttpPost("redeem-invite")]
    public async Task<IActionResult> RedeemInviteCode([FromBody] RedeemInviteCodeRequest request)
    {
        var result = await _parentService.RedeemInviteCodeAsync(GetParentId(), request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ── Get My Children ───────────────────────────────────
    [HttpGet("children")]
    public async Task<IActionResult> GetMyChildren()
    {
        var result = await _parentService.GetMyChildrenAsync(GetParentId());
        return Ok(result);
    }
}