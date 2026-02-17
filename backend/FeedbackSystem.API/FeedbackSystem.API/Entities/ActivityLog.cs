namespace FeedbackSystem.API.Entities
{
    public class ActivityLog
    {
        public int ActivityId { get; set; }
        public string UserId { get; set; } = null!;
        public string ActionType { get; set; } = null!;
        public string? EntityType { get; set; }
        public int? EntityId { get; set; }
        public DateTime CreatedAt { get; set; }

        public User User { get; set; } = null!;
    }
}
