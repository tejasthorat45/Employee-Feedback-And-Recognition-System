// src/app/core/services/settings.service.ts
import { Injectable, inject } from '@angular/core';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { ApiService } from './api.service';
import { AppSettings } from '../models/settings.model';

@Injectable({
  providedIn: 'root'
})
export class SettingsService {
  private readonly api = inject(ApiService);
  
  // Cache settings to avoid repeated API calls
  private settingsCache$ = new BehaviorSubject<AppSettings | null>(null);
  
  /**
   * Get application settings (all users can read)
   */
  getSettings(): Observable<AppSettings> {
    return this.api.get<AppSettings>('/api/settings').pipe(
      tap(settings => this.settingsCache$.next(settings))
    );
  }

  /**
   * Get cached settings without making an API call
   */
  getCachedSettings(): AppSettings | null {
    return this.settingsCache$.value;
  }

  /**
   * Get settings as observable stream
   */
  getSettings$(): Observable<AppSettings | null> {
    return this.settingsCache$.asObservable();
  }

  /**
   * Save application settings (Admin only)
   */
  saveSettings(settings: AppSettings): Observable<any> {
    return this.api.put<any>('/api/settings', settings).pipe(
      tap(() => this.settingsCache$.next(settings))
    );
  }

  /**
   * Load settings into cache
   */
  loadSettings(): void {
    this.getSettings().subscribe();
  }
}
