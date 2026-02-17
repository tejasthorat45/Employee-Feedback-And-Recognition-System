namespace FeedbackSystem.API.Entities
{

    public class FeedbackReview
    {
        public int ReviewId { get; set; }
        public int FeedbackId { get; set; }
        public string ReviewedBy { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string? Remarks { get; set; }
        public DateTime ReviewedAt { get; set; }

        public Feedback Feedback { get; set; } = null!;
        public User Reviewer { get; set; } = null!;
    }
}
