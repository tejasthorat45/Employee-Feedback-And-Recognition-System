using System.ComponentModel.DataAnnotations;

namespace FeedbackSystem.API.DTOs;

/// <summary>
/// Request DTO for employee submitting their own feedback
/// </summary>
public record MyFeedbackSubmitDto(
    [Required] string ToUserId,
    [Required] string CategoryId,
    [Required] string Comments,
    bool? IsAnonymous
);

/// <summary>
/// Request DTO for employee submitting their own recognition
/// </summary>
public record MyRecognitionSubmitDto(
    [Required] string ToUserId,
    [Required] string BadgeId,
    [Required] [Range(1, 10)] int Points,
    [Required] string Message
);

/// <summary>
/// Response DTO for "My Feedback" items
/// </summary>
public record MyFeedbackDto(
    int FeedbackId,
    string Direction,  // "Given" or "Received"
    string OtherUserId,
    string OtherUserName,
    string CategoryId,
    string CategoryName,
    string Comments,
    bool IsAnonymous,
    DateTime CreatedAt
);

/// <summary>
/// Response DTO for "My Recognition" items
/// </summary>
public record MyRecognitionDto(
    int RecognitionId,
    string Direction,  // "Given" or "Received"
    string OtherUserId,
    string OtherUserName,
    string BadgeId,
    string BadgeName,
    int Points,
    string Message,
    DateTime CreatedAt
);

/// <summary>
/// Summary of user's feedback and recognition stats
/// </summary>
public record MySummaryDto(
    int FeedbackGivenCount,
    int FeedbackReceivedCount,
    int RecognitionGivenCount,
    int RecognitionReceivedCount,
    int TotalPointsGiven,
    int TotalPointsReceived,
    DateTime? LastActivityAt
);

/// <summary>
/// Result of feedback submission
/// </summary>
public record MyFeedbackSubmitResultDto(
    int FeedbackId,
    bool Success,
    string Message
);

/// <summary>
/// Result of recognition submission
/// </summary>
public record MyRecognitionSubmitResultDto(
    int RecognitionId,
    bool Success,
    string Message,
    int PointsAwarded
);
