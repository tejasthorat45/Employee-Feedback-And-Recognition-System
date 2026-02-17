using FeedbackSystem.API.DTOs;
using FeedbackSystem.API.Entities;
using FeedbackSystem.API.Repositories;

namespace FeedbackSystem.API.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repo;

    public CategoryService(ICategoryRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<CategoryReadDto>> GetAllAsync(CancellationToken ct = default)
    {
        var data = await _repo.GetAllAsync(ct);
        return data.Select(c => new CategoryReadDto(
            c.CategoryId,
            c.CategoryName,
            c.Description,
            c.IsActive,
            c.CreatedAt
        )).ToList();
    }

    public async Task<CategoryReadDto?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        if (entity is null) return null;
        
        return new CategoryReadDto(
            entity.CategoryId,
            entity.CategoryName,
            entity.Description,
            entity.IsActive,
            entity.CreatedAt
        );
    }

    public async Task<CategoryReadDto> CreateAsync(CategoryCreateDto dto, CancellationToken ct = default)
    {
        if (await _repo.ExistsByNameAsync(dto.CategoryName, ct))
            throw new InvalidOperationException("Category name already exists.");

        var entity = new Category
        {
            CategoryId = dto.CategoryId,
            CategoryName = dto.CategoryName,
            Description = dto.Description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        await _repo.AddAsync(entity, ct);
        
        return new CategoryReadDto(
            entity.CategoryId,
            entity.CategoryName,
            entity.Description,
            entity.IsActive,
            entity.CreatedAt
        );
    }

    public async Task<bool> UpdateAsync(string id, CategoryUpdateDto dto, CancellationToken ct = default)
    {
        var existing = await _repo.GetByIdAsync(id, ct);
        if (existing is null) return false;

        existing.CategoryName = dto.CategoryName;
        existing.Description = dto.Description;
        existing.IsActive = dto.IsActive;
        
        await _repo.UpdateAsync(existing, ct);
        return true;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
    {
        var existing = await _repo.GetByIdAsync(id, ct);
        if (existing is null) return false;

        await _repo.DeleteAsync(existing, ct);
        return true;
    }
}