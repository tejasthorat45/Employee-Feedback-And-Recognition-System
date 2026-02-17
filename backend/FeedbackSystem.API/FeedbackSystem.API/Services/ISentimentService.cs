using FeedbackSystem.API.DTOs;

namespace FeedbackSystem.API.Services
{
    public interface ISentimentService
    {
        /// <summary>
        /// Get sentiment statistics for all feedback
        /// </summary>
        Task<SentimentStatsDto> GetSentimentStatsAsync(DateTime? from = null, DateTime? to = null, string? departmentId = null, CancellationToken ct = default);

        /// <summary>
        /// Get feedback with sentiment analysis
        /// </summary>
        Task<PagedResult<FeedbackWithSentimentDto>> GetFeedbackWithSentimentAsync(
            DateTime? from, DateTime? to, string? categoryId, string? departmentId, 
            string? sentimentFilter, int page, int pageSize, CancellationToken ct = default);

        /// <summary>
        /// Analyze sentiment of a single text
        /// </summary>
        string AnalyzeSentiment(string text);
    }
}
