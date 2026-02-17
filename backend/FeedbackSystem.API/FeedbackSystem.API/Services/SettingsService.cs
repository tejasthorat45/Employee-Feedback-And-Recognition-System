using FeedbackSystem.API.DTOs;
using FeedbackSystem.API.Repositories;

namespace FeedbackSystem.API.Services;

public class SettingsService : ISettingsService
{
    private readonly ISettingsRepository _repository;

    // Default values for all settings
    private static readonly Dictionary<string, string> Defaults = new()
    {
        ["feedbackSettings.allowAnonymousFeedback"] = "false",
        ["feedbackSettings.minimumFeedbackLength"] = "20",
        ["feedbackSettings.requireCategorySelection"] = "true",
        ["recognitionSettings.maxPointsPerRecognition"] = "10",
        ["recognitionSettings.monthlyPointsBudgetPerEmployee"] = "100",
        ["notificationSettings.enableEmailNotifications"] = "true",
        ["notificationSettings.notifyOnFeedbackReceived"] = "true",
        ["notificationSettings.notifyOnRecognitionReceived"] = "true",
        ["userSettings.allowPublicEmployeeRegistration"] = "false",
        ["userSettings.sessionTimeout"] = "30"
    };

    public SettingsService(ISettingsRepository repository)
    {
        _repository = repository;
    }

    public async Task<AppSettingsDto> GetSettingsAsync(CancellationToken ct = default)
    {
        var storedSettings = await _repository.GetAllSettingsAsync(ct);

        // Merge defaults with stored values
        var merged = new Dictionary<string, string>(Defaults);
        foreach (var kv in storedSettings)
        {
            if (merged.ContainsKey(kv.Key))
                merged[kv.Key] = kv.Value;
        }

        // Map to DTO
        return new AppSettingsDto
        {
            FeedbackSettings = new FeedbackSettingsDto
            {
                AllowAnonymousFeedback = ParseBool(merged["feedbackSettings.allowAnonymousFeedback"]),
                MinimumFeedbackLength = ParseInt(merged["feedbackSettings.minimumFeedbackLength"]),
                RequireCategorySelection = ParseBool(merged["feedbackSettings.requireCategorySelection"])
            },
            RecognitionSettings = new RecognitionSettingsDto
            {
                MaxPointsPerRecognition = ParseInt(merged["recognitionSettings.maxPointsPerRecognition"]),
                MonthlyPointsBudgetPerEmployee = ParseInt(merged["recognitionSettings.monthlyPointsBudgetPerEmployee"])
            },
            NotificationSettings = new NotificationSettingsDto
            {
                EnableEmailNotifications = ParseBool(merged["notificationSettings.enableEmailNotifications"]),
                NotifyOnFeedbackReceived = ParseBool(merged["notificationSettings.notifyOnFeedbackReceived"]),
                NotifyOnRecognitionReceived = ParseBool(merged["notificationSettings.notifyOnRecognitionReceived"])
            },
            UserSettings = new UserSettingsDto
            {
                AllowPublicEmployeeRegistration = ParseBool(merged["userSettings.allowPublicEmployeeRegistration"]),
                SessionTimeout = ParseInt(merged["userSettings.sessionTimeout"])
            }
        };
    }

    public async Task SaveSettingsAsync(AppSettingsDto settings, CancellationToken ct = default)
    {
        // Convert DTO to flat key-value pairs
        var flat = new Dictionary<string, string>
        {
            ["feedbackSettings.allowAnonymousFeedback"] = settings.FeedbackSettings.AllowAnonymousFeedback.ToString().ToLower(),
            ["feedbackSettings.minimumFeedbackLength"] = settings.FeedbackSettings.MinimumFeedbackLength.ToString(),
            ["feedbackSettings.requireCategorySelection"] = settings.FeedbackSettings.RequireCategorySelection.ToString().ToLower(),
            ["recognitionSettings.maxPointsPerRecognition"] = settings.RecognitionSettings.MaxPointsPerRecognition.ToString(),
            ["recognitionSettings.monthlyPointsBudgetPerEmployee"] = settings.RecognitionSettings.MonthlyPointsBudgetPerEmployee.ToString(),
            ["notificationSettings.enableEmailNotifications"] = settings.NotificationSettings.EnableEmailNotifications.ToString().ToLower(),
            ["notificationSettings.notifyOnFeedbackReceived"] = settings.NotificationSettings.NotifyOnFeedbackReceived.ToString().ToLower(),
            ["notificationSettings.notifyOnRecognitionReceived"] = settings.NotificationSettings.NotifyOnRecognitionReceived.ToString().ToLower(),
            ["userSettings.allowPublicEmployeeRegistration"] = settings.UserSettings.AllowPublicEmployeeRegistration.ToString().ToLower(),
            ["userSettings.sessionTimeout"] = settings.UserSettings.SessionTimeout.ToString()
        };

        // Save all settings
        foreach (var kv in flat)
        {
            await _repository.UpsertSettingAsync(kv.Key, kv.Value, ct);
        }

        await _repository.SaveChangesAsync(ct);
    }

    private static bool ParseBool(string value) => bool.TryParse(value, out var result) && result;
    private static int ParseInt(string value) => int.TryParse(value, out var result) ? result : 0;
}
