namespace FeedbackSystem.API.DTOs
{
    // Dashboard summary statistics
    public record DashboardSummaryDto(
        int TotalUsers,
        int ActiveUsers,
        int TotalFeedback,
        int TotalRecognition
    );

    // Monthly trend data
    public record MonthlyTrendDto(
        string[] Labels,
        int[] FeedbackCounts,
        int[] RecognitionCounts
    );

    // Department feedback/recognition counts
    public record DepartmentCountDto(
        string DepartmentId,
        string DepartmentName,
        int Count
    );

    // Top employees by points
    public record TopEmployeeDto(
        string UserId,
        string FullName,
        int Points
    );

    // Recognition given/received by department
    public record DepartmentRecognitionDto(
        string DepartmentId,
        string DepartmentName,
        int GivenCount,
        int ReceivedCount
    );

    // Category average scores
    public record CategoryAverageScoreDto(
        string CategoryId,
        string CategoryName,
        double AverageScore
    );
}
