using System.ComponentModel.DataAnnotations;

namespace FeedbackSystem.API.DTOs;

public record DepartmentReadDto(
    string DepartmentId,
    string DepartmentName,
    string? Description,
    bool IsActive,
    DateTime CreatedAt
);

public record DepartmentCreateDto(
    [Required, StringLength(20)] string DepartmentId,
    [Required, StringLength(100)] string DepartmentName,
    [StringLength(250)] string? Description
);

public record DepartmentUpdateDto(
    [Required, StringLength(100)] string DepartmentName,
    [StringLength(250)] string? Description,
    bool IsActive
);
