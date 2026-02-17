import { Injectable, inject } from '@angular/core';
import { SettingsService } from '../../core/services/settings.service';
import { AppSettings } from '../../core/models/settings.model';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ManagerService {
  private feedbackKey = 'feedback_db';
  private recognitionKey = 'recognition_db';
  private settingsService = inject(SettingsService);

  // --- Feedback Logic ---   
  getAllFeedback(): any[] {
    const data = localStorage.getItem(this.feedbackKey);
    return data ? JSON.parse(data) : [];
  }

  // --- Recognition Logic ---
  getAllRecognitions(): any[] {
    const data = localStorage.getItem(this.recognitionKey);
    return data ? JSON.parse(data) : [];
  }

  updateFeedbackStatus(id: any, newStatus: string): void {
    const feedbacks = this.getAllFeedback();
    const index = feedbacks.findIndex(f => f.id === id || f.feedbackId === id);
    if (index !== -1) {
      feedbacks[index].status = newStatus;
      localStorage.setItem(this.feedbackKey, JSON.stringify(feedbacks));
    }
  }

  // --- Settings Integration ---
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