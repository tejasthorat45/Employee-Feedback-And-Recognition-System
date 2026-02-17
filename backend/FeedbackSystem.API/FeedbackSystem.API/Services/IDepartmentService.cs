using FeedbackSystem.API.DTOs;

namespace FeedbackSystem.API.Services;

public interface IDepartmentService
{
    Task<List<DepartmentReadDto>> GetAllAsync(CancellationToken ct = default);
    Task<DepartmentReadDto?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<DepartmentReadDto> CreateAsync(DepartmentCreateDto dto, CancellationToken ct = default);
    Task<bool> UpdateAsync(string id, DepartmentUpdateDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(string id, CancellationToken ct = default);
}
