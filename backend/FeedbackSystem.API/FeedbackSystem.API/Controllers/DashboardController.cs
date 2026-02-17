using FeedbackSystem.API.DTOs;
using FeedbackSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeedbackSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Manager")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _service;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(IDashboardService service, ILogger<DashboardController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // GET /api/dashboard/summary
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary(CancellationToken ct = default)
        {
            try
            {
                var result = await _service.GetSummaryAsync(ct);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching dashboard summary");
                return StatusCode(500, new { error = "Failed to load summary", detail = ex.Message });
            }
        }

        // GET /api/dashboard/feedback-by-category
        [HttpGet("feedback-by-category")]
        public async Task<IActionResult> GetFeedbackByCategory(CancellationToken ct = default)
        {
            try
            {
                var result = await _service.GetFeedbackByCategoryAsync(ct);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching feedback by category");
                return StatusCode(500, new { error = "Failed to load feedback by category", detail = ex.Message });
            }
        }

        // GET /api/dashboard/recognition-by-badge
        [HttpGet("recognition-by-badge")]
        public async Task<IActionResult> GetRecognitionByBadge(CancellationToken ct = default)
        {
            try
            {
                var result = await _service.GetRecognitionByBadgeAsync(ct);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching recognition by badge");
                return StatusCode(500, new { error = "Failed to load recognition by category", detail = ex.Message });
            }
        }

        // GET /api/dashboard/monthly-trends?months=6
        [HttpGet("monthly-trends")]
        public async Task<IActionResult> GetMonthlyTrends(
            [FromQuery] int months = 6, CancellationToken ct = default)
        {
            try
            {
                var result = await _service.GetMonthlyTrendsAsync(months, ct);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching monthly trends");
                return StatusCode(500, new { error = "Failed to load monthly trends", detail = ex.Message });
            }
        }

        // GET /api/dashboard/department-feedback-counts?from=&to=
        [HttpGet("department-feedback-counts")]
        public async Task<IActionResult> GetDepartmentFeedbackCounts(
            [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct = default)
        {
            try
            {
                var result = await _service.GetDepartmentFeedbackCountsAsync(from, to, ct);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching department feedback counts");
                return StatusCode(500, new { error = "Failed to load department feedback counts", detail = ex.Message });
            }
        }

        // GET /api/dashboard/top-employees?limit=10&from=&to=
        [HttpGet("top-employees")]
        public async Task<IActionResult> GetTopEmployees(
            [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int limit = 10,
            CancellationToken ct = default)
        {
            try
            {
                var result = await _service.GetTopEmployeesByPointsAsync(limit, from, to, ct);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching top employees");
                return StatusCode(500, new { error = "Failed to load top employees", detail = ex.Message });
            }
        }

        // GET /api/dashboard/department-recognition-stats?from=&to=
        [HttpGet("department-recognition-stats")]
        public async Task<IActionResult> GetDepartmentRecognitionStats(
            [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct = default)
        {
            try
            {
                var result = await _service.GetDepartmentRecognitionStatsAsync(from, to, ct);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching department recognition stats");
                return StatusCode(500, new { error = "Failed to load department recognition stats", detail = ex.Message });
            }
        }

        // GET /api/dashboard/category-average-scores?from=&to=
        [HttpGet("category-average-scores")]
        public async Task<IActionResult> GetCategoryAverageScores(
            [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct = default)
        {
            try
            {
                var result = await _service.GetCategoryAverageScoresAsync(from, to, ct);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching category average scores");
                return StatusCode(500, new { error = "Failed to load category average scores", detail = ex.Message });
            }
        }
    }
}
