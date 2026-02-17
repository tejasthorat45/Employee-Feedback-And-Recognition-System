using FeedbackSystem.API.Entities;

namespace FeedbackSystem.API.Repositories;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<List<User>> GetAllAsync(CancellationToken ct = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
    Task<Role?> GetRoleByNameAsync(string roleName, CancellationToken ct = default);
    Task<Department?> GetDepartmentByIdAsync(string departmentId, CancellationToken ct = default);
    Task<User> AddAsync(User user, CancellationToken ct = default);
    Task UpdateAsync(User user, CancellationToken ct = default);
    Task DeleteAsync(User user, CancellationToken ct = default);
    
    // ✅ Count methods
    Task<int> GetTotalCountAsync(CancellationToken ct = default);
    Task<int> GetActiveCountAsync(CancellationToken ct = default);
    
    // ✅ Helpers for InsightsService
    Task<bool> UserExistsAsync(string userId, CancellationToken ct = default);
    Task<string> GetDepartmentIdAsync(string userId, CancellationToken ct = default);
    
    // ✅ Search
    Task<List<User>> SearchAsync(string query, CancellationToken ct = default);
}