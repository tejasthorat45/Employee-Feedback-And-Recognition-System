namespace FeedbackSystem.API.Entities
{
    public class User
    {
        public string UserId { get; set; } = null!;

        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string? Phone { get; set; }

        public int RoleId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        // ✅ New: FK to Department
        public string DepartmentId { get; set; } = null!;

        // Navigations
        public Role Role { get; set; } = null!;
        public Department Department { get; set; } = null!;

        public ICollection<Feedback> FeedbacksFrom { get; set; } = new List<Feedback>();
        public ICollection<Feedback> FeedbacksTo { get; set; } = new List<Feedback>();
        public ICollection<FeedbackReview> ReviewsDone { get; set; } = new List<FeedbackReview>();
        public ICollection<Recognition> RecognitionsFrom { get; set; } = new List<Recognition>();
        public ICollection<Recognition> RecognitionsTo { get; set; } = new List<Recognition>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();
    }
}