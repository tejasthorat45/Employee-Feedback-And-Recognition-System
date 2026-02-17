using System.ComponentModel.DataAnnotations;

namespace FeedbackSystem.API.Entities
{
    public class Department
    {
        public string DepartmentId { get; set; } = null!;

        [Required, MaxLength(100)]
        public string DepartmentName { get; set; } = null!;

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation: One Department has many Users
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}