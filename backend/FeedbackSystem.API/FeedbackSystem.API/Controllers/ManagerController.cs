using System.Security.Claims;
using FeedbackSystem.API.DTOs;
using FeedbackSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeedbackSystem.API.Controllers;

[ApiController]
[Route("api/manager")]
[Authorize(Roles = "Manager,Admin")]
public class ManagerController : ControllerBase
{
    private readonly IManagerService _service;

    public ManagerController(IManagerService service) => _service = service;

    private string GetUserId()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("userId");
        if (string.IsNullOrWhiteSpace(id))
            throw new UnauthorizedAccessException("UserId claim missing.");
        return id;
    }

    // GET /api/manager/dashboard
    [HttpGet("dashboard")]
    public async Task<ActionResult<ManagerDashboardDto>> GetDashboard(CancellationToken ct)
    {
        try
        {
            var result = await _service.GetDashboardAsync(GetUserId(), ct);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // GET /api/manager/feedback?status=Pending&categoryId=&search=&page=1&pageSize=20
    [HttpGet("feedback")]
    public async Task<ActionResult<PagedResult<ManagerFeedbackItemDto>>> GetFeedbackList(
        [FromQuery] string? status,
        [FromQuery] string? categoryId,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        try
        {
            var result = await _service.GetFeedbackListAsync(GetUserId(), status, categoryId, search, page, pageSize, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // PUT /api/manager/feedback/{id}/status
    [HttpPut("feedback/{id}/status")]
    public async Task<IActionResult> UpdateFeedbackStatus(
        int id,
        [FromBody] UpdateFeedbackStatusDto dto,
        [FromQuery] string? remarks,
        CancellationToken ct)
    {
        try
        {
            var updated = await _service.UpdateFeedbackStatusAsync(GetUserId(), id, dto.Status, remarks, ct);
            if (!updated)
                return NotFound(new { message = "Feedback not found." });
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    // GET /api/manager/activity?count=10
    [HttpGet("activity")]
    public async Task<ActionResult<IReadOnlyList<RecentActivityDto>>> GetRecentActivity(
        [FromQuery] int count = 10,
        CancellationToken ct = default)
    {
        try
        {
            var result = await _service.GetRecentActivityAsync(GetUserId(), count, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // GET /api/manager/categories/distribution?status=All
    [HttpGet("categories/distribution")]
    public async Task<ActionResult<IReadOnlyList<CategoryStatsDto>>> GetCategoryDistribution(
        [FromQuery] string? status,
        CancellationToken ct = default)
    {
        try
        {
            var result = await _service.GetCategoryDistributionAsync(GetUserId(), status, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // GET /api/manager/recognitions?page=1&pageSize=20
    [HttpGet("recognitions")]
    public async Task<ActionResult<PagedResult<RecognitionItemDto>>> GetRecognitions(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        // Use existing insights service for recognitions
        // This endpoint can be expanded if needed
        return Ok(new { message = "Use /api/insight/recognitions endpoint for recognitions." });
    }
}
