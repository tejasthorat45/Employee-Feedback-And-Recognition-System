using FeedbackSystem.API.DTOs;

namespace FeedbackSystem.API.Services
{
    public interface IInsightsService
    {
        // Per-user
        Task<PagedResult<FeedbackItemDto>> GetFeedbackGivenAsync(
            string requesterUserId, bool isAdmin,
            string userId, DateTime? from, DateTime? to, string? categoryId, string? search, int page, int pageSize, CancellationToken ct);

        Task<PagedResult<FeedbackItemDto>> GetFeedbackReceivedAsync(
            string requesterUserId, bool isAdmin,
            string userId, DateTime? from, DateTime? to, string? categoryId, string? search, int page, int pageSize, CancellationToken ct);

        Task<PagedResult<RecognitionItemDto>> GetRecognitionsGivenAsync(
            string requesterUserId, bool isAdmin,
            string userId, DateTime? from, DateTime? to, string? search, int page, int pageSize, CancellationToken ct);

        Task<PagedResult<RecognitionItemDto>> GetRecognitionsReceivedAsync(
            string requesterUserId, bool isAdmin,
            string userId, DateTime? from, DateTime? to, string? search, int page, int pageSize, CancellationToken ct);

        Task<UserInsightSummaryDto> GetSummaryAsync(
            string requesterUserId, bool isAdmin, string userId, CancellationToken ct);

        // "Get all" across users
        Task<PagedResult<FeedbackItemDto>> GetAllFeedbackAsync(
            string requesterUserId, bool isAdmin, FeedbackAllFilter filter, CancellationToken ct);

        Task<PagedResult<RecognitionItemDto>> GetAllRecognitionsAsync(
            string requesterUserId, bool isAdmin, RecognitionAllFilter filter, CancellationToken ct);

        // ✅ Count-only across users
        Task<CountResultDto> GetAllFeedbackCountAsync(
            string requesterUserId, bool isAdmin, FeedbackAllFilter filter, CancellationToken ct);

        Task<CountResultDto> GetAllRecognitionsCountAsync(
            string requesterUserId, bool isAdmin, RecognitionAllFilter filter, CancellationToken ct);

        // ✅ Category-based statistics
        Task<IReadOnlyList<CategoryStatsDto>> GetFeedbackByCategoryAsync(
            string requesterUserId, bool isAdmin, CategoryStatsFilter filter, CancellationToken ct);

        Task<IReadOnlyList<RecognitionBadgeStatsDto>> GetRecognitionsByBadgeAsync(
            string requesterUserId, bool isAdmin, CategoryStatsFilter filter, CancellationToken ct);

        // ✅ Parameterless by-category (all data, no filters)
        Task<IReadOnlyList<CategoryStatsDto>> GetFeedbackByCategoryAsync(CancellationToken ct);
        Task<IReadOnlyList<RecognitionBadgeStatsDto>> GetRecognitionsByBadgeAsync(CancellationToken ct);
    }
}