using FeedbackSystem.API.Data;
using FeedbackSystem.API.DTOs;
using Microsoft.EntityFrameworkCore;

namespace FeedbackSystem.API.Repositories
{
    public class FeedbackRepository : IFeedbackRepository
    {
        private readonly AppDbContext _db;
        public FeedbackRepository(AppDbContext db) => _db = db;

        public async Task<(IReadOnlyList<FeedbackItemDto> Items, int Total)> GetGivenAsync(
            string userId, DateTime? from, DateTime? to, string? categoryId, string? search, int page, int pageSize, CancellationToken ct)
        {
            var q = _db.Feedbacks.AsNoTracking().Where(f => f.FromUserId == userId);

            if (from.HasValue) q = q.Where(f => f.CreatedAt >= from.Value);
            if (to.HasValue)   q = q.Where(f => f.CreatedAt <= to.Value);
            if (!string.IsNullOrWhiteSpace(categoryId)) q = q.Where(f => f.CategoryId == categoryId);
            if (!string.IsNullOrWhiteSpace(search)) q = q.Where(f => f.Comments.Contains(search.Trim()));

            var total = await q.CountAsync(ct);

            var items = await q.OrderByDescending(f => f.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(f => new FeedbackItemDto(
                    f.FeedbackId,
                    f.FromUserId,
                    f.FromUser.FullName,
                    f.ToUserId,
                    f.ToUser.FullName,
                    f.CategoryId,
                    f.Category.CategoryName,
                    f.Comments,
                    f.IsAnonymous,
                    f.CreatedAt
                ))
                .ToListAsync(ct);

            return (items, total);
        }

        public async Task<(IReadOnlyList<FeedbackItemDto> Items, int Total)> GetReceivedAsync(
            string userId, DateTime? from, DateTime? to, string? categoryId, string? search, int page, int pageSize, CancellationToken ct)
        {
            var q = _db.Feedbacks.AsNoTracking().Where(f => f.ToUserId == userId);

            if (from.HasValue) q = q.Where(f => f.CreatedAt >= from.Value);
            if (to.HasValue)   q = q.Where(f => f.CreatedAt <= to.Value);
            if (!string.IsNullOrWhiteSpace(categoryId)) q = q.Where(f => f.CategoryId == categoryId);
            if (!string.IsNullOrWhiteSpace(search)) q = q.Where(f => f.Comments.Contains(search.Trim()));

            var total = await q.CountAsync(ct);

            var items = await q.OrderByDescending(f => f.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(f => new FeedbackItemDto(
                    f.FeedbackId,
                    f.FromUserId,
                    f.FromUser.FullName,
                    f.ToUserId,
                    f.ToUser.FullName,
                    f.CategoryId,
                    f.Category.CategoryName,
                    f.Comments,
                    f.IsAnonymous,
                    f.CreatedAt
                ))
                .ToListAsync(ct);

            return (items, total);
        }

        public async Task<(IReadOnlyList<FeedbackItemDto> Items, int Total)> GetAllAsync(
            DateTime? from, DateTime? to, string? categoryId, string? search, string? departmentScopeId,
            string? fromUserId, string? toUserId, int page, int pageSize, CancellationToken ct)
        {
            var q = ApplyAllFilters(from, to, categoryId, search, departmentScopeId, fromUserId, toUserId);

            var total = await q.CountAsync(ct);

            var items = await q.OrderByDescending(f => f.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(f => new FeedbackItemDto(
                    f.FeedbackId,
                    f.FromUserId,
                    f.FromUser.FullName,
                    f.ToUserId,
                    f.ToUser.FullName,
                    f.CategoryId,
                    f.Category.CategoryName,
                    f.Comments,
                    f.IsAnonymous,
                    f.CreatedAt
                ))
                .ToListAsync(ct);

            return (items, total);
        }

        // ✅ Count-only (same filters, no projection/paging)
        public Task<int> CountAllAsync(
            DateTime? from, DateTime? to, string? categoryId, string? search, string? departmentScopeId,
            string? fromUserId, string? toUserId, CancellationToken ct)
        {
            var q = ApplyAllFilters(from, to, categoryId, search, departmentScopeId, fromUserId, toUserId);
            return q.CountAsync(ct);
        }

        public async Task<(int Given, int Received, DateTime? LatestGivenAt, DateTime? LatestReceivedAt)> GetSummaryAsync(
            string userId, CancellationToken ct)
        {
            var givenCountTask = _db.Feedbacks.AsNoTracking().CountAsync(f => f.FromUserId == userId, ct);
            var receivedCountTask = _db.Feedbacks.AsNoTracking().CountAsync(f => f.ToUserId == userId, ct);

            var latestGivenAtTask = _db.Feedbacks.AsNoTracking()
                .Where(f => f.FromUserId == userId)
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => (DateTime?)f.CreatedAt)
                .FirstOrDefaultAsync(ct);

            var latestReceivedAtTask = _db.Feedbacks.AsNoTracking()
                .Where(f => f.ToUserId == userId)
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => (DateTime?)f.CreatedAt)
                .FirstOrDefaultAsync(ct);

            await Task.WhenAll(givenCountTask, receivedCountTask, latestGivenAtTask, latestReceivedAtTask);

            return (givenCountTask.Result, receivedCountTask.Result, latestGivenAtTask.Result, latestReceivedAtTask.Result);
        }

        // -------------- private helpers --------------
        private IQueryable<Entities.Feedback> ApplyAllFilters(
            DateTime? from, DateTime? to, string? categoryId, string? search, string? departmentScopeId,
            string? fromUserId, string? toUserId)
        {
            var q = _db.Feedbacks.AsNoTracking().AsQueryable();

            if (from.HasValue) q = q.Where(f => f.CreatedAt >= from.Value);
            if (to.HasValue)   q = q.Where(f => f.CreatedAt <= to.Value);
            if (!string.IsNullOrWhiteSpace(categoryId)) q = q.Where(f => f.CategoryId == categoryId);
            if (!string.IsNullOrWhiteSpace(search)) q = q.Where(f => f.Comments.Contains(search.Trim()));
            if (!string.IsNullOrWhiteSpace(fromUserId)) q = q.Where(f => f.FromUserId == fromUserId);
            if (!string.IsNullOrWhiteSpace(toUserId))     q = q.Where(f => f.ToUserId == toUserId);

            if (!string.IsNullOrWhiteSpace(departmentScopeId))
            {
                var depId = departmentScopeId;
                q = q.Where(f => f.FromUser.DepartmentId == depId || f.ToUser.DepartmentId == depId);
            }

            return q;
        }

        // ✅ Total count across entire system
        public Task<int> GetTotalCountAsync(CancellationToken ct) =>
            _db.Feedbacks.CountAsync(ct);

        // ✅ Category statistics
        public async Task<IReadOnlyList<CategoryStatsDto>> GetByCategoryAsync(
            DateTime? from, DateTime? to, string? departmentScopeId, string? userId, CancellationToken ct)
        {
            var q = _db.Feedbacks.AsNoTracking().AsQueryable();

            if (from.HasValue) q = q.Where(f => f.CreatedAt >= from.Value);
            if (to.HasValue)   q = q.Where(f => f.CreatedAt <= to.Value);

            if (!string.IsNullOrWhiteSpace(departmentScopeId))
            {
                var depId = departmentScopeId;
                q = q.Where(f => f.FromUser.DepartmentId == depId || f.ToUser.DepartmentId == depId);
            }

            if (!string.IsNullOrWhiteSpace(userId))
                q = q.Where(f => f.FromUserId == userId || f.ToUserId == userId);

            return await q
                .GroupBy(f => new { f.CategoryId, f.Category.CategoryName })
                .Select(g => new CategoryStatsDto(
                    g.Key.CategoryId,
                    g.Key.CategoryName,
                    g.Count(),
                    g.Max(f => (DateTime?)f.CreatedAt)
                ))
                .OrderByDescending(s => s.FeedbackCount)
                .ToListAsync(ct);
        }
    }
}