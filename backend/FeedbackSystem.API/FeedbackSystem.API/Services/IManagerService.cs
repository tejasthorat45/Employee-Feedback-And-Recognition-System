using FeedbackSystem.API.DTOs;

namespace FeedbackSystem.API.Services;

public interface IManagerService
{
    // Dashboard
    Task<ManagerDashboardDto> GetDashboardAsync(string managerId, CancellationToken ct);
    
    // Feedback with status
    Task<PagedResult<ManagerFeedbackItemDto>> GetFeedbackListAsync(
        string managerId, string? status, string? categoryId, string? search, 
        int page, int pageSize, CancellationToken ct);
    
    // Update status
    Task<bool> UpdateFeedbackStatusAsync(string managerId, int feedbackId, string newStatus, string? remarks, CancellationToken ct);
    
    // Recent activity
    Task<IReadOnlyList<RecentActivityDto>> GetRecentActivityAsync(string managerId, int count, CancellationToken ct);
    
    // Category distribution
    Task<IReadOnlyList<CategoryStatsDto>> GetCategoryDistributionAsync(
        string managerId, string? status, CancellationToken ct);
}
