using FeedbackSystem.API.Data;
using FeedbackSystem.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace FeedbackSystem.API.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly AppDbContext _db;
    public DepartmentRepository(AppDbContext db) => _db = db;

    public Task<Department?> GetByIdAsync(string id, CancellationToken ct = default) =>
        _db.Departments.Include(d => d.Users).FirstOrDefaultAsync(d => d.DepartmentId == id, ct);

    public Task<List<Department>> GetAllAsync(CancellationToken ct = default) =>
        _db.Departments.OrderBy(d => d.DepartmentName).ToListAsync(ct);

    public Task<bool> DepartmentNameExistsAsync(string name, string? excludeId = null, CancellationToken ct = default)
    {
        var query = _db.Departments.Where(d => d.DepartmentName == name);
        if (excludeId != null)
            query = query.Where(d => d.DepartmentId != excludeId);
        return query.AnyAsync(ct);
    }

    public async Task<Department> AddAsync(Department department, CancellationToken ct = default)
    {
        _db.Departments.Add(department);
        await _db.SaveChangesAsync(ct);
        return department;
    }

    public async Task UpdateAsync(Department department, CancellationToken ct = default)
    {
        _db.Departments.Update(department);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Department department, CancellationToken ct = default)
    {
        _db.Departments.Remove(department);
        await _db.SaveChangesAsync(ct);
    }
}
