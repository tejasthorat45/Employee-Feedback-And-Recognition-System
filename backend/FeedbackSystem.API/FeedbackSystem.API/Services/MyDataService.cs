using FeedbackSystem.API.Data;
using FeedbackSystem.API.DTOs;
using FeedbackSystem.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace FeedbackSystem.API.Services;

public class MyDataService : IMyDataService
{
    private readonly AppDbContext _db;
    private readonly ILogger<MyDataService> _logger;

    public MyDataService(
        AppDbContext db,
        ILogger<MyDataService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<MyFeedbackSubmitResultDto> SubmitMyFeedbackAsync(
        string userId, 
        MyFeedbackSubmitDto dto, 
        CancellationToken ct)
    {
        // Validate target user exists
        var targetUser = await _db.Users.FindAsync(new object[] { dto.ToUserId }, ct);
        if (targetUser == null)
            throw new InvalidOperationException($"Target user '{dto.ToUserId}' not found.");

        // Validate category exists
        var category = await _db.Categories.FindAsync(new object[] { dto.CategoryId }, ct);
        if (category == null)
            throw new InvalidOperationException($"Category '{dto.CategoryId}' not found.");

        // Create feedback entity
        var feedback = new Feedback
        {
            FromUserId = userId,
            ToUserId = dto.ToUserId,
            CategoryId = dto.CategoryId,
            Comments = dto.Comments,
            IsAnonymous = dto.IsAnonymous ?? false,
            CreatedAt = DateTime.UtcNow
        };

        _db.Feedbacks.Add(feedback);
        await _db.SaveChangesAsync(ct);

        return new MyFeedbackSubmitResultDto(
            feedback.FeedbackId,
            true,
            "Feedback submitted successfully"
        );
    }

    public async Task<List<MyFeedbackDto>> GetMyFeedbackAsync(
        string userId, 
        string? direction, 
        CancellationToken ct)
    {
        var query = _db.Feedbacks
            .AsNoTracking()
            .Include(f => f.FromUser)
            .Include(f => f.ToUser)
            .Include(f => f.Category)
            .Where(f => f.FromUserId == userId || f.ToUserId == userId);

        // Filter by direction if specified
        if (!string.IsNullOrWhiteSpace(direction))
        {
            if (direction.Equals("given", StringComparison.OrdinalIgnoreCase))
                query = query.Where(f => f.FromUserId == userId);
            else if (direction.Equals("received", StringComparison.OrdinalIgnoreCase))
                query = query.Where(f => f.ToUserId == userId);
        }

        var feedbacks = await query
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(ct);

        return feedbacks.Select(f => new MyFeedbackDto(
            f.FeedbackId,
            f.FromUserId == userId ? "Given" : "Received",
            f.FromUserId == userId ? f.ToUserId : f.FromUserId,
            f.FromUserId == userId ? f.ToUser.FullName : f.FromUser.FullName,
            f.CategoryId,
            f.Category.CategoryName,
            f.Comments,
            f.IsAnonymous,
            f.CreatedAt
        )).ToList();
    }

    public async Task<MyRecognitionSubmitResultDto> SubmitMyRecognitionAsync(
        string userId, 
        MyRecognitionSubmitDto dto, 
        CancellationToken ct)
    {
        // Validate target user exists
        var targetUser = await _db.Users.FindAsync(new object[] { dto.ToUserId }, ct);
        if (targetUser == null)
            throw new InvalidOperationException($"Target user '{dto.ToUserId}' not found.");

        // Validate badge exists
        var badge = await _db.Badges.FindAsync(new object[] { dto.BadgeId }, ct);
        if (badge == null)
            throw new InvalidOperationException($"Badge '{dto.BadgeId}' not found.");

        // Create recognition entity
        var recognition = new Recognition
        {
            FromUserId = userId,
            ToUserId = dto.ToUserId,
            BadgeId = dto.BadgeId,
            Points = dto.Points,
            Message = dto.Message,
            CreatedAt = DateTime.UtcNow
        };

        _db.Recognitions.Add(recognition);
        await _db.SaveChangesAsync(ct);

        return new MyRecognitionSubmitResultDto(
            recognition.RecognitionId,
            true,
            "Recognition submitted successfully",
            dto.Points
        );
    }

    public async Task<List<MyRecognitionDto>> GetMyRecognitionAsync(
        string userId, 
        string? direction, 
        CancellationToken ct)
    {
        var query = _db.Recognitions
            .AsNoTracking()
            .Include(r => r.FromUser)
            .Include(r => r.ToUser)
            .Include(r => r.Badge)
            .Where(r => r.FromUserId == userId || r.ToUserId == userId);

        // Filter by direction if specified
        if (!string.IsNullOrWhiteSpace(direction))
        {
            if (direction.Equals("given", StringComparison.OrdinalIgnoreCase))
                query = query.Where(r => r.FromUserId == userId);
            else if (direction.Equals("received", StringComparison.OrdinalIgnoreCase))
                query = query.Where(r => r.ToUserId == userId);
        }

        var recognitions = await query
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);

        return recognitions.Select(r => new MyRecognitionDto(
            r.RecognitionId,
            r.FromUserId == userId ? "Given" : "Received",
            r.FromUserId == userId ? r.ToUserId : r.FromUserId,
            r.FromUserId == userId ? r.ToUser.FullName : r.FromUser.FullName,
            r.BadgeId,
            r.Badge.BadgeName,
            r.Points,
            r.Message,
            r.CreatedAt
        )).ToList();
    }

    public async Task<MySummaryDto> GetMySummaryAsync(string userId, CancellationToken ct)
    {
        // Execute queries sequentially to avoid DbContext concurrency issues
        var feedbackGiven = await _db.Feedbacks.CountAsync(f => f.FromUserId == userId, ct);
        var feedbackReceived = await _db.Feedbacks.CountAsync(f => f.ToUserId == userId, ct);
        var recognitionGiven = await _db.Recognitions.CountAsync(r => r.FromUserId == userId, ct);
        var recognitionReceived = await _db.Recognitions.CountAsync(r => r.ToUserId == userId, ct);
        
        var pointsGiven = await _db.Recognitions
            .Where(r => r.FromUserId == userId)
            .SumAsync(r => (int?)r.Points, ct) ?? 0;
            
        var pointsReceived = await _db.Recognitions
            .Where(r => r.ToUserId == userId)
            .SumAsync(r => (int?)r.Points, ct) ?? 0;

        var lastFeedbackDate = await _db.Feedbacks
            .Where(f => f.FromUserId == userId || f.ToUserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => (DateTime?)f.CreatedAt)
            .FirstOrDefaultAsync(ct);

        var lastRecognitionDate = await _db.Recognitions
            .Where(r => r.FromUserId == userId || r.ToUserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => (DateTime?)r.CreatedAt)
            .FirstOrDefaultAsync(ct);

        var lastActivity = lastFeedbackDate.HasValue && lastRecognitionDate.HasValue
            ? (lastFeedbackDate > lastRecognitionDate ? lastFeedbackDate : lastRecognitionDate)
            : lastFeedbackDate ?? lastRecognitionDate;

        return new MySummaryDto(
            feedbackGiven,
            feedbackReceived,
            recognitionGiven,
            recognitionReceived,
            pointsGiven,
            pointsReceived,
            lastActivity
        );
    }
}
