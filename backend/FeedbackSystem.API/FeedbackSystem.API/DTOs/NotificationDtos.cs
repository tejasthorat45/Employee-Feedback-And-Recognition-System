using System.ComponentModel.DataAnnotations;

namespace FeedbackSystem.API.DTOs;

public record NotificationReadDto(
    int NotificationId,
    string UserId,
    string? Title,
    string? Message,
    bool IsRead,
    DateTime CreatedAt
);

public record NotificationCreateDto(
    [Required] string UserId,
    [Required] string Title,
    [Required] string Message
);

public record MarkNotificationReadDto(
    [Required] bool IsRead
);

public record NotificationCountDto(
    int Total,
    int Unread
);
