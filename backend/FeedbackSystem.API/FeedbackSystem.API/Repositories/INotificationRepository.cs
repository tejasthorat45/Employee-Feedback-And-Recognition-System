using FeedbackSystem.API.DTOs;

namespace FeedbackSystem.API.Repositories;

public interface INotificationRepository
{
    Task<IReadOnlyList<NotificationReadDto>> GetByUserIdAsync(string userId, bool? isRead, int page, int pageSize, CancellationToken ct);
    Task<int> GetUnreadCountAsync(string userId, CancellationToken ct);
    Task<int> GetTotalCountAsync(string userId, CancellationToken ct);
    Task<bool> MarkAsReadAsync(int notificationId, string userId, CancellationToken ct);
    Task<bool> MarkAllAsReadAsync(string userId, CancellationToken ct);
    Task CreateAsync(string userId, string title, string message, CancellationToken ct);
}
