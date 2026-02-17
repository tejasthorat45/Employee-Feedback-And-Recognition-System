using FeedbackSystem.API.DTOs;

namespace FeedbackSystem.API.Services
{
    public interface IBadgeService
    {
        Task<BadgeDto?> GetByIdAsync(string badgeId, CancellationToken ct);
        Task<PagedBadgeResult> GetAllAsync(bool? isActive, string? search, int page, int pageSize, CancellationToken ct);
        Task<BadgeDto> CreateAsync(CreateBadgeDto dto, CancellationToken ct);
        Task<BadgeDto> UpdateAsync(string badgeId, UpdateBadgeDto dto, CancellationToken ct);
        Task DeleteAsync(string badgeId, CancellationToken ct);
        Task<int> GetTotalCountAsync(CancellationToken ct);
    }
}
