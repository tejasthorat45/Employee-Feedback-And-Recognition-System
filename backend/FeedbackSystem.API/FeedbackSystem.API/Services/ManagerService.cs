using FeedbackSystem.API.Data;
using FeedbackSystem.API.DTOs;
using FeedbackSystem.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace FeedbackSystem.API.Services;

public class ManagerService : IManagerService
{
    private readonly AppDbContext _db;

    public ManagerService(AppDbContext db) => _db = db;

    public async Task<ManagerDashboardDto> GetDashboardAsync(string managerId, CancellationToken ct)
    {
        // Get manager's department
        var deptId = await _db.Users
            .Where(u => u.UserId == managerId)
            .Select(u => u.DepartmentId)
            .FirstOrDefaultAsync(ct);

        if (string.IsNullOrEmpty(deptId))
            throw new KeyNotFoundException("Manager not found");

        // Query feedback where either sender or receiver is in manager's department
        var feedbackQuery = _db.Feedbacks
            .Include(f => f.Reviews)
            .Where(f => f.FromUser.DepartmentId == deptId || f.ToUser.DepartmentId == deptId);

        var allFeedback = await feedbackQuery.ToListAsync(ct);

        var totalFeedback = allFeedback.Count;
        
        // Get latest review status for each feedback
        var pending = allFeedback.Count(f => !f.Reviews.Any() || f.Reviews.OrderByDescending(r => r.ReviewedAt).First().Status == "Pending");
        var acknowledged = allFeedback.Count(f => f.Reviews.Any() && f.Reviews.OrderByDescending(r => r.ReviewedAt).First().Status == "Acknowledged");
        var resolved = allFeedback.Count(f => f.Reviews.Any() && f.Reviews.OrderByDescending(r => r.ReviewedAt).First().Status == "Resolved");

        // Recognition stats
        var recognitionQuery = _db.Recognitions
            .Where(r => r.FromUser.DepartmentId == deptId || r.ToUser.DepartmentId == deptId);

        var totalRecognitions = await recognitionQuery.CountAsync(ct);
        var totalPoints = await recognitionQuery.SumAsync(r => r.Points, ct);

        // Engagement score = (acknowledged + resolved) / total * 100
        var engagementScore = totalFeedback > 0 
            ? (int)Math.Round((double)(acknowledged + resolved) / totalFeedback * 100) 
            : 0;

        return new ManagerDashboardDto(
            totalFeedback,
            pending,
            acknowledged,
            resolved,
            engagementScore,
            totalRecognitions,
            totalPoints
        );
    }

    public async Task<PagedResult<ManagerFeedbackItemDto>> GetFeedbackListAsync(
        string managerId, string? status, string? categoryId, string? search,
        int page, int pageSize, CancellationToken ct)
    {
        // Get manager's department
        var deptId = await _db.Users
            .Where(u => u.UserId == managerId)
            .Select(u => u.DepartmentId)
            .FirstOrDefaultAsync(ct);

        if (string.IsNullOrEmpty(deptId))
            throw new KeyNotFoundException("Manager not found");

        var query = _db.Feedbacks
            .Include(f => f.FromUser)
            .Include(f => f.ToUser)
            .Include(f => f.Category)
            .Include(f => f.Reviews).ThenInclude(r => r.Reviewer)
            .Where(f => f.FromUser.DepartmentId == deptId || f.ToUser.DepartmentId == deptId)
            .AsNoTracking();

        // Filter by category
        if (!string.IsNullOrWhiteSpace(categoryId))
            query = query.Where(f => f.CategoryId == categoryId);

        // Filter by search (comments or user name)
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(f => 
                f.Comments.ToLower().Contains(s) ||
                f.FromUser.FullName.ToLower().Contains(s) ||
                f.ToUser.FullName.ToLower().Contains(s));
        }

        // Get all with reviews to filter by status in memory (EF limitation with latest review)
        var allItems = await query.OrderByDescending(f => f.CreatedAt).ToListAsync(ct);

        // Calculate status for each feedback
        var itemsWithStatus = allItems.Select(f =>
        {
            var latestReview = f.Reviews.OrderByDescending(r => r.ReviewedAt).FirstOrDefault();
            var currentStatus = latestReview?.Status ?? "Pending";
            return new { Feedback = f, Status = currentStatus, LatestReview = latestReview };
        });

        // Filter by status if specified
        if (!string.IsNullOrWhiteSpace(status) && status != "All")
        {
            itemsWithStatus = itemsWithStatus.Where(x => x.Status == status);
        }

        var filtered = itemsWithStatus.ToList();
        var total = filtered.Count;

