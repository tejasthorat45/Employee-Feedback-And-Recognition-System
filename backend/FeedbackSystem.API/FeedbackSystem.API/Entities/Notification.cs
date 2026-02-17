namespace FeedbackSystem.API.Entities
{
    public class Notification
    {
        public int NotificationId { get; set; }
        public string UserId { get; set; } = null!;
        public string? Title { get; set; }
        public string? Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }

        public User User { get; set; } = null!;
    }

}
