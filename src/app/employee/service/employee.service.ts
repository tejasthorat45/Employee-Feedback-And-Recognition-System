import { inject, Injectable } from "@angular/core";
import { AuthService } from "../../core/services/auth.service";
import { Observable, of, catchError, map } from "rxjs";
import { SettingsService } from "../../core/services/settings.service";
import { AppSettings } from "../../core/models/settings.model";
import { HttpClient, HttpParams } from "@angular/common/http";
import { environment } from "../../../environments/environment";


export interface Feedback {
  id?: number;
  feedbackId?: string;
  submittedByUserId: string;
  targetUserId: string;
  searchEmployee?: string;
  category: string;
  comments: string;
  isAnonymous: boolean;
  submissionDate: string;
}

export interface Recognition {
  id?: number;
  recognitionId?: string;
  fromUserId: string;
  toUserId: string;
  badgeId: string;
  BadgeType?: string;
  points: number;
  date: string;
  comment?: string;
  message?: string;
}


export interface Employee {
  id: string;
  userId?: string;
  name: string;
  fullName?: string;
  email?: string;
  departmentId?: string;
  departmentName?: string;
}

export interface Badge {
  badgeId: string;
  badgeName: string;
  description?: string;
  iconClass?: string;
  isActive: boolean;
}

export interface Category {
  categoryId: string;
  categoryName: string;
  description?: string;
  isActive: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class EmployeeService {

  private authService = inject(AuthService);
  private settingsService = inject(SettingsService);
  private http = inject(HttpClient);

  private readonly apiUrl = environment.apiUrl;

  // Storage Keys (for user data fallback only)
  private readonly KEYS = {
    USERS: 'feedback_project_users'
  };

  constructor() {
    // Clear old local storage data from previous implementation
    this.clearOldLocalStorage();
  }

  //. INTERNAL HELPERS --

  private clearOldLocalStorage() {
    // Remove old feedback and recognition data from local storage
    localStorage.removeItem('feedback_db');
    localStorage.removeItem('recognition_db');
  }

  getCurrentUserId(): string | null {
    return this.authService.getCurrentUserId() ?? '';
  }

  getCurrentUser(): string {
    return this.getCurrentUserId() ?? 'unknown';
  }

  // --- EMPLOYEE SEARCH WITH AUTOSEARCH ---

  searchEmployees(query: string): Observable<Employee[]> {
    console.log('🔍 Searching employees with query:', query);
    
    
    if (!query || query.trim().length < 2) {
      console.log('⚠️ Query too short, returning empty');
      return of([]);
    }

    let params = new HttpParams().set('search', query.trim());
    const url = `${this.apiUrl}/users/search`;
    console.log('📡 API URL:', url, 'Params:', params.toString());
    
    return this.http.get<any>(url, { params }).pipe(
      map(response => {
        console.log('✅ API Response:', response);
        if (response && Array.isArray(response)) {
          const mapped = response.map((user: any) => ({
            id: user.userId,
            userId: user.userId,
            name: user.fullName || user.name || user.userId,
            fullName: user.fullName,
            email: user.email,
            departmentId: user.departmentId,
            departmentName: user.departmentName
          }));
          console.log('🗺️ Mapped results:', mapped);
          return mapped;
        }
        console.log('⚠️ Response is not an array');
        return [];
      }),
      catchError(error => {
        console.error('❌ Error searching employees:', error);
        console.error('Error details:', error.status, error.statusText, error.error);
        // Fallback to local search
        return of(this.getAllEmployeesLocal().filter(emp => 
          emp.name.toLowerCase().includes(query.toLowerCase()) ||
          emp.id.toLowerCase().includes(query.toLowerCase())
        ));
      })
    );
  }

  private getAllEmployeesLocal(): Employee[] {
    const data = localStorage.getItem(this.KEYS.USERS);
    if (!data) return [];
    
    try {
      const users = JSON.parse(data);
      const myId = this.getCurrentUserId();
      
      return users
        .filter((u: any) => u.userId !== myId && ['employee', 'manager'].includes(u.role?.toLowerCase()))
        .map((u: any) => ({ 
          id: u.userId,
          userId: u.userId,
          name: u.name || u.username,
          fullName: u.fullName
        }));
    } catch { 
      return []; 
    }
  }

  // --- BADGES API ---

  getBadges(): Observable<Badge[]> {
    let params = new HttpParams().set('isActive', 'true').set('pageSize', '100');
    
    return this.http.get<any>(`${this.apiUrl}/badges`, { params }).pipe(
      map(response => {
        if (response && response.items) {
          return response.items;
        }
        return response || [];
      }),
      catchError(error => {
        console.error('Error fetching badges:', error);
        return of([]);
      })
    );
  }

  // --- CATEGORIES API ---

  getCategories(): Observable<Category[]> {
    let params = new HttpParams().set('isActive', 'true').set('pageSize', '100');
    
    return this.http.get<any>(`${this.apiUrl}/categories`, { params }).pipe(
      map(response => {
        if (response && response.items) {
          return response.items;
        }
        return response || [];
      }),
      catchError(error => {
        console.error('Error fetching categories:', error);
        return of([]);
      })
    );
  }

  // --- 3. FEEDBACK LOGIC ---

  getMyReceivedFeedback(): Observable<any> {
    return this.http.get(`${this.apiUrl}/my/feedback?direction=received`).pipe(
      map(response => ({ items: response })),
      catchError(error => {
        console.error('Error fetching received feedback:', error);
        return of({ items: [] });
      })
    );
  }

  getMySentFeedback(): Observable<any> {
    return this.http.get(`${this.apiUrl}/my/feedback?direction=given`).pipe(
      map(response => ({ items: response })),
      catchError(error => {
        console.error('Error fetching sent feedback:', error);
        return of({ items: [] });
      })
    );
  }

  saveFeedback(data: { targetUserId: string; category: string; comments: string; isAnonymous: boolean }): Observable<any> {
    const payload = {
      toUserId: data.targetUserId,
      categoryId: data.category,
      comments: data.comments,
      isAnonymous: data.isAnonymous
    };

    return this.http.post(`${this.apiUrl}/my/feedback`, payload).pipe(
      catchError(error => {
        console.error('Error saving feedback:', error);
        throw error;
      })
    );
  }

  // --- 4. RECOGNITION LOGIC ---

  getMyRecognitions(): Observable<any> {
    return this.http.get(`${this.apiUrl}/my/recognition?direction=received`).pipe(
      map(response => ({ items: response })),
      catchError(error => {
        console.error('Error fetching received recognitions:', error);
        return of({ items: [] });
      })
    );
  }

  getMySentRecognition(): Observable<any> {
    return this.http.get(`${this.apiUrl}/my/recognition?direction=given`).pipe(
      map(response => ({ items: response })),
      catchError(error => {
        console.error('Error fetching sent recognitions:', error);
        return of({ items: [] });
      })
    );
  }

  saveRecognition(data: { toUserId: string; badgeId: string; points: number; message: string }): Observable<any> {
    const payload = {
      toUserId: data.toUserId,
      badgeId: data.badgeId,
      points: data.points,
      message: data.message
    };

    return this.http.post(`${this.apiUrl}/my/recognition`, payload).pipe(
      catchError(error => {
        console.error('Error saving recognition:', error);
        throw error;
      })
    );
  }

  // --- 5. DASHBOARD STATS ---

  getDashboardStats(): Observable<any> {
    return this.http.get(`${this.apiUrl}/my/summary`).pipe(
      map((response: any) => {
        return [
          { label: 'Feedback Given', value: response.feedbackGivenCount || 0, trend: 12, icon: 'bi-pencil-square', bgClass: 'bg-primary-soft' },
          { label: 'Feedback Received', value: response.feedbackReceivedCount || 0, trend: 5, icon: 'bi-chat-left-dots', bgClass: 'bg-warning-soft' },
          { label: 'Recognition Given', value: response.recognitionGivenCount || 0, trend: 8, icon: 'bi-star', bgClass: 'bg-info-soft' },
          { label: 'Points Earned', value: response.totalPointsReceived || 0, trend: 20, icon: 'bi-award', bgClass: 'bg-success-soft' }
        ];
      }),
      catchError(error => {
        console.error('Error fetching dashboard stats:', error);
        return of([
          { label: 'Feedback Given', value: 0, trend: 12, icon: 'bi-pencil-square', bgClass: 'bg-primary-soft' },
          { label: 'Feedback Received', value: 0, trend: 5, icon: 'bi-chat-left-dots', bgClass: 'bg-warning-soft' },
          { label: 'Recognition Given', value: 0, trend: 8, icon: 'bi-star', bgClass: 'bg-info-soft' },
          { label: 'Points Earned', value: 0, trend: 20, icon: 'bi-award', bgClass: 'bg-success-soft' }
        ]);
      })
    );
  }

  getMySummary(): Observable<any> {
    return this.http.get(`${this.apiUrl}/my/summary`).pipe(
      catchError(error => {
        console.error('Error fetching summary:', error);
        return of({
          feedbackGivenCount: 0,
          feedbackReceivedCount: 0,
          recognitionGivenCount: 0,
          recognitionReceivedCount: 0,
          totalPointsGiven: 0,
          totalPointsReceived: 0,
          lastActivityAt: null
        });
      })
    );
  }

  private calculateTotalPoints(summary: any): number {
    return summary.totalPointsReceived || 0;
  }

  getDasboardStats(): Observable<any[]> {
    // Deprecated: Use getDashboardStats() instead
    return this.getDashboardStats();
  }

  // --- 6. EMPLOYEE LIST ---

  getAllEmployees(): Employee[] {
    const data = localStorage.getItem(this.KEYS.USERS);
    if (!data) return [];
    
    try {
      const users = JSON.parse(data);
      const myId = this.getCurrentUserId();
      
      return users
        .filter((u: any) => u.userId !== myId && ['employee', 'manager'].includes(u.role?.toLowerCase()))
        .map((u: any) => ({ id: u.userId, name: u.name || u.username }));
    } catch { 
      return []; 
    }
  }

  getEmployeeName(id: string): string {
    const emp = this.getAllEmployees().find(e => e.id === id);
    return emp ? emp.name : id;
  }

  // --- 7. SETTINGS INTEGRATION ---

  getSettings(): Observable<AppSettings> {
    return this.settingsService.getSettings();
  }

  getCachedSettings(): AppSettings | null {
    return this.settingsService.getCachedSettings();
  }

  getSettings$(): Observable<AppSettings | null> {
    return this.settingsService.getSettings$();
  }

}












