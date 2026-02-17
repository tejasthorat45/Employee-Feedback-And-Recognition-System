using FeedbackSystem.API.DTOs;
using FeedbackSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FeedbackSystem.API.Controllers;

/// <summary>
/// Employee's personal feedback and recognition endpoints
/// All endpoints are accessible to authenticated employees for their own data
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]  // Any authenticated user can access their own data
public class MyController : ControllerBase
{
    private readonly IMyDataService _myDataService;
    private readonly ILogger<MyController> _logger;

    public MyController(
        IMyDataService myDataService,
        ILogger<MyController> logger)
    {
        _myDataService = myDataService;
        _logger = logger;
    }

    // ==================== FEEDBACK ====================

    /// <summary>
    /// POST /api/my/feedback - Submit feedback
    /// </summary>
    [HttpPost("feedback")]
    public async Task<ActionResult<MyFeedbackSubmitResultDto>> SubmitFeedback(
        [FromBody] MyFeedbackSubmitDto dto,
        CancellationToken ct)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("Extracted userId from token: {UserId}", userId);
            
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User ID not found in token." });

            var result = await _myDataService.SubmitMyFeedbackAsync(userId, dto, ct);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Feedback submission validation failed for user");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to submit feedback");
            return StatusCode(500, new { message = "Failed to submit feedback.", detail = ex.Message });
        }
    }

    /// <summary>
    /// GET /api/my/feedback?direction=given|received - Get my feedback
    /// </summary>
    [HttpGet("feedback")]
    public async Task<ActionResult<List<MyFeedbackDto>>> GetMyFeedback(
        [FromQuery] string? direction,
        CancellationToken ct)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User ID not found in token." });

            var result = await _myDataService.GetMyFeedbackAsync(userId, direction, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get feedback");
            return StatusCode(500, new { message = "Failed to retrieve feedback.", detail = ex.Message });
        }
    }

    // ==================== RECOGNITION ====================

    /// <summary>
    /// POST /api/my/recognition - Submit recognition
    /// </summary>
    [HttpPost("recognition")]
    public async Task<ActionResult<MyRecognitionSubmitResultDto>> SubmitRecognition(
        [FromBody] MyRecognitionSubmitDto dto,
        CancellationToken ct)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User ID not found in token." });

            var result = await _myDataService.SubmitMyRecognitionAsync(userId, dto, ct);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Recognition submission validation failed for user");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to submit recognition");
            return StatusCode(500, new { message = "Failed to submit recognition.", detail = ex.Message });
        }
    }

    /// <summary>
    /// GET /api/my/recognition?direction=given|received - Get my recognition
    /// </summary>
    [HttpGet("recognition")]
    public async Task<ActionResult<List<MyRecognitionDto>>> GetMyRecognition(
        [FromQuery] string? direction,
        CancellationToken ct)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User ID not found in token." });

            var result = await _myDataService.GetMyRecognitionAsync(userId, direction, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get recognition");
            return StatusCode(500, new { message = "Failed to retrieve recognition.", detail = ex.Message });
        }
    }

    // ==================== SUMMARY ====================

    /// <summary>
    /// GET /api/my/summary - Get my overall statistics
    /// </summary>
    [HttpGet("summary")]
    public async Task<ActionResult<MySummaryDto>> GetMySummary(CancellationToken ct)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User ID not found in token." });

            var result = await _myDataService.GetMySummaryAsync(userId, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get summary");
            return StatusCode(500, new { message = "Failed to retrieve summary.", detail = ex.Message });
        }
    }

}
