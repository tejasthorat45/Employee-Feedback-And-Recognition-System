using FeedbackSystem.API.DTOs;
using FeedbackSystem.API.Repositories;

namespace FeedbackSystem.API.Services
{
    public class BadgeService : IBadgeService
    {
        private readonly IBadgeRepository _repo;

        public BadgeService(IBadgeRepository repo) => _repo = repo;

        public async Task<BadgeDto?> GetByIdAsync(string badgeId, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(badgeId))
                throw new ArgumentException("Badge ID is required", nameof(badgeId));

            return await _repo.GetByIdAsync(badgeId, ct);
        }

        public async Task<PagedBadgeResult> GetAllAsync(bool? isActive, string? search, int page, int pageSize, CancellationToken ct)
        {
            // Normalize pagination
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var (items, total) = await _repo.GetAllAsync(isActive, search, page, pageSize, ct);
            return new PagedBadgeResult(page, pageSize, total, items);
        }

        public async Task<BadgeDto> CreateAsync(CreateBadgeDto dto, CancellationToken ct)
        {
            // Validate badge ID format
            if (string.IsNullOrWhiteSpace(dto.BadgeId))
                throw new ArgumentException("Badge ID is required");

            // Check if badge ID already exists
            if (await _repo.ExistsAsync(dto.BadgeId, ct))
                throw new InvalidOperationException($"Badge with ID '{dto.BadgeId}' already exists");

            // Check if badge name already exists
            if (await _repo.BadgeNameExistsAsync(dto.BadgeName, null, ct))
                throw new InvalidOperationException($"Badge with name '{dto.BadgeName}' already exists");

            await _repo.CreateAsync(dto, ct);

            var created = await _repo.GetByIdAsync(dto.BadgeId, ct);
            return created ?? throw new InvalidOperationException("Failed to create badge");
        }

        public async Task<BadgeDto> UpdateAsync(string badgeId, UpdateBadgeDto dto, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(badgeId))
                throw new ArgumentException("Badge ID is required", nameof(badgeId));

            // Check if badge exists
            var existing = await _repo.GetByIdAsync(badgeId, ct);
            if (existing == null)
                throw new KeyNotFoundException($"Badge with ID '{badgeId}' not found");

            // Check if new name conflicts with existing badges
            if (await _repo.BadgeNameExistsAsync(dto.BadgeName, badgeId, ct))
                throw new InvalidOperationException($"Badge with name '{dto.BadgeName}' already exists");

            var updated = await _repo.UpdateAsync(badgeId, dto, ct);
            if (!updated)
                throw new InvalidOperationException("Failed to update badge");

            var result = await _repo.GetByIdAsync(badgeId, ct);
            return result ?? throw new InvalidOperationException("Failed to retrieve updated badge");
        }

        public async Task DeleteAsync(string badgeId, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(badgeId))
                throw new ArgumentException("Badge ID is required", nameof(badgeId));

            // Check if badge exists
            var existing = await _repo.GetByIdAsync(badgeId, ct);
            if (existing == null)
                throw new KeyNotFoundException($"Badge with ID '{badgeId}' not found");

            var deleted = await _repo.DeleteAsync(badgeId, ct);
            if (!deleted)
                throw new InvalidOperationException("Cannot delete badge that is in use by recognitions");
        }

        public async Task<int> GetTotalCountAsync(CancellationToken ct)
        {
            return await _repo.GetTotalCountAsync(ct);
        }
    }
}
