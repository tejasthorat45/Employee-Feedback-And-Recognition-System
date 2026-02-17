using FeedbackSystem.API.DTOs;

namespace FeedbackSystem.API.Services;

public interface INotificationService
{
    Task<IReadOnlyList<NotificationReadDto>> GetNotificationsAsync(string userId, bool? isRead, int page, int pageSize, CancellationToken ct);
    Task<NotificationCountDto> GetCountAsync(string userId, CancellationToken ct);
    Task<bool> MarkAsReadAsync(int notificationId, string userId, CancellationToken ct);
    Task<bool> MarkAllAsReadAsync(string userId, CancellationToken ct);
    Task CreateNotificationAsync(string userId, string title, string message, CancellationToken ct);
}
