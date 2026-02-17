using FeedbackSystem.API.Data;
using FeedbackSystem.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace FeedbackSystem.API.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;
    public UserRepository(AppDbContext db) => _db = db;

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        _db.Users.Include(u => u.Role).Include(u => u.Department).FirstOrDefaultAsync(u => u.Email == email, ct);

    public Task<User?> GetByIdAsync(string id, CancellationToken ct = default) =>
        _db.Users.Include(u => u.Role).Include(u => u.Department).FirstOrDefaultAsync(u => u.UserId == id, ct);

    public Task<List<User>> GetAllAsync(CancellationToken ct = default) =>
        _db.Users.Include(u => u.Role).Include(u => u.Department).OrderBy(u => u.FullName).ToListAsync(ct);

    public Task<bool> EmailExistsAsync(string email, CancellationToken ct = default) =>
        _db.Users.AnyAsync(u => u.Email == email, ct);

    public Task<Role?> GetRoleByNameAsync(string roleName, CancellationToken ct = default) =>
        _db.Roles.FirstOrDefaultAsync(r => r.RoleName == roleName, ct);

    public Task<Department?> GetDepartmentByIdAsync(string departmentId, CancellationToken ct = default) =>
        _db.Departments.FirstOrDefaultAsync(d => d.DepartmentId == departmentId && d.IsActive, ct);

    public async Task<User> AddAsync(User user, CancellationToken ct = default)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);
        return user;
    }

    public async Task UpdateAsync(User user, CancellationToken ct = default)
    {
        _db.Users.Update(user);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(User user, CancellationToken ct = default)
    {
        _db.Users.Remove(user);
        await _db.SaveChangesAsync(ct);
    }

    // ✅ Count methods
    public Task<int> GetTotalCountAsync(CancellationToken ct = default) =>
        _db.Users.CountAsync(ct);

    public Task<int> GetActiveCountAsync(CancellationToken ct = default) =>
        _db.Users.CountAsync(u => u.IsActive, ct);

    // ✅ Helpers for InsightsService
    public Task<bool> UserExistsAsync(string userId, CancellationToken ct = default) =>
        _db.Users.AsNoTracking().AnyAsync(u => u.UserId == userId, ct);

    public async Task<string> GetDepartmentIdAsync(string userId, CancellationToken ct = default)
    {
        var depId = await _db.Users.AsNoTracking()
            .Where(u => u.UserId == userId)
            .Select(u => u.DepartmentId)
            .FirstOrDefaultAsync(ct);

        if (string.IsNullOrEmpty(depId)) throw new InvalidOperationException("Requester has no department assigned.");
        return depId;
    }

    // ✅ Search
    public Task<List<User>> SearchAsync(string query, CancellationToken ct = default)
    {
        var lowerQuery = query.ToLower();
        return _db.Users
            .Include(u => u.Role)
            .Include(u => u.Department)
            .Where(u => u.IsActive && 
                       (u.FullName.ToLower().Contains(lowerQuery) || 
                        u.Email.ToLower().Contains(lowerQuery) ||
                        u.UserId.ToLower().Contains(lowerQuery)))
            .OrderBy(u => u.FullName)
            .Take(20)
            .ToListAsync(ct);
    }
}