import { Injectable, inject } from '@angular/core';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { ApiService } from '../../core/services/api.service';

@Injectable({
  providedIn: 'root'
})
export class AdminSettingsService {
  private readonly api = inject(ApiService);

  getSettings(): Observable<any> {
    return this.api.get<any>('/api/settings').pipe(
      catchError(() => of(null))   // fallback to defaults on error
    );
  }

  saveSettings(settings: any): Observable<any> {
    return this.api.put<any>('/api/settings', settings);
  }
}