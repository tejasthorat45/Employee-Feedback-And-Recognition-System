using FeedbackSystem.API.DTOs;

namespace FeedbackSystem.API.Services
{
    public interface IDashboardService
    {
        Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken ct = default);
        Task<IReadOnlyList<CategoryStatsDto>> GetFeedbackByCategoryAsync(CancellationToken ct = default);
        Task<IReadOnlyList<RecognitionBadgeStatsDto>> GetRecognitionByBadgeAsync(CancellationToken ct = default);
        Task<MonthlyTrendDto> GetMonthlyTrendsAsync(int months = 6, CancellationToken ct = default);
        Task<IReadOnlyList<DepartmentCountDto>> GetDepartmentFeedbackCountsAsync(DateTime? from = null, DateTime? to = null, CancellationToken ct = default);
        Task<IReadOnlyList<TopEmployeeDto>> GetTopEmployeesByPointsAsync(int limit = 10, DateTime? from = null, DateTime? to = null, CancellationToken ct = default);
        Task<IReadOnlyList<DepartmentRecognitionDto>> GetDepartmentRecognitionStatsAsync(DateTime? from = null, DateTime? to = null, CancellationToken ct = default);
        Task<IReadOnlyList<CategoryAverageScoreDto>> GetCategoryAverageScoresAsync(DateTime? from = null, DateTime? to = null, CancellationToken ct = default);
    }
}
