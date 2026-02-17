using FeedbackSystem.API.DTOs;
using FeedbackSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeedbackSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BadgesController : ControllerBase
    {
        private readonly IBadgeService _service;
        private readonly ILogger<BadgesController> _logger;

        public BadgesController(IBadgeService service, ILogger<BadgesController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // GET /api/badges?isActive=true&search=team&page=1&pageSize=10
        [HttpGet]
        public async Task<ActionResult<PagedBadgeResult>> GetAll(
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken ct = default)
        {
            try
            {
                var result = await _service.GetAllAsync(isActive, search, page, pageSize, ct);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching badges");
                return StatusCode(500, new { message = "An error occurred while fetching badges" });
            }
        }

        // GET /api/badges/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<BadgeDto>> GetById(string id, CancellationToken ct = default)
        {
            try
            {
                var badge = await _service.GetByIdAsync(id, ct);
                if (badge == null)
                    return NotFound(new { message = $"Badge with ID '{id}' not found" });

                return Ok(badge);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching badge {BadgeId}", id);
                return StatusCode(500, new { message = "An error occurred while fetching the badge" });
            }
        }

        // POST /api/badges
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BadgeDto>> Create([FromBody] CreateBadgeDto dto, CancellationToken ct = default)
        {
            try
            {
                var created = await _service.CreateAsync(dto, ct);
                return CreatedAtAction(nameof(GetById), new { id = created.BadgeId }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating badge");
                return StatusCode(500, new { message = "An error occurred while creating the badge" });
            }
        }

        // PUT /api/badges/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BadgeDto>> Update(string id, [FromBody] UpdateBadgeDto dto, CancellationToken ct = default)
        {
            try
            {
                var updated = await _service.UpdateAsync(id, dto, ct);
                return Ok(updated);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating badge {BadgeId}", id);
                return StatusCode(500, new { message = "An error occurred while updating the badge" });
            }
        }

        // DELETE /api/badges/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id, CancellationToken ct = default)
        {
            try
            {
                await _service.DeleteAsync(id, ct);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting badge {BadgeId}", id);
                return StatusCode(500, new { message = "An error occurred while deleting the badge" });
            }
        }

        // GET /api/badges/count
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetCount(CancellationToken ct = default)
        {
            try
            {
                var count = await _service.GetTotalCountAsync(ct);
                return Ok(new { totalCount = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching badge count");
                return StatusCode(500, new { message = "An error occurred while fetching badge count" });
            }
        }
    }
}
