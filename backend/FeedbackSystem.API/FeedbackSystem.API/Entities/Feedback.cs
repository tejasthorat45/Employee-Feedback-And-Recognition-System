namespace FeedbackSystem.API.Entities
{
    public class Feedback
    {
        public int FeedbackId { get; set; }
        public string FromUserId { get; set; } = null!;
        public string ToUserId { get; set; } = null!;
        public string CategoryId { get; set; } = null!;
        public string Comments { get; set; } = null!;
        public bool IsAnonymous { get; set; }
        public DateTime CreatedAt { get; set; }

        public User FromUser { get; set; } = null!;
        public User ToUser { get; set; } = null!;
        public Category Category { get; set; } = null!;
        public ICollection<FeedbackReview> Reviews { get; set; } = new List<FeedbackReview>();
    }
}
