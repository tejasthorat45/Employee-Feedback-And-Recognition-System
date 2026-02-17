import { Injectable, inject } from '@angular/core';
import { Observable, map, of, catchError, BehaviorSubject, tap } from 'rxjs';
import { ManagerApiService } from './manager-api.service';
import {
  ManagerDashboard,
  ManagerFeedbackItem,
  RecentActivity,
  CategoryStats,
  PagedResult,
  FeedbackFilter,
  RecognitionItem
} from '../models/manager.models';

@Injectable({
  providedIn: 'root'
})
export class ManagerService {
  private api = inject(ManagerApiService);

  // Cache subjects for reactive updates
  private dashboardCache$ = new BehaviorSubject<ManagerDashboard | null>(null);
  private feedbackCache$ = new BehaviorSubject<ManagerFeedbackItem[]>([]);
  private recognitionCache$ = new BehaviorSubject<RecognitionItem[]>([]);

  // Loading states
  private _loading$ = new BehaviorSubject<boolean>(false);
  readonly loading$ = this._loading$.asObservable();

  // Error state
  private _error$ = new BehaviorSubject<string | null>(null);
  readonly error$ = this._error$.asObservable();

  // ============ DASHBOARD ============

  /**
   * Fetch dashboard data from API
   */
  loadDashboard(): Observable<ManagerDashboard> {
    this._loading$.next(true);
    this._error$.next(null);

    return this.api.getDashboard().pipe(
      tap(data => {
        this.dashboardCache$.next(data);
        this._loading$.next(false);
      }),
      catchError(err => {
        this._error$.next(err.message);
        this._loading$.next(false);
        // Return default values on error
        return of({
          totalFeedback: 0,
          pendingReviews: 0,
          acknowledged: 0,
          resolved: 0,
          engagementScore: 0,
          totalRecognitions: 0,
          totalRecognitionPoints: 0
        });
      })
    );
  }

  /**
   * Get cached dashboard or fetch new
   */
  getDashboard(): Observable<ManagerDashboard> {
    if (this.dashboardCache$.getValue()) {
      return this.dashboardCache$.asObservable() as Observable<ManagerDashboard>;
    }
    return this.loadDashboard();
  }

  // ============ FEEDBACK ============

  /**
   * Get all feedback with optional filters
   */
  getAllFeedback(filter: FeedbackFilter = {}): Observable<PagedResult<ManagerFeedbackItem>> {
    this._loading$.next(true);
    this._error$.next(null);

    return this.api.getFeedbackList(filter).pipe(
      tap(result => {
        this.feedbackCache$.next(result.items);
        this._loading$.next(false);
      }),
      catchError(err => {
        this._error$.next(err.message);
        this._loading$.next(false);
        return of({ page: 1, pageSize: 20, totalCount: 0, items: [] });
      })
    );
  }

  /**
   * Update feedback status
   */
  updateFeedbackStatus(feedbackId: number, newStatus: string, remarks?: string): Observable<boolean> {
    this._loading$.next(true);
    this._error$.next(null);

    return this.api.updateFeedbackStatus(feedbackId, newStatus, remarks).pipe(
      map(() => {
        // Update cache
        const currentList = this.feedbackCache$.getValue();
        const index = currentList.findIndex(f => f.feedbackId === feedbackId);
        if (index !== -1) {
          currentList[index] = { ...currentList[index], status: newStatus as any };
          this.feedbackCache$.next([...currentList]);
        }
        this._loading$.next(false);
        return true;
      }),
      catchError(err => {
        this._error$.next(err.message);
        this._loading$.next(false);
        return of(false);
      })
    );
  }

  // ============ RECOGNITIONS ============

  /**
   * Get all recognitions
   */
  getAllRecognitions(): Observable<RecognitionItem[]> {
    this._loading$.next(true);
    this._error$.next(null);

    return this.api.getRecognitions({ pageSize: 100 }).pipe(
      map(result => result.items),
      tap(items => {
        this.recognitionCache$.next(items);
        this._loading$.next(false);
      }),
      catchError(err => {
        this._error$.next(err.message);
        this._loading$.next(false);
        return of([]);
      })
    );
  }

  // ============ RECENT ACTIVITY ============

  /**
   * Get recent activity for dashboard
   */
  getRecentActivity(count: number = 10): Observable<RecentActivity[]> {
    return this.api.getRecentActivity(count).pipe(
      catchError(err => {
        console.error('Error loading activity:', err);
        return of([]);
      })
    );
  }

  // ============ CATEGORY DISTRIBUTION ============

  /**
   * Get category distribution with optional status filter
   */
  getCategoryDistribution(status?: string): Observable<CategoryStats[]> {
    return this.api.getCategoryDistribution(status).pipe(
      catchError(err => {
        console.error('Error loading category distribution:', err);
        return of([]);
      })
    );
  }

  // ============ UTILITY METHODS ============

  /**
   * Refresh all data
   */
  refreshAll(): void {
    this.dashboardCache$.next(null);
    this.feedbackCache$.next([]);
    this.recognitionCache$.next([]);
    this._error$.next(null);
  }

  /**
   * Clear error
   */
  clearError(): void {
    this._error$.next(null);
  }
}
