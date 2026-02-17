using FeedbackSystem.API.DTOs;
using FeedbackSystem.API.Entities;
using FeedbackSystem.API.Repositories;

namespace FeedbackSystem.API.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _repo;

    public DepartmentService(IDepartmentRepository repo) => _repo = repo;

    public async Task<List<DepartmentReadDto>> GetAllAsync(CancellationToken ct = default)
    {
        var departments = await _repo.GetAllAsync(ct);
        return departments.Select(d => new DepartmentReadDto(
            d.DepartmentId,
            d.DepartmentName,
            d.Description,
            d.IsActive,
            d.CreatedAt
        )).ToList();
    }

    public async Task<DepartmentReadDto?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var dept = await _repo.GetByIdAsync(id, ct);
        if (dept is null) return null;
        
        return new DepartmentReadDto(
            dept.DepartmentId,
            dept.DepartmentName,
            dept.Description,
            dept.IsActive,
            dept.CreatedAt
        );
    }

    public async Task<DepartmentReadDto> CreateAsync(DepartmentCreateDto dto, CancellationToken ct = default)
    {
        if (await _repo.DepartmentNameExistsAsync(dto.DepartmentName, null, ct))
            throw new InvalidOperationException("Department name already exists.");

        var entity = new Department
        {
            DepartmentId = dto.DepartmentId,
            DepartmentName = dto.DepartmentName,
            Description = dto.Description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _repo.AddAsync(entity, ct);
        
        return new DepartmentReadDto(
            entity.DepartmentId,
            entity.DepartmentName,
            entity.Description,
            entity.IsActive,
            entity.CreatedAt
        );
    }

    public async Task<bool> UpdateAsync(string id, DepartmentUpdateDto dto, CancellationToken ct = default)
    {
        var dept = await _repo.GetByIdAsync(id, ct);
        if (dept is null) return false;

        if (await _repo.DepartmentNameExistsAsync(dto.DepartmentName, id, ct))
            throw new InvalidOperationException("Department name already exists.");

        dept.DepartmentName = dto.DepartmentName;
        dept.Description = dto.Description;
        dept.IsActive = dto.IsActive;

        await _repo.UpdateAsync(dept, ct);
        return true;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
    {
        var dept = await _repo.GetByIdAsync(id, ct);
        if (dept is null) return false;

        // Check if department has users
        if (dept.Users != null && dept.Users.Any())
            throw new InvalidOperationException("Cannot delete department with existing users. Please reassign or remove users first.");

        await _repo.DeleteAsync(dept, ct);
        return true;
    }
}