        // Paginate
        var paged = filtered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ManagerFeedbackItemDto(
                x.Feedback.FeedbackId,
                x.Feedback.FromUserId,
                x.Feedback.IsAnonymous ? "Anonymous" : x.Feedback.FromUser.FullName,
                x.Feedback.ToUserId,
                x.Feedback.ToUser.FullName,
                x.Feedback.CategoryId,
                x.Feedback.Category.CategoryName,
                x.Feedback.Comments,
                x.Feedback.IsAnonymous,
                x.Feedback.CreatedAt,
                x.Status,
                x.LatestReview?.ReviewedBy,
                x.LatestReview?.ReviewedAt,
                x.LatestReview?.Remarks
            ))
            .ToList();

        return new PagedResult<ManagerFeedbackItemDto>(page, pageSize, total, paged);
    }

    public async Task<bool> UpdateFeedbackStatusAsync(
        string managerId, int feedbackId, string newStatus, string? remarks, CancellationToken ct)
    {
        // Validate status
        var validStatuses = new[] { "Pending", "Acknowledged", "Resolved" };
        if (!validStatuses.Contains(newStatus))
            throw new ArgumentException("Invalid status. Must be Pending, Acknowledged, or Resolved.");

        // Get manager's department
        var deptId = await _db.Users
            .Where(u => u.UserId == managerId)
            .Select(u => u.DepartmentId)
            .FirstOrDefaultAsync(ct);

        if (string.IsNullOrEmpty(deptId))
            throw new KeyNotFoundException("Manager not found");

        // Get feedback and verify it belongs to manager's department
        var feedback = await _db.Feedbacks
            .Include(f => f.FromUser)
            .Include(f => f.ToUser)
            .FirstOrDefaultAsync(f => f.FeedbackId == feedbackId, ct);

        if (feedback == null)
            return false;

        // Check if feedback involves manager's department
        if (feedback.FromUser.DepartmentId != deptId && feedback.ToUser.DepartmentId != deptId)
            throw new UnauthorizedAccessException("You can only update feedback in your department.");

        // Create new review entry
        var review = new FeedbackReview
        {
            FeedbackId = feedbackId,
            ReviewedBy = managerId,
            Status = newStatus,
            Remarks = remarks,
            ReviewedAt = DateTime.UtcNow
        };

        _db.FeedbackReviews.Add(review);
        await _db.SaveChangesAsync(ct);

        return true;
    }

    public async Task<IReadOnlyList<RecentActivityDto>> GetRecentActivityAsync(
        string managerId, int count, CancellationToken ct)
    {
        // Get manager's department
        var deptId = await _db.Users
            .Where(u => u.UserId == managerId)
            .Select(u => u.DepartmentId)
            .FirstOrDefaultAsync(ct);

        if (string.IsNullOrEmpty(deptId))
            throw new KeyNotFoundException("Manager not found");

        // Get recent feedback
        var recentFeedback = await _db.Feedbacks
            .Include(f => f.FromUser)
            .Include(f => f.Category)
            .Include(f => f.Reviews)
            .Where(f => f.FromUser.DepartmentId == deptId || f.ToUser.DepartmentId == deptId)
            .OrderByDescending(f => f.CreatedAt)
            .Take(count)
            .Select(f => new RecentActivityDto(
                f.FeedbackId,
                "Feedback",
                f.Reviews.Any() && f.Reviews.OrderByDescending(r => r.ReviewedAt).First().Status == "Resolved" 
                    ? "Review Completed" 
                    : "New Feedback",
                f.IsAnonymous ? "Anonymous" : f.FromUser.FullName,
                f.Category.CategoryName,
                f.CreatedAt,
                f.Reviews.Any() 
                    ? f.Reviews.OrderByDescending(r => r.ReviewedAt).First().Status 
                    : "Pending"
            ))
            .ToListAsync(ct);

        // Get recent recognitions
        var recentRecognitions = await _db.Recognitions
            .Include(r => r.FromUser)
            .Include(r => r.Category)
            .Where(r => r.FromUser.DepartmentId == deptId || r.ToUser.DepartmentId == deptId)
            .OrderByDescending(r => r.CreatedAt)
            .Take(count)
            .Select(r => new RecentActivityDto(
                r.RecognitionId,
                "Recognition",
                "Recognition Sent",
                r.FromUser.FullName,
                r.Category.CategoryName,
                r.CreatedAt,
                "Completed"
            ))
            .ToListAsync(ct);

        // Combine and sort by date
        return recentFeedback
            .Concat(recentRecognitions)
            .OrderByDescending(a => a.CreatedAt)
            .Take(count)
            .ToList();
    }

    public async Task<IReadOnlyList<CategoryStatsDto>> GetCategoryDistributionAsync(
        string managerId, string? status, CancellationToken ct)
    {
        // Get manager's department
        var deptId = await _db.Users
            .Where(u => u.UserId == managerId)
            .Select(u => u.DepartmentId)
            .FirstOrDefaultAsync(ct);

        if (string.IsNullOrEmpty(deptId))
            throw new KeyNotFoundException("Manager not found");

        var query = _db.Feedbacks
            .Include(f => f.FromUser)
            .Include(f => f.ToUser)
            .Include(f => f.Category)
            .Include(f => f.Reviews)
            .Where(f => f.FromUser.DepartmentId == deptId || f.ToUser.DepartmentId == deptId);

        var allFeedback = await query.ToListAsync(ct);

        // Filter by status if specified
        if (!string.IsNullOrWhiteSpace(status) && status != "All")
        {
            allFeedback = allFeedback.Where(f =>
            {
                var latestReview = f.Reviews.OrderByDescending(r => r.ReviewedAt).FirstOrDefault();
                var currentStatus = latestReview?.Status ?? "Pending";
                return currentStatus == status;
            }).ToList();
        }

        // Group by category
        var categoryStats = allFeedback
            .GroupBy(f => new { f.CategoryId, f.Category.CategoryName })
            .Select(g => new CategoryStatsDto(
                g.Key.CategoryId,
                g.Key.CategoryName,
                g.Count(),
                g.Max(f => (DateTime?)f.CreatedAt)
            ))
            .OrderByDescending(c => c.FeedbackCount)
            .ToList();

        return categoryStats;
    }
}
