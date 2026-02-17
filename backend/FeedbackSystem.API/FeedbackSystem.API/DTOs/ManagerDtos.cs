using System.ComponentModel.DataAnnotations;

namespace FeedbackSystem.API.DTOs;

// Dashboard summary for managers
public record ManagerDashboardDto(
    int TotalFeedback,
    int PendingReviews,
    int Acknowledged,
    int Resolved,
    int EngagementScore,
    int TotalRecognitions,
    int TotalRecognitionPoints
);

// Update feedback status
public record UpdateFeedbackStatusDto(
    [Required] string Status  // "Pending", "Acknowledged", "Resolved"
);

// Feedback with status for manager view
public record ManagerFeedbackItemDto(
    int FeedbackId,
    string FromUserId,
    string FromUserName,
    string ToUserId,
    string ToUserName,
    string CategoryId,
    string CategoryName,
    string Comments,
    bool IsAnonymous,
    DateTime CreatedAt,
    string Status,
    string? ReviewedBy,
    DateTime? ReviewedAt,
    string? ReviewRemarks
);

// Recent activity item for dashboard
public record RecentActivityDto(
    int Id,
    string Type,  // "Feedback" or "Recognition"
    string Title,
    string UserName,
    string Detail,
    DateTime CreatedAt,
    string Status
);
