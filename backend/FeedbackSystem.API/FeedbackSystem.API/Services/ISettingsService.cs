using FeedbackSystem.API.DTOs;

namespace FeedbackSystem.API.Services;

public interface ISettingsService
{
    Task<AppSettingsDto> GetSettingsAsync(CancellationToken ct = default);
    Task SaveSettingsAsync(AppSettingsDto settings, CancellationToken ct = default);
}
