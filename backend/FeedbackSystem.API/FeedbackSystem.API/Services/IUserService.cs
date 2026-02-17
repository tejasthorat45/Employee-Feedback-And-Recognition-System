using FeedbackSystem.API.DTOs;

namespace FeedbackSystem.API.Services;

public interface IUserService
{
    Task<List<UserReadDto>> GetAllAsync(CancellationToken ct = default);
    Task<UserReadDto?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<UserReadDto> CreateAsync(UserCreateDto dto, CancellationToken ct = default);
    Task<bool> UpdateAsync(string id, UserUpdateDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(string id, CancellationToken ct = default);
    
    // ✅ Statistics
    Task<UserStatsDto> GetStatsAsync(CancellationToken ct = default);
<<<<<<< HEAD
    
    // ✅ Search
    Task<List<UserReadDto>> SearchAsync(string query, CancellationToken ct = default);
=======

    // ✅ Profile
    Task<ProfileReadDto?> GetProfileAsync(string userId, CancellationToken ct = default);
    Task<bool> UpdateProfileAsync(string userId, ProfileUpdateDto dto, CancellationToken ct = default);
>>>>>>> 3fe9f7803dbbd63808fef403e6923b2697672c72
}
