using System.Security.Claims;
using FeedbackSystem.API.DTOs;
using FeedbackSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeedbackSystem.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _service;

    public NotificationsController(INotificationService service) => _service = service;

    private string GetUserId()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("userId");
        if (string.IsNullOrWhiteSpace(id))
            throw new UnauthorizedAccessException("UserId claim missing.");
        return id;
    }

    // GET /api/notifications?isRead=false&page=1&pageSize=20
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<NotificationReadDto>>> GetNotifications(
        [FromQuery] bool? isRead,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _service.GetNotificationsAsync(GetUserId(), isRead, page, pageSize, ct);
        return Ok(result);
    }

    // GET /api/notifications/count
    [HttpGet("count")]
    public async Task<ActionResult<NotificationCountDto>> GetCount(CancellationToken ct)
    {
        var result = await _service.GetCountAsync(GetUserId(), ct);
        return Ok(result);
    }

    // PUT /api/notifications/{id}/read
    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id, CancellationToken ct)
    {
        var success = await _service.MarkAsReadAsync(id, GetUserId(), ct);
        if (!success) return NotFound(new { message = "Notification not found." });
        return NoContent();
    }

    // PUT /api/notifications/read-all
    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken ct)
    {
        await _service.MarkAllAsReadAsync(GetUserId(), ct);
        return NoContent();
    }
}
