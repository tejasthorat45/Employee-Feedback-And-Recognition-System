using FeedbackSystem.API.DTOs;
using FeedbackSystem.API.Repositories;

namespace FeedbackSystem.API.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repo;

    public NotificationService(INotificationRepository repo) => _repo = repo;

    public Task<IReadOnlyList<NotificationReadDto>> GetNotificationsAsync(
        string userId, bool? isRead, int page, int pageSize, CancellationToken ct)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        return _repo.GetByUserIdAsync(userId, isRead, page, pageSize, ct);
    }

    public async Task<NotificationCountDto> GetCountAsync(string userId, CancellationToken ct)
    {
        var total = await _repo.GetTotalCountAsync(userId, ct);
        var unread = await _repo.GetUnreadCountAsync(userId, ct);
        return new NotificationCountDto(total, unread);
    }

    public Task<bool> MarkAsReadAsync(int notificationId, string userId, CancellationToken ct)
    {
        return _repo.MarkAsReadAsync(notificationId, userId, ct);
    }

    public Task<bool> MarkAllAsReadAsync(string userId, CancellationToken ct)
    {
        return _repo.MarkAllAsReadAsync(userId, ct);
    }

    public Task CreateNotificationAsync(string userId, string title, string message, CancellationToken ct)
    {
        return _repo.CreateAsync(userId, title, message, ct);
    }
}
