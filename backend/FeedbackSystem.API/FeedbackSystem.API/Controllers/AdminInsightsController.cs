using System.Security.Claims;
using FeedbackSystem.API.DTOs;
using FeedbackSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeedbackSystem.API.Controllers
{
    [ApiController]
    [Route("api/insight")] // keeping your route; both roles can call it now
    [Authorize(Roles = "Admin,Manager")]
    public class AdminInsightsController : ControllerBase
    {
        private readonly IInsightsService _service;
        private readonly ISentimentService _sentimentService;

        public AdminInsightsController(IInsightsService service, ISentimentService sentimentService)
        {
            _service = service;
            _sentimentService = sentimentService;
        }

        // Helpers to read requester identity
        private string GetRequesterUserId()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("userId");
            if (string.IsNullOrWhiteSpace(id)) throw new UnauthorizedAccessException("UserId claim missing.");
            return id;
        }
        private bool IsAdmin() =>
            string.Equals(User.FindFirstValue(ClaimTypes.Role), "Admin", StringComparison.OrdinalIgnoreCase);

        // ---------------- By USER ----------------

        // GET /api/admin/users/{userId}/feedback/given?from=&to=&categoryId=&search=&page=&pageSize=
        [HttpGet("users/{userId}/feedback/given")]
        public async Task<ActionResult<PagedResult<FeedbackItemDto>>> GetFeedbackGiven(
            string userId, [FromQuery] DateTime? from, [FromQuery] DateTime? to,
            [FromQuery] string? categoryId, [FromQuery] string? search,
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
        {
            var requesterId = GetRequesterUserId();
            var result = await _service.GetFeedbackGivenAsync(requesterId, IsAdmin(),
                userId, from, to, categoryId, search, page, pageSize, ct);
            return Ok(result);
        }

        // GET /api/admin/users/{userId}/feedback/received
        [HttpGet("users/{userId}/feedback/received")]
        public async Task<ActionResult<PagedResult<FeedbackItemDto>>> GetFeedbackReceived(
            string userId, [FromQuery] DateTime? from, [FromQuery] DateTime? to,
            [FromQuery] string? categoryId, [FromQuery] string? search,
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
        {
            var requesterId = GetRequesterUserId();
            var result = await _service.GetFeedbackReceivedAsync(requesterId, IsAdmin(),
                userId, from, to, categoryId, search, page, pageSize, ct);
            return Ok(result);
        }

        // GET /api/admin/users/{userId}/recognitions/given
        [HttpGet("users/{userId}/recognitions/given")]
        public async Task<ActionResult<PagedResult<RecognitionItemDto>>> GetRecognitionsGiven(
            string userId, [FromQuery] DateTime? from, [FromQuery] DateTime? to,
            [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            var requesterId = GetRequesterUserId();
            var result = await _service.GetRecognitionsGivenAsync(requesterId, IsAdmin(),
                userId, from, to, search, page, pageSize, ct);
            return Ok(result);
        }

        // GET /api/admin/users/{userId}/recognitions/received
        [HttpGet("users/{userId}/recognitions/received")]
        public async Task<ActionResult<PagedResult<RecognitionItemDto>>> GetRecognitionsReceived(
            string userId, [FromQuery] DateTime? from, [FromQuery] DateTime? to,
            [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            var requesterId = GetRequesterUserId();
            var result = await _service.GetRecognitionsReceivedAsync(requesterId, IsAdmin(),
                userId, from, to, search, page, pageSize, ct);
            return Ok(result);
        }

        // GET /api/admin/users/{userId}/insights/summary
        [HttpGet("users/{userId}/insights/summary")]
        public async Task<ActionResult<UserInsightSummaryDto>> GetUserSummary(string userId, CancellationToken ct = default)
        {
            var requesterId = GetRequesterUserId();
            var result = await _service.GetSummaryAsync(requesterId, IsAdmin(), userId, ct);
            return Ok(result);
        }

        // ---------------- “GET ALL” across users ----------------
        // Admin → full system; Manager → limited to their Department (either side)

        // GET /api/admin/feedback?from=&to=&categoryId=&search=&departmentId=&fromUserId=&toUserId=&page=&pageSize=
        [HttpGet("feedback")]
        public async Task<ActionResult<PagedResult<FeedbackItemDto>>> GetAllFeedback(
            [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string? categoryId,
            [FromQuery] string? search, [FromQuery] string? departmentId,
            [FromQuery] string? fromUserId, [FromQuery] string? toUserId,
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
        {
            var requesterId = GetRequesterUserId();
            var filter = new FeedbackAllFilter(from, to, categoryId, search, departmentId, fromUserId, toUserId, page, pageSize);
            var result = await _service.GetAllFeedbackAsync(requesterId, IsAdmin(), filter, ct);
            return Ok(result);
        }

        // GET /api/admin/recognitions?from=&to=&search=&departmentId=&fromUserId=&toUserId=&page=&pageSize=
        [HttpGet("recognitions")]
        public async Task<ActionResult<PagedResult<RecognitionItemDto>>> GetAllRecognitions(
            [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string? search,
            [FromQuery] string? departmentId, [FromQuery] string? fromUserId, [FromQuery] string? toUserId,
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
        {
            var requesterId = GetRequesterUserId();
            var filter = new RecognitionAllFilter(from, to, search, departmentId, fromUserId, toUserId, page, pageSize);
            var result = await _service.GetAllRecognitionsAsync(requesterId, IsAdmin(), filter, ct);
            return Ok(result);
        }

        // ---------------- COUNT-ONLY endpoints ----------------
        // Useful for dashboards/stats without fetching full data

        // GET /api/admin/feedback/count?from=&to=&categoryId=&search=&departmentId=&fromUserId=&toUserId=
        [HttpGet("feedback/count")]
        public async Task<ActionResult<CountResultDto>> GetAllFeedbackCount(
            [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string? categoryId,
            [FromQuery] string? search, [FromQuery] string? departmentId,
            [FromQuery] string? fromUserId, [FromQuery] string? toUserId, CancellationToken ct = default)
        {
            var requesterId = GetRequesterUserId();
            var filter = new FeedbackAllFilter(from, to, categoryId, search, departmentId, fromUserId, toUserId, 1, 1);
            var result = await _service.GetAllFeedbackCountAsync(requesterId, IsAdmin(), filter, ct);
            return Ok(result);
        }

        // GET /api/admin/recognitions/count?from=&to=&search=&departmentId=&fromUserId=&toUserId=
        [HttpGet("recognitions/count")]
        public async Task<ActionResult<CountResultDto>> GetAllRecognitionsCount(
            [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string? search,
            [FromQuery] string? departmentId, [FromQuery] string? fromUserId, [FromQuery] string? toUserId,
            CancellationToken ct = default)
        {
            var requesterId = GetRequesterUserId();
            var filter = new RecognitionAllFilter(from, to, search, departmentId, fromUserId, toUserId, 1, 1);
            var result = await _service.GetAllRecognitionsCountAsync(requesterId, IsAdmin(), filter, ct);
            return Ok(result);
        }

        // ---------------- CATEGORY STATISTICS ----------------
        // Group statistics by category (no input required)

        // GET /api/insight/feedback/by-category
        [HttpGet("feedback/by-category")]
        public async Task<ActionResult<IReadOnlyList<CategoryStatsDto>>> GetFeedbackByCategory(
            CancellationToken ct = default)
        {
            var result = await _service.GetFeedbackByCategoryAsync(ct);
            return Ok(result);
        }

        // GET /api/insight/recognitions/by-badge
        [HttpGet("recognitions/by-badge")]
        public async Task<ActionResult<IReadOnlyList<RecognitionBadgeStatsDto>>> GetRecognitionsByBadge(
            CancellationToken ct = default)
        {
            var result = await _service.GetRecognitionsByBadgeAsync(ct);
            return Ok(result);
        }

        // ---------------- SENTIMENT ANALYSIS ----------------

        // GET /api/insight/sentiment/stats?from=&to=&departmentId=
        [HttpGet("sentiment/stats")]
        public async Task<ActionResult<SentimentStatsDto>> GetSentimentStats(
            [FromQuery] DateTime? from, [FromQuery] DateTime? to, 
            [FromQuery] string? departmentId, CancellationToken ct = default)
        {
            var result = await _sentimentService.GetSentimentStatsAsync(from, to, departmentId, ct);
            return Ok(result);
        }

        // GET /api/insight/sentiment/feedback?from=&to=&categoryId=&departmentId=&sentiment=&page=&pageSize=
        [HttpGet("sentiment/feedback")]
        public async Task<ActionResult<PagedResult<FeedbackWithSentimentDto>>> GetFeedbackWithSentiment(
            [FromQuery] DateTime? from, [FromQuery] DateTime? to,
            [FromQuery] string? categoryId, [FromQuery] string? departmentId,
            [FromQuery] string? sentiment, [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            var result = await _sentimentService.GetFeedbackWithSentimentAsync(
                from, to, categoryId, departmentId, sentiment, page, pageSize, ct);
            return Ok(result);
        }
    }
}

