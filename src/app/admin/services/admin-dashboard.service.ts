
import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { delay, map } from 'rxjs/operators';
import { ApiService } from '../../core/services/api.service';

export interface DashboardSummary {
  totalUsers: number;
  activeUsers: number;
  totalFeedback: number;
  totalRecognition: number;
}

export interface SentimentStats {
  positiveCount: number;
  negativeCount: number;
  neutralCount: number;
  totalCount: number;
  positivePercentage: number;
  negativePercentage: number;
  neutralPercentage: number;
}

export interface CategoryCount { category: string; count: number; }
export interface TypeDistribution { type: string; count: number; }
export interface MonthlyTrend { month: string; count: number; }
export interface ActivityItem { time: string; user: string; action: string; details: string; }

@Injectable({ providedIn: 'root' })
export class AdminDashboardService {

  constructor(private api: ApiService) {}

  // Get sentiment statistics
  getSentimentStats(from?: Date, to?: Date, departmentId?: string): Observable<SentimentStats> {
    const params: any = {};
    if (from) params.from = from.toISOString();
    if (to) params.to = to.toISOString();
    if (departmentId) params.departmentId = departmentId;

    return this.api.get<SentimentStats>('/api/insight/sentiment/stats', params);
  }

  getSummary$(): Observable<DashboardSummary> {
    return this.api.get<DashboardSummary>('/api/dashboard/summary');
  }

  getFeedbackByCategory$(): Observable<CategoryCount[]> {
    return this.api.get<any[]>('/api/dashboard/feedback-by-category').pipe(
      map((items: any[]) => items.map(item => ({
        category: item.categoryName,
        count: item.feedbackCount
      })))
    );
  }

  getRecognitionByCategory$(): Observable<CategoryCount[]> {
    return this.api.get<any[]>('/api/dashboard/recognition-by-category').pipe(
      map((items: any[]) => items.map(item => ({
        category: item.categoryName,
        count: item.recognitionCount
      })))
    );
  }

  getFeedbackTypeDistribution$(): Observable<TypeDistribution[]> {
    // Using sentiment stats for type distribution
    return this.getSentimentStats().pipe(
      map(stats => [
        { type: 'Positive', count: stats.positiveCount },
        { type: 'Neutral', count: stats.neutralCount },
        { type: 'Negative', count: stats.negativeCount },
      ])
    );
  }

  getRecognitionTypeDistribution$(): Observable<TypeDistribution[]> {
    // Mock data for now - can be enhanced later
    return of([
      { type: 'Appreciation', count: 210 },
      { type: 'Kudos',        count: 70  },
      { type: 'Shout-outs',   count: 30  },
    ]).pipe(delay(150));
  }

  getMonthlyFeedbackTrend$(): Observable<MonthlyTrend[]> {
    return this.api.get<any>('/api/dashboard/monthly-trends', { months: 6 }).pipe(
      map((data: any) => {
        return data.labels.map((label: string, index: number) => ({
          month: label,
          count: data.feedbackCounts[index]
        }));
      })
    );
  }

  getMonthlyRecognitionTrend$(): Observable<MonthlyTrend[]> {
    return this.api.get<any>('/api/dashboard/monthly-trends', { months: 6 }).pipe(
      map((data: any) => {
        return data.labels.map((label: string, index: number) => ({
          month: label,
          count: data.recognitionCounts[index]
        }));
      })
    );
  }

  getActivityLog$(): Observable<ActivityItem[]> {
    // Activity log from ActivityController - mock for now since it needs different endpoint
    return of([
      { time: '2 min ago',  user: 'Amit',  action: 'Gave Feedback',    details: 'Communication' },
      { time: '10 min ago', user: 'Admin', action: 'Disabled Category',details: 'Time Management' },
      { time: '1 hr ago',   user: 'Neha',  action: 'Sent Recognition', details: 'Team Player' },
      { time: 'Today',      user: 'Ravi',  action: 'Gave Feedback',    details: 'Leadership' },
      { time: 'Today',      user: 'Priya', action: 'Updated Settings', details: 'Email templates' },
    ]).pipe(delay(150));
  }
}
