using FeedbackSystem.API.DTOs;

namespace FeedbackSystem.API.Repositories
{
    public interface IBadgeRepository
    {
        Task<BadgeDto?> GetByIdAsync(string badgeId, CancellationToken ct);
        Task<(IReadOnlyList<BadgeDto> Items, int Total)> GetAllAsync(bool? isActive, string? search, int page, int pageSize, CancellationToken ct);
        Task<bool> ExistsAsync(string badgeId, CancellationToken ct);
        Task<bool> BadgeNameExistsAsync(string badgeName, string? excludeBadgeId, CancellationToken ct);
        Task CreateAsync(CreateBadgeDto dto, CancellationToken ct);
        Task<bool> UpdateAsync(string badgeId, UpdateBadgeDto dto, CancellationToken ct);
        Task<bool> DeleteAsync(string badgeId, CancellationToken ct);
        Task<int> GetTotalCountAsync(CancellationToken ct);
    }
}
