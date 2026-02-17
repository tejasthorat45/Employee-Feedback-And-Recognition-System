import { Injectable, inject } from '@angular/core';
import { Observable, catchError, throwError } from 'rxjs';
import { ApiService } from '../../core/services/api.service';
import {
  ManagerDashboard,
  ManagerFeedbackItem,
  RecentActivity,
  CategoryStats,
  PagedResult,
  FeedbackFilter,
  RecognitionItem,
  RecognitionFilter,
  NotificationItem,
  NotificationCount
} from '../models/manager.models';

@Injectable({
  providedIn: 'root'
})
export class ManagerApiService {
  private api = inject(ApiService);

  // ============ DASHBOARD ============
  
  /**
   * Get dashboard summary stats for the logged-in manager
   */
  getDashboard(): Observable<ManagerDashboard> {
    return this.api.get<ManagerDashboard>('/api/manager/dashboard').pipe(
      catchError(this.handleError)
    );
  }

  // ============ FEEDBACK ============

  /**
   * Get paginated feedback list with filters
   */
  getFeedbackList(filter: FeedbackFilter = {}): Observable<PagedResult<ManagerFeedbackItem>> {
    const params: Record<string, any> = {};
    
    if (filter.status && filter.status !== 'All') params['status'] = filter.status;
    if (filter.categoryId) params['categoryId'] = filter.categoryId;
    if (filter.search) params['search'] = filter.search;
    if (filter.page) params['page'] = filter.page;
    if (filter.pageSize) params['pageSize'] = filter.pageSize;

    return this.api.get<PagedResult<ManagerFeedbackItem>>('/api/manager/feedback', params).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Update feedback status (Pending -> Acknowledged -> Resolved)
   */
  updateFeedbackStatus(feedbackId: number, status: string, remarks?: string): Observable<void> {
    const params: Record<string, any> = {};
    if (remarks) params['remarks'] = remarks;

    return this.api.put<void>(`/api/manager/feedback/${feedbackId}/status`, { status }).pipe(
      catchError(this.handleError)
    );
  }

  // ============ RECENT ACTIVITY ============

  /**
   * Get recent activity for dashboard
   */
  getRecentActivity(count: number = 10): Observable<RecentActivity[]> {
    return this.api.get<RecentActivity[]>('/api/manager/activity', { count }).pipe(
      catchError(this.handleError)
    );
  }

  // ============ CATEGORY STATS ============

  /**
   * Get category distribution for chart
   */
  getCategoryDistribution(status?: string): Observable<CategoryStats[]> {
    const params: Record<string, any> = {};
    if (status && status !== 'All') params['status'] = status;

    return this.api.get<CategoryStats[]>('/api/manager/categories/distribution', params).pipe(
      catchError(this.handleError)
    );
  }

  // ============ RECOGNITIONS (via insight API) ============

  /**
   * Get all recognitions for manager's department
   */
  getRecognitions(filter: RecognitionFilter = {}): Observable<PagedResult<RecognitionItem>> {
    const params: Record<string, any> = {
      page: filter.page || 1,
      pageSize: filter.pageSize || 50
    };
    if (filter.search) params['search'] = filter.search;

    return this.api.get<PagedResult<RecognitionItem>>('/api/insight/recognitions', params).pipe(
      catchError(this.handleError)
    );
  }

  // ============ NOTIFICATIONS ============

  /**
   * Get all notifications for the logged-in user
   */
  getNotifications(): Observable<NotificationItem[]> {
    return this.api.get<NotificationItem[]>('/api/notifications').pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Get unread notification count
   */
  getNotificationCount(): Observable<NotificationCount> {
    return this.api.get<NotificationCount>('/api/notifications/count').pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Mark a specific notification as read
   */
  markAsRead(notificationId: number): Observable<void> {
    return this.api.put<void>(`/api/notifications/${notificationId}/read`, {}).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Mark all notifications as read
   */
  markAllAsRead(): Observable<void> {
    return this.api.put<void>('/api/notifications/read-all', {}).pipe(
      catchError(this.handleError)
    );
  }

  // ============ ERROR HANDLING ============

  private handleError(error: any) {
    console.error('Manager API Error:', error);
    let message = 'An error occurred. Please try again.';
    
    if (error.error?.message) {
      message = error.error.message;
    } else if (error.status === 401) {
      message = 'Session expired. Please login again.';
    } else if (error.status === 403) {
      message = 'You do not have permission to perform this action.';
    } else if (error.status === 404) {
      message = 'Resource not found.';
    }
    
    return throwError(() => new Error(message));
  }
}
