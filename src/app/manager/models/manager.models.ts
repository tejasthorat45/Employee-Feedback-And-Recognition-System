// Manager API Models - TypeScript interfaces matching backend DTOs

export interface ManagerDashboard {
  totalFeedback: number;
  pendingReviews: number;
  acknowledged: number;
  resolved: number;
  engagementScore: number;
  totalRecognitions: number;
  totalRecognitionPoints: number;
}

export interface ManagerFeedbackItem {
  feedbackId: number;
  fromUserId: string;
  fromUserName: string;
  toUserId: string;
  toUserName: string;
  categoryId: string;
  categoryName: string;
  comments: string;
  isAnonymous: boolean;
  createdAt: string;
  status: 'Pending' | 'Acknowledged' | 'Resolved';
  reviewedBy?: string;
  reviewedAt?: string;
  reviewRemarks?: string;
}

export interface RecentActivity {
  id: number;
  type: 'Feedback' | 'Recognition';
  title: string;
  userName: string;
  detail: string;
  createdAt: string;
  status: string;
}

export interface CategoryStats {
  categoryId: string;
  categoryName: string;
  feedbackCount: number;
  latestFeedbackAt?: string;
}

export interface RecognitionCategoryStats {
  categoryId: string;
  categoryName: string;
  recognitionCount: number;
  latestRecognitionAt?: string;
}

export interface PagedResult<T> {
  page: number;
  pageSize: number;
  totalCount: number;
  items: T[];
}

export interface RecognitionItem {
  recognitionId: number;
  fromUserId: string;
  fromUserName: string;
  toUserId: string;
  toUserName: string;
  categoryId: string;
  categoryName: string;
  points: number;
  message: string;
  createdAt: string;
}

// Filter types
export interface FeedbackFilter {
  status?: string;
  categoryId?: string;
  search?: string;
  page?: number;
  pageSize?: number;
}

export interface RecognitionFilter {
  search?: string;
  page?: number;
  pageSize?: number;
}

// Notification types
export interface NotificationItem {
  notificationId: number;
  userId: string;
  title: string;
  message: string;
  isRead: boolean;
  createdAt: string;
}

export interface NotificationCount {
  total: number;
  unread: number;
}
