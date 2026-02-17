using FeedbackSystem.API.DTOs;

namespace FeedbackSystem.API.Repositories
{
    public interface IFeedbackRepository
    {
        Task<(IReadOnlyList<FeedbackItemDto> Items, int Total)> GetGivenAsync(
            string userId, DateTime? from, DateTime? to, string? categoryId, string? search, int page, int pageSize, CancellationToken ct);

        Task<(IReadOnlyList<FeedbackItemDto> Items, int Total)> GetReceivedAsync(
            string userId, DateTime? from, DateTime? to, string? categoryId, string? search, int page, int pageSize, CancellationToken ct);

        // Admin/Manager "get all" with optional department scoping
        Task<(IReadOnlyList<FeedbackItemDto> Items, int Total)> GetAllAsync(
            DateTime? from, DateTime? to, string? categoryId, string? search, string? departmentScopeId,
            string? fromUserId, string? toUserId, int page, int pageSize, CancellationToken ct);

        // ✅ Count-only with same filters
        Task<int> CountAllAsync(
            DateTime? from, DateTime? to, string? categoryId, string? search, string? departmentScopeId,
            string? fromUserId, string? toUserId, CancellationToken ct);

        Task<(int Given, int Received, DateTime? LatestGivenAt, DateTime? LatestReceivedAt)> GetSummaryAsync(
            string userId, CancellationToken ct);

        // ✅ Total count across entire system
        Task<int> GetTotalCountAsync(CancellationToken ct);

        // ✅ Category statistics
        Task<IReadOnlyList<CategoryStatsDto>> GetByCategoryAsync(
            DateTime? from, DateTime? to, string? departmentScopeId, string? userId, CancellationToken ct);
    }
}
