using FeedbackSystem.API.DTOs;
using FeedbackSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeedbackSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SettingsController : ControllerBase
{
    private readonly ISettingsService _settingsService;
    private readonly ILogger<SettingsController> _logger;

    public SettingsController(ISettingsService settingsService, ILogger<SettingsController> logger)
    {
        _settingsService = settingsService;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/settings – returns the structured settings object
    /// Accessible by all authenticated users (read-only for non-admins)
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetSettings(CancellationToken ct)
    {
        try
        {
            var settings = await _settingsService.GetSettingsAsync(ct);
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get settings");
            return StatusCode(500, new { error = "Failed to load settings", detail = ex.Message });
        }
    }

    /// <summary>
    /// PUT /api/settings – accepts the structured settings object and persists it
    /// Only accessible by Admin users
    /// </summary>
    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SaveSettings([FromBody] AppSettingsDto settings, CancellationToken ct)
    {
        try
        {
            if (settings == null)
                return BadRequest(new { error = "Settings object is required" });

            await _settingsService.SaveSettingsAsync(settings, ct);
            return Ok(new { message = "Settings saved successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save settings");
            return StatusCode(500, new { error = "Failed to save settings", detail = ex.Message });
        }
    }
}
