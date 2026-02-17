using FeedbackSystem.API.DTOs;
using FeedbackSystem.API.Entities;
using FeedbackSystem.API.Repositories;
using FeedbackSystem.API.Security;

namespace FeedbackSystem.API.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IJwtTokenService _jwt;

    public AuthService(IUserRepository users, IJwtTokenService jwt)
    {
        _users = users;
        _jwt = jwt;
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto dto, CancellationToken ct = default)
    {
        var user = await _users.GetByEmailAsync(dto.Email, ct);
        if (user is null || !user.IsActive) return null;

        if (!PasswordHasher.Verify(dto.Password, user.PasswordHash))
            return null;

        var (token, expires) = _jwt.CreateToken(user);
        var authUser = new AuthUserDto(
            user.UserId,
            user.FullName,
            user.Email,
            user.Role.RoleName,
            user.DepartmentId,
            user.Department.DepartmentName,
            user.IsActive,
            user.CreatedAt
        );
        return new AuthResponseDto(token, expires, authUser);
    }

    public async Task<UserReadDto> RegisterAsync(RegisterUserDto dto, CancellationToken ct = default)
    {
        if (await _users.EmailExistsAsync(dto.Email, ct))
            throw new InvalidOperationException("Email already exists.");

        var role = await _users.GetRoleByNameAsync(dto.RoleName, ct)
                   ?? throw new InvalidOperationException("Role not found.");

        var department = await _users.GetDepartmentByIdAsync(dto.DepartmentId, ct)
                        ?? throw new InvalidOperationException("Department not found or inactive.");

        var user = new User
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

        await _users.AddAsync(user, ct);

        user = await _users.GetByIdAsync(user.UserId, ct) ?? user;
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

    // ✅ Public registration - Always forces role to "Employee"
    public async Task<UserReadDto> PublicRegisterAsync(PublicRegisterDto dto, CancellationToken ct = default)
    {
        if (await _users.EmailExistsAsync(dto.Email, ct))
            throw new InvalidOperationException("Email already exists.");

        // Force role to Employee - ignore any role sent by client
        var employeeRole = await _users.GetRoleByNameAsync("Employee", ct)
                          ?? throw new InvalidOperationException("Employee role not found in system.");

        var department = await _users.GetDepartmentByIdAsync(dto.DepartmentId, ct)
                        ?? throw new InvalidOperationException("Department not found or inactive.");

        var user = new User
        {
            UserId = dto.UserId,
            FullName = dto.FullName,
            Email = dto.Email,
            PasswordHash = PasswordHasher.Hash(dto.Password),
            RoleId = employeeRole.RoleId,  // Always Employee
            DepartmentId = dto.DepartmentId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _users.AddAsync(user, ct);

        user = await _users.GetByIdAsync(user.UserId, ct) ?? user;
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
}