using FeedbackSystem.API.Data;
using FeedbackSystem.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace FeedbackSystem.API.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _db;
    public CategoryRepository(AppDbContext db) => _db = db;

    public Task<List<Category>> GetAllAsync(CancellationToken ct = default) =>
        _db.Categories.AsNoTracking().OrderBy(x => x.CategoryName).ToListAsync(ct);

    public Task<Category?> GetByIdAsync(string id, CancellationToken ct = default) =>
        _db.Categories.AsNoTracking().FirstOrDefaultAsync(x => x.CategoryId == id, ct);

    public async Task<Category> AddAsync(Category entity, CancellationToken ct = default)
    {
        _db.Categories.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task UpdateAsync(Category entity, CancellationToken ct = default)
    {
        _db.Categories.Update(entity);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Category entity, CancellationToken ct = default)
    {
        _db.Categories.Remove(entity);
        await _db.SaveChangesAsync(ct);
    }

    public Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default) =>
        _db.Categories.AnyAsync(c => c.CategoryName == name, ct);
}