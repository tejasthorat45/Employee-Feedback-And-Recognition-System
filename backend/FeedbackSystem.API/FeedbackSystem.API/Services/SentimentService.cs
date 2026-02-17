using FeedbackSystem.API.Data;
using FeedbackSystem.API.DTOs;
using Microsoft.EntityFrameworkCore;

namespace FeedbackSystem.API.Services
{
    public class SentimentService : ISentimentService
    {
        private readonly AppDbContext _db;

        // Keyword-based sentiment analysis
        private readonly string[] _positiveKeywords = {
            "excellent", "great", "good", "outstanding", "fantastic", "amazing", "wonderful",
            "impressive", "superb", "brilliant", "exceptional", "perfect", "awesome",
            "helpful", "professional", "reliable", "efficient", "dedicated", "skilled",
            "innovative", "creative", "proactive", "collaborative", "supportive",
            "strong", "effective", "successful", "quality", "exceeded", "well done"
        };

        private readonly string[] _negativeKeywords = {
            "poor", "bad", "terrible", "awful", "horrible", "disappointing", "unacceptable",
            "inadequate", "unsatisfactory", "lacking", "insufficient", "fail", "failed",
            "weak", "slow", "careless", "unprofessional", "unreliable", "inconsistent",
            "late", "missed", "ignore", "neglect", "difficult", "below expectations",
            "not acceptable", "needs improvement", "concern", "problem"
        };

        public SentimentService(AppDbContext db)
        {
            _db = db;
        }

        public string AnalyzeSentiment(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "Neutral";

            var lowerText = text.ToLowerInvariant();
            int positiveCount = _positiveKeywords.Count(keyword => lowerText.Contains(keyword));
            int negativeCount = _negativeKeywords.Count(keyword => lowerText.Contains(keyword));

            if (positiveCount > negativeCount)
                return "Positive";
            else if (negativeCount > positiveCount)
                return "Negative";
            else
                return "Neutral";
        }

        public async Task<SentimentStatsDto> GetSentimentStatsAsync(
            DateTime? from = null, DateTime? to = null, string? departmentId = null, 
            CancellationToken ct = default)
        {
            var query = _db.Feedbacks.AsQueryable();

            if (from.HasValue)
                query = query.Where(f => f.CreatedAt >= from.Value);
            if (to.HasValue)
                query = query.Where(f => f.CreatedAt <= to.Value);
            if (!string.IsNullOrEmpty(departmentId))
                query = query.Where(f => f.ToUser.DepartmentId == departmentId);

            var feedbacks = await query
                .Select(f => f.Comments)
                .ToListAsync(ct);

            int positive = 0, negative = 0, neutral = 0;

            foreach (var comment in feedbacks)
            {
                var sentiment = AnalyzeSentiment(comment);
                if (sentiment == "Positive") positive++;
                else if (sentiment == "Negative") negative++;
                else neutral++;
            }

            int total = feedbacks.Count;
            return new SentimentStatsDto(
                PositiveCount: positive,
                NegativeCount: negative,
                NeutralCount: neutral,
                TotalCount: total,
                PositivePercentage: total > 0 ? (double)positive / total * 100 : 0,
                NegativePercentage: total > 0 ? (double)negative / total * 100 : 0,
                NeutralPercentage: total > 0 ? (double)neutral / total * 100 : 0
            );
        }

        public async Task<PagedResult<FeedbackWithSentimentDto>> GetFeedbackWithSentimentAsync(
            DateTime? from, DateTime? to, string? categoryId, string? departmentId,
            string? sentimentFilter, int page, int pageSize, CancellationToken ct = default)
        {
            var query = _db.Feedbacks
                .Include(f => f.FromUser)
                .Include(f => f.ToUser)
                .Include(f => f.Category)
                .AsQueryable();

            if (from.HasValue)
                query = query.Where(f => f.CreatedAt >= from.Value);
            if (to.HasValue)
                query = query.Where(f => f.CreatedAt <= to.Value);
            if (!string.IsNullOrEmpty(categoryId))
                query = query.Where(f => f.CategoryId == categoryId);
            if (!string.IsNullOrEmpty(departmentId))
                query = query.Where(f => f.ToUser.DepartmentId == departmentId);

            var totalCount = await query.CountAsync(ct);

            var feedbacks = await query
                .OrderByDescending(f => f.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            var items = feedbacks.Select(f =>
            {
                var sentiment = AnalyzeSentiment(f.Comments);
                return new FeedbackWithSentimentDto(
                    FeedbackId: f.FeedbackId,
                    FromUserId: f.FromUserId,
                    FromUserName: f.FromUser.FullName,
                    ToUserId: f.ToUserId,
                    ToUserName: f.ToUser.FullName,
                    CategoryId: f.CategoryId,
                    CategoryName: f.Category.CategoryName,
                    Comments: f.Comments,
                    IsAnonymous: f.IsAnonymous,
                    CreatedAt: f.CreatedAt,
                    Sentiment: sentiment
                );
            }).ToList();

            // Apply sentiment filter if specified
            if (!string.IsNullOrEmpty(sentimentFilter))
            {
                items = items.Where(f => 
                    f.Sentiment.Equals(sentimentFilter, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            return new PagedResult<FeedbackWithSentimentDto>(
                Page: page,
                PageSize: pageSize,
                TotalCount: totalCount,
                Items: items
            );
        }
    }
}
