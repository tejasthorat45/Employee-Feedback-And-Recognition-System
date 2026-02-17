namespace FeedbackSystem.API.DTOs;

/// <summary>
/// Root settings object returned by GET /api/settings
/// </summary>
public class AppSettingsDto
{
    public FeedbackSettingsDto FeedbackSettings { get; set; } = new();
    public RecognitionSettingsDto RecognitionSettings { get; set; } = new();
    public NotificationSettingsDto NotificationSettings { get; set; } = new();
    public UserSettingsDto UserSettings { get; set; } = new();
}

public class FeedbackSettingsDto
{
    public bool AllowAnonymousFeedback { get; set; }
    public int MinimumFeedbackLength { get; set; }
    public bool RequireCategorySelection { get; set; }
}

public class RecognitionSettingsDto
{
    public int MaxPointsPerRecognition { get; set; }
    public int MonthlyPointsBudgetPerEmployee { get; set; }
}

public class NotificationSettingsDto
{
    public bool EnableEmailNotifications { get; set; }
    public bool NotifyOnFeedbackReceived { get; set; }
    public bool NotifyOnRecognitionReceived { get; set; }
}

public class UserSettingsDto
{
    public bool AllowPublicEmployeeRegistration { get; set; }
    public int SessionTimeout { get; set; }
}
