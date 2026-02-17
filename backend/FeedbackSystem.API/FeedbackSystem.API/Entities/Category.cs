namespace FeedbackSystem.API.Entities
{
    public class Category
    {
        public string CategoryId { get; set; } = null!;
        public string CategoryName { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
    }
}
