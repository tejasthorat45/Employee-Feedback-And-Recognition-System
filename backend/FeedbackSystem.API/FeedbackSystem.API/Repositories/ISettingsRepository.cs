using FeedbackSystem.API.Entities;

namespace FeedbackSystem.API.Repositories;

public interface ISettingsRepository
{
    Task<Dictionary<string, string>> GetAllSettingsAsync(CancellationToken ct = default);
    Task UpsertSettingAsync(string key, string value, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
