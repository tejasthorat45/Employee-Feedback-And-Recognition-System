using FeedbackSystem.API.Data;
using FeedbackSystem.API.DTOs;
using FeedbackSystem.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace FeedbackSystem.API.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly AppDbContext _db;

    public NotificationRepository(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<NotificationReadDto>> GetByUserIdAsync(
        string userId, bool? isRead, int page, int pageSize, CancellationToken ct)
    {
        var query = _db.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId);

        if (isRead.HasValue)
            query = query.Where(n => n.IsRead == isRead.Value);

        return await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new NotificationReadDto(
                n.NotificationId,
                n.UserId,
                n.Title,
                n.Message,
                n.IsRead,
                n.CreatedAt
            ))
            .ToListAsync(ct);
    }

    public Task<int> GetUnreadCountAsync(string userId, CancellationToken ct)
    {
        return _db.Notifications
            .AsNoTracking()
            .CountAsync(n => n.UserId == userId && !n.IsRead, ct);
    }

    public Task<int> GetTotalCountAsync(string userId, CancellationToken ct)
    {
        return _db.Notifications
            .AsNoTracking()
            .CountAsync(n => n.UserId == userId, ct);
    }

    public async Task<bool> MarkAsReadAsync(int notificationId, string userId, CancellationToken ct)
    {
        var notification = await _db.Notifications
            .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.UserId == userId, ct);

        if (notification == null) return false;

        notification.IsRead = true;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> MarkAllAsReadAsync(string userId, CancellationToken ct)
    {
        var notifications = await _db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync(ct);

        if (notifications.Count == 0) return true;

        foreach (var n in notifications)
            n.IsRead = true;

        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task CreateAsync(string userId, string title, string message, CancellationToken ct)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync(ct);
    }
}
