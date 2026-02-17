// src/app/core/models/settings.model.ts

export interface AppSettings {
  feedbackSettings: FeedbackSettings;
  recognitionSettings: RecognitionSettings;
  notificationSettings: NotificationSettings;
  userSettings: UserSettings;
}

export interface FeedbackSettings {
  allowAnonymousFeedback: boolean;
  minimumFeedbackLength: number;
  requireCategorySelection: boolean;
}

export interface RecognitionSettings {
  maxPointsPerRecognition: number;
  monthlyPointsBudgetPerEmployee: number;
}

export interface NotificationSettings {
  enableEmailNotifications: boolean;
  notifyOnFeedbackReceived: boolean;
  notifyOnRecognitionReceived: boolean;
}

export interface UserSettings {
  allowPublicEmployeeRegistration: boolean;
  sessionTimeout: number;
}
