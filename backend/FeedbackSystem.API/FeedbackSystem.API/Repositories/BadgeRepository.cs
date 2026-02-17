using FeedbackSystem.API.Data;
using FeedbackSystem.API.DTOs;
using FeedbackSystem.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace FeedbackSystem.API.Repositories
{
    public class BadgeRepository : IBadgeRepository
    {
        private readonly AppDbContext _db;

        public BadgeRepository(AppDbContext db) => _db = db;

        public async Task<BadgeDto?> GetByIdAsync(string badgeId, CancellationToken ct)
        {
            return await _db.Badges
                .AsNoTracking()
                .Where(b => b.BadgeId == badgeId)
                .Select(b => new BadgeDto(
                    b.BadgeId,
                    b.BadgeName,
                    b.Description,
                    b.IconClass,
                    b.IsActive,
                    b.CreatedAt
                ))
                .FirstOrDefaultAsync(ct);
        }

        public async Task<(IReadOnlyList<BadgeDto> Items, int Total)> GetAllAsync(
            bool? isActive, string? search, int page, int pageSize, CancellationToken ct)
        {
            var query = _db.Badges.AsNoTracking().AsQueryable();

            // Filter by active status
            if (isActive.HasValue)
                query = query.Where(b => b.IsActive == isActive.Value);

            // Search by name or description
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchTerm = search.Trim();
                query = query.Where(b => b.BadgeName.Contains(searchTerm) || 
                                        (b.Description != null && b.Description.Contains(searchTerm)));
            }

            var total = await query.CountAsync(ct);

            var items = await query
                .OrderBy(b => b.BadgeName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new BadgeDto(
                    b.BadgeId,
                    b.BadgeName,
                    b.Description,
                    b.IconClass,
                    b.IsActive,
                    b.CreatedAt
                ))
                .ToListAsync(ct);

            return (items, total);
        }

        public async Task<bool> ExistsAsync(string badgeId, CancellationToken ct)
        {
            return await _db.Badges.AnyAsync(b => b.BadgeId == badgeId, ct);
        }

        public async Task<bool> BadgeNameExistsAsync(string badgeName, string? excludeBadgeId, CancellationToken ct)
        {
            var query = _db.Badges.Where(b => b.BadgeName == badgeName);
            
            if (!string.IsNullOrWhiteSpace(excludeBadgeId))
                query = query.Where(b => b.BadgeId != excludeBadgeId);

            return await query.AnyAsync(ct);
        }

        public async Task CreateAsync(CreateBadgeDto dto, CancellationToken ct)
        {
            var badge = new Badge
            {
                BadgeId = dto.BadgeId,
                BadgeName = dto.BadgeName,
                Description = dto.Description,
                IconClass = dto.IconClass,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _db.Badges.Add(badge);
            await _db.SaveChangesAsync(ct);
        }

        public async Task<bool> UpdateAsync(string badgeId, UpdateBadgeDto dto, CancellationToken ct)
        {
            var badge = await _db.Badges.FirstOrDefaultAsync(b => b.BadgeId == badgeId, ct);
            if (badge == null)
                return false;

            badge.BadgeName = dto.BadgeName;
            badge.Description = dto.Description;
            badge.IconClass = dto.IconClass;
            badge.IsActive = dto.IsActive;

            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> DeleteAsync(string badgeId, CancellationToken ct)
        {
            var badge = await _db.Badges.FirstOrDefaultAsync(b => b.BadgeId == badgeId, ct);
            if (badge == null)
                return false;

            // Check if badge is in use
            var inUse = await _db.Recognitions.AnyAsync(r => r.BadgeId == badgeId, ct);
            if (inUse)
                return false; // Cannot delete badge that's in use

            _db.Badges.Remove(badge);
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<int> GetTotalCountAsync(CancellationToken ct)
        {
            return await _db.Badges.CountAsync(ct);
        }
    }
}
