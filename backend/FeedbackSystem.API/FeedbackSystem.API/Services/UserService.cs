using FeedbackSystem.API.DTOs;
using FeedbackSystem.API.Entities;
using FeedbackSystem.API.Repositories;
using FeedbackSystem.API.Security;

namespace FeedbackSystem.API.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repo;
    private readonly IFeedbackRepository _feedbackRepo;
    private readonly IRecognitionRepository _recognitionRepo;

    public UserService(IUserRepository repo, IFeedbackRepository feedbackRepo, IRecognitionRepository recognitionRepo)
    {
        _repo = repo;
        _feedbackRepo = feedbackRepo;
        _recognitionRepo = recognitionRepo;
    }

    public async Task<List<UserReadDto>> GetAllAsync(CancellationToken ct = default)
    {
        var users = await _repo.GetAllAsync(ct);
        return users.Select(u => new UserReadDto(
            u.UserId,
            u.FullName,
            u.Email,
            u.Phone,
            u.Role.RoleName,
            u.DepartmentId,
            u.Department.DepartmentName,
            u.IsActive,
            u.CreatedAt
        )).ToList();
    }

    public async Task<UserReadDto?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var user = await _repo.GetByIdAsync(id, ct);
        if (user is null) return null;
        
        return new UserReadDto(
            user.UserId,
            user.FullName,
            user.Email,
            user.Phone,
            user.Role.RoleName,
            user.DepartmentId,
            user.Department.DepartmentName,
            user.IsActive,
            user.CreatedAt
        );
    }

    public async Task<UserReadDto> CreateAsync(UserCreateDto dto, CancellationToken ct = default)
    {
        if (await _repo.EmailExistsAsync(dto.Email, ct))
            throw new InvalidOperationException("Email already exists.");

        var role = await _repo.GetRoleByNameAsync(dto.RoleName, ct)
                   ?? throw new InvalidOperationException("Role not found.");

        var department = await _repo.GetDepartmentByIdAsync(dto.DepartmentId, ct)
                        ?? throw new InvalidOperationException("Department not found or inactive.");

        var entity = new User
        {
            UserId = dto.UserId,
            FullName = dto.FullName,
            Email = dto.Email,
            PasswordHash = PasswordHasher.Hash(dto.Password),
            RoleId = role.RoleId,
            DepartmentId = dto.DepartmentId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _repo.AddAsync(entity, ct);
        entity = await _repo.GetByIdAsync(entity.UserId, ct) ?? entity;
        
        return new UserReadDto(
            entity.UserId,
            entity.FullName,
            entity.Email,
            entity.Phone,
            entity.Role.RoleName,
            entity.DepartmentId,
            entity.Department.DepartmentName,
            entity.IsActive,
            entity.CreatedAt
        );
    }

    public async Task<bool> UpdateAsync(string id, UserUpdateDto dto, CancellationToken ct = default)
    {
        var user = await _repo.GetByIdAsync(id, ct);
        if (user is null) return false;

        var role = await _repo.GetRoleByNameAsync(dto.RoleName, ct)
                   ?? throw new InvalidOperationException("Role not found.");

        var department = await _repo.GetDepartmentByIdAsync(dto.DepartmentId, ct)
                        ?? throw new InvalidOperationException("Department not found or inactive.");

        user.FullName = dto.FullName;
        user.RoleId = role.RoleId;
        user.DepartmentId = department.DepartmentId;
        user.IsActive = dto.IsActive;

        await _repo.UpdateAsync(user, ct);
        return true;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
    {
        var user = await _repo.GetByIdAsync(id, ct);
        if (user is null) return false;


        await _repo.DeleteAsync(user, ct);
        return true;
    }

    // ✅ Statistics
    public async Task<UserStatsDto> GetStatsAsync(CancellationToken ct = default)
    {
        var totalUsers = await _repo.GetTotalCountAsync(ct);
        var activeUsers = await _repo.GetActiveCountAsync(ct);
        var inactiveUsers = totalUsers - activeUsers;
        var totalFeedbacks = await _feedbackRepo.GetTotalCountAsync(ct);
        var totalRecognitions = await _recognitionRepo.GetTotalCountAsync(ct);

        return new UserStatsDto(totalUsers, activeUsers, inactiveUsers, totalFeedbacks, totalRecognitions);
    }

<<<<<<< HEAD
    // ✅ Search
    public async Task<List<UserReadDto>> SearchAsync(string query, CancellationToken ct = default)
    {
        var users = await _repo.SearchAsync(query, ct);
        return users.Select(u => new UserReadDto(
            u.UserId,
            u.FullName,
            u.Email,
            u.Role.RoleName,
            u.DepartmentId,
            u.Department.DepartmentName,
            u.IsActive,
            u.CreatedAt
        )).ToList();
=======
    // ✅ Profile - Get current user's profile
    public async Task<ProfileReadDto?> GetProfileAsync(string userId, CancellationToken ct = default)
    {
        var user = await _repo.GetByIdAsync(userId, ct);
        if (user is null) return null;

        return new ProfileReadDto(
            user.UserId,
            user.FullName,
            user.Email,
            user.Phone,
            user.Role.RoleName,
            user.DepartmentId,
            user.Department.DepartmentName,
            user.CreatedAt
        );
    }

    // ✅ Profile - Update current user's profile
    public async Task<bool> UpdateProfileAsync(string userId, ProfileUpdateDto dto, CancellationToken ct = default)
    {
        var user = await _repo.GetByIdAsync(userId, ct);
        if (user is null) return false;

        // Check if email is being changed and if it already exists
        if (user.Email != dto.Email && await _repo.EmailExistsAsync(dto.Email, ct))
            throw new InvalidOperationException("Email already exists.");

        user.FullName = dto.FullName;
        user.Email = dto.Email;
        user.Phone = dto.Phone;

        await _repo.UpdateAsync(user, ct);
        return true;
>>>>>>> 3fe9f7803dbbd63808fef403e6923b2697672c72
    }
}