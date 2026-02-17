using System.ComponentModel.DataAnnotations;

namespace FeedbackSystem.API.DTOs
{
    // DTO for creating a new badge
    public record CreateBadgeDto(
        [Required][StringLength(20)] string BadgeId,
        [Required][StringLength(100)] string BadgeName,
        [StringLength(225)] string? Description,
        [StringLength(50)] string? IconClass
    );

    // DTO for updating an existing badge
    public record UpdateBadgeDto(
        [Required][StringLength(100)] string BadgeName,
        [StringLength(225)] string? Description,
        [StringLength(50)] string? IconClass,
        bool IsActive
    );

    // DTO for returning badge data
    public record BadgeDto(
        string BadgeId,
        string BadgeName,
        string? Description,
        string? IconClass,
        bool IsActive,
        DateTime CreatedAt
    );

    // Paged result for badges
    public record PagedBadgeResult(
        int Page,
        int PageSize,
        int TotalCount,
        IReadOnlyList<BadgeDto> Items
    );
}
