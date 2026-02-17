using System.ComponentModel.DataAnnotations;

namespace FeedbackSystem.API.DTOs
{
    // Generic paged wrapper
    public record PagedResult<T>(
        int Page,
        int PageSize,
        int TotalCount,
        IReadOnlyList<T> Items
    );

    // Flat list item for Feedback
    public record FeedbackItemDto(
        int FeedbackId,
        string FromUserId,
        string FromUserName,
        string ToUserId,
        string ToUserName,
        string CategoryId,
        string CategoryName,
        string Comments,
        bool IsAnonymous,
        DateTime CreatedAt
    );

    // Flat list item for Recognition
    public record RecognitionItemDto(
        int RecognitionId,
        string FromUserId,
        string FromUserName,
        string ToUserId,
        string ToUserName,
        string BadgeId,
        string BadgeName,
        int Points,
        string Message,
        DateTime CreatedAt
    );

    // Compact numbers for dashboards (per user)
    public record UserInsightSummaryDto(
        string UserId,
        int FeedbackGivenCount,
        int FeedbackReceivedCount,
        int RecognitionGivenCount,
        int RecognitionReceivedCount,
        DateTime? LatestFeedbackGivenAt,
        DateTime? LatestFeedbackReceivedAt,
        DateTime? LatestRecognitionGivenAt,
        DateTime? LatestRecognitionReceivedAt
    );

    // Filters for "get all" endpoints
    public record FeedbackAllFilter(
        DateTime? From,
        DateTime? To,
        string? CategoryId,
        string? Search,
        string? DepartmentId,
        string? FromUserId,
        string? ToUserId,
        int Page = 1,
        int PageSize = 20
    );

    public record RecognitionAllFilter(
        DateTime? From,
        DateTime? To,
        string? Search,
        string? DepartmentId,
        string? FromUserId,
        string? ToUserId,
        int Page = 1,
        int PageSize = 20
    );

    // For standalone count endpoints
    public record CountResultDto(int TotalCount);

    // Category-based statistics
    public record CategoryStatsDto(
        string CategoryId,
        string CategoryName,
        int FeedbackCount,
        DateTime? LatestFeedbackAt
    );

    public record RecognitionBadgeStatsDto(
        string BadgeId,
        string BadgeName,
        int RecognitionCount,
        DateTime? LatestRecognitionAt
    );

    // Filter for category statistics
    public record CategoryStatsFilter(
        DateTime? From,
        DateTime? To,
        string? DepartmentId,
        string? UserId
    );

    // Sentiment analysis DTOs
    public record SentimentStatsDto(
        int PositiveCount,
        int NegativeCount,
        int NeutralCount,
        int TotalCount,
        double PositivePercentage,
        double NegativePercentage,
        double NeutralPercentage
    );

    public record FeedbackWithSentimentDto(
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
        string Sentiment
    );
}
