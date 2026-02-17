using System.Security.Claims;
using FeedbackSystem.API.DTOs;
using FeedbackSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeedbackSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _service;
    public UsersController(IUserService service) => _service = service;

    private string GetUserId()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("userId");
        if (string.IsNullOrWhiteSpace(id))
            throw new UnauthorizedAccessException("UserId claim missing.");
        return id;
    }

    // ============ PROFILE ENDPOINTS (Any authenticated user) ============

    [HttpGet("profile")]
    public async Task<ActionResult<ProfileReadDto>> GetProfile(CancellationToken ct)
    {
        var profile = await _service.GetProfileAsync(GetUserId(), ct);
        return profile is null ? NotFound() : Ok(profile);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(ProfileUpdateDto dto, CancellationToken ct)
    {
        try
        {
            var success = await _service.UpdateProfileAsync(GetUserId(), dto, ct);
            return success ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ============ ADMIN ONLY ENDPOINTS ============

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<UserReadDto>>> GetAll(CancellationToken ct)
        => Ok(await _service.GetAllAsync(ct));

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UserReadDto>> Get(string id, CancellationToken ct)
    {
        var user = await _service.GetByIdAsync(id, ct);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UserReadDto>> Create(UserCreateDto dto, CancellationToken ct)
    {
        var created = await _service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(Get), new { id = created.UserId }, created);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(string id, UserUpdateDto dto, CancellationToken ct)
        => await _service.UpdateAsync(id, dto, ct) ? NoContent() : NotFound();

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
        => await _service.DeleteAsync(id, ct) ? NoContent() : NotFound();

    // ✅ Statistics endpoint
    [HttpGet("stats")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UserStatsDto>> GetStats(CancellationToken ct)
        => Ok(await _service.GetStatsAsync(ct));

    /// <summary>
    /// GET /api/users/search?query={searchTerm}
    /// Search for users by name or email (accessible to all authenticated users)
    /// </summary>
    [HttpGet("search")]
    [AllowAnonymous] // Override the controller-level [Authorize(Roles = "Admin")]
    [Authorize] // But still require authentication (any role)
    public async Task<ActionResult<List<UserReadDto>>> Search([FromQuery] string search, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(search) || search.Length < 2)
            return Ok(new List<UserReadDto>());

        var results = await _service.SearchAsync(search, ct);
        return Ok(results);
    }
}