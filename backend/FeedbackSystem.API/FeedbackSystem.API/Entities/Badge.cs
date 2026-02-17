namespace FeedbackSystem.API.Entities
{
    public class Badge
    {
        public string BadgeId { get; set; } = null!;
        public string BadgeName { get; set; } = null!;
        public string? Description { get; set; }
        public string? IconClass { get; set; } // For storing icon/emoji class
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<Recognition> Recognitions { get; set; } = new List<Recognition>();
    }
}
