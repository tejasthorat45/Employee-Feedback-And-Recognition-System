using FeedbackSystem.API.Data;
using FeedbackSystem.API.DTOs;
using Microsoft.EntityFrameworkCore;

namespace FeedbackSystem.API.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _db;

        public DashboardService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken ct = default)
        {
            var totalUsers = await _db.Users.CountAsync(ct);
            var activeUsers = await _db.Users.CountAsync(u => u.IsActive, ct);
            var totalFeedback = await _db.Feedbacks.CountAsync(ct);
            var totalRecognition = await _db.Recognitions.CountAsync(ct);

            return new DashboardSummaryDto(totalUsers, activeUsers, totalFeedback, totalRecognition);
        }

        public async Task<IReadOnlyList<CategoryStatsDto>> GetFeedbackByCategoryAsync(CancellationToken ct = default)
        {
            var feedbacks = await _db.Feedbacks
                .Include(f => f.Category)
                .ToListAsync(ct);

            var stats = feedbacks
                .GroupBy(f => new { f.CategoryId, f.Category.CategoryName })
                .Select(g => new CategoryStatsDto(
                    g.Key.CategoryId,
                    g.Key.CategoryName,
                    g.Count(),
                    g.Max(f => (DateTime?)f.CreatedAt)
                ))
                .OrderByDescending(s => s.FeedbackCount)
                .ToList();

            return stats;
        }

        public async Task<IReadOnlyList<RecognitionBadgeStatsDto>> GetRecognitionByBadgeAsync(CancellationToken ct = default)
        {
            var recognitions = await _db.Recognitions
                .Include(r => r.Badge)
                .ToListAsync(ct);

            var stats = recognitions
                .GroupBy(r => new { r.BadgeId, r.Badge.BadgeName })
                .Select(g => new RecognitionBadgeStatsDto(
                    g.Key.BadgeId,
                    g.Key.BadgeName,
                    g.Count(),
                    g.Max(r => (DateTime?)r.CreatedAt)
                ))
                .OrderByDescending(s => s.RecognitionCount)
                .ToList();

            return stats;
        }

        public async Task<MonthlyTrendDto> GetMonthlyTrendsAsync(int months = 6, CancellationToken ct = default)
        {
            var labels = new List<string>();
            var feedbackCounts = new List<int>();
            var recognitionCounts = new List<int>();

            for (int i = months - 1; i >= 0; i--)
            {
                var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(-i);
                var monthEnd = monthStart.AddMonths(1);

                labels.Add(monthStart.ToString("MMM"));

                var feedbackCount = await _db.Feedbacks
                    .CountAsync(f => f.CreatedAt >= monthStart && f.CreatedAt < monthEnd, ct);
                feedbackCounts.Add(feedbackCount);

                var recognitionCount = await _db.Recognitions
                    .CountAsync(r => r.CreatedAt >= monthStart && r.CreatedAt < monthEnd, ct);
                recognitionCounts.Add(recognitionCount);
            }

            return new MonthlyTrendDto(labels.ToArray(), feedbackCounts.ToArray(), recognitionCounts.ToArray());
        }

        public async Task<IReadOnlyList<DepartmentCountDto>> GetDepartmentFeedbackCountsAsync(
            DateTime? from = null, DateTime? to = null, CancellationToken ct = default)
        {
            var query = _db.Feedbacks
                .Include(f => f.ToUser)
                    .ThenInclude(u => u.Department)
                .AsQueryable();

            if (from.HasValue)
                query = query.Where(f => f.CreatedAt >= from.Value);
            if (to.HasValue)
                query = query.Where(f => f.CreatedAt <= to.Value);

            var feedbacks = await query.ToListAsync(ct);

            var stats = feedbacks
                .Where(f => f.ToUser?.Department != null)
                .GroupBy(f => new { f.ToUser.DepartmentId, f.ToUser.Department.DepartmentName })
                .Select(g => new DepartmentCountDto(
                    g.Key.DepartmentId,
                    g.Key.DepartmentName,
                    g.Count()
                ))
                .OrderByDescending(d => d.Count)
                .ToList();

            return stats;
        }

        public async Task<IReadOnlyList<TopEmployeeDto>> GetTopEmployeesByPointsAsync(
            int limit = 10, DateTime? from = null, DateTime? to = null, CancellationToken ct = default)
        {
            var query = _db.Recognitions
                .Include(r => r.ToUser)
                .AsQueryable();

            if (from.HasValue)
                query = query.Where(r => r.CreatedAt >= from.Value);
            if (to.HasValue)
                query = query.Where(r => r.CreatedAt <= to.Value);

            var recognitions = await query.ToListAsync(ct);

            var stats = recognitions
                .Where(r => r.ToUser != null)
                .GroupBy(r => new { r.ToUserId, r.ToUser.FullName })
                .Select(g => new TopEmployeeDto(
                    g.Key.ToUserId,
                    g.Key.FullName,
                    g.Sum(r => r.Points)
                ))
                .OrderByDescending(e => e.Points)
                .Take(limit)
                .ToList();

            return stats;
        }

        public async Task<IReadOnlyList<DepartmentRecognitionDto>> GetDepartmentRecognitionStatsAsync(
            DateTime? from = null, DateTime? to = null, CancellationToken ct = default)
        {
            var query = _db.Recognitions
                .Include(r => r.FromUser).ThenInclude(u => u.Department)
                .Include(r => r.ToUser).ThenInclude(u => u.Department)
                .AsQueryable();

            if (from.HasValue)
                query = query.Where(r => r.CreatedAt >= from.Value);
            if (to.HasValue)
                query = query.Where(r => r.CreatedAt <= to.Value);

            var recognitions = await query.ToListAsync(ct);

            var given = recognitions
                .Where(r => r.FromUser?.Department != null)
                .GroupBy(r => new { r.FromUser.DepartmentId, r.FromUser.Department.DepartmentName })
                .Select(g => new { g.Key.DepartmentId, g.Key.DepartmentName, Count = g.Count() })
                .ToList();

            var received = recognitions
                .Where(r => r.ToUser?.Department != null)
                .GroupBy(r => new { r.ToUser.DepartmentId, r.ToUser.Department.DepartmentName })
                .Select(g => new { g.Key.DepartmentId, g.Key.DepartmentName, Count = g.Count() })
                .ToList();

            var allDepts = given.Select(g => g.DepartmentId)
                .Union(received.Select(r => r.DepartmentId))
                .Distinct();

            var stats = allDepts.Select(deptId =>
            {
                var givenItem = given.FirstOrDefault(g => g.DepartmentId == deptId);
                var receivedItem = received.FirstOrDefault(r => r.DepartmentId == deptId);
                var deptName = givenItem?.DepartmentName ?? receivedItem?.DepartmentName ?? "";

                return new DepartmentRecognitionDto(
                    deptId,
                    deptName,
                    givenItem?.Count ?? 0,
                    receivedItem?.Count ?? 0
                );
            }).OrderByDescending(d => d.GivenCount + d.ReceivedCount).ToList();

            return stats;
        }

        public async Task<IReadOnlyList<CategoryAverageScoreDto>> GetCategoryAverageScoresAsync(
            DateTime? from = null, DateTime? to = null, CancellationToken ct = default)
        {
            var query = _db.Recognitions
                .Include(r => r.Badge)
                .AsQueryable();

            if (from.HasValue)
                query = query.Where(r => r.CreatedAt >= from.Value);
            if (to.HasValue)
                query = query.Where(r => r.CreatedAt <= to.Value);

            var recognitions = await query.ToListAsync(ct);

            var stats = recognitions
                .Where(r => r.Badge != null)
                .GroupBy(r => new { r.BadgeId, r.Badge.BadgeName })
                .Select(g => new CategoryAverageScoreDto(
                    g.Key.BadgeId,
                    g.Key.BadgeName,
                    Math.Round(g.Average(r => (double)r.Points), 2)
                ))
                .OrderByDescending(c => c.AverageScore)
                .ToList();

            return stats;
        }
    }
}
