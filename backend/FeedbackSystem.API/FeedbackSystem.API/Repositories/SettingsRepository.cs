using FeedbackSystem.API.Data;
using FeedbackSystem.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace FeedbackSystem.API.Repositories;

public class SettingsRepository : ISettingsRepository
{
    private readonly AppDbContext _db;

    public SettingsRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Dictionary<string, string>> GetAllSettingsAsync(CancellationToken ct = default)
    {
        var settings = await _db.AppSettings.ToListAsync(ct);
        return settings.ToDictionary(s => s.SettingKey, s => s.SettingValue);
    }

    public async Task UpsertSettingAsync(string key, string value, CancellationToken ct = default)
    {
        var existing = await _db.AppSettings.FindAsync(new object[] { key }, ct);
        
        if (existing != null)
        {
            existing.SettingValue = value;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            _db.AppSettings.Add(new AppSetting
            {
                SettingKey = key,
                SettingValue = value,
                UpdatedAt = DateTime.UtcNow
            });
        }
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _db.SaveChangesAsync(ct);
    }
}
