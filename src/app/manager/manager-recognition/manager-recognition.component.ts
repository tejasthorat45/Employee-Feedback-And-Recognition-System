import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject, takeUntil } from 'rxjs';
import { ManagerService } from '../service/manager.service';
import { RecognitionItem } from '../models/manager.models';

@Component({
  selector: 'app-manager-recognition',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './manager-recognition.component.html'
})
export class ManagerRecognitionComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  
  recognitions: RecognitionItem[] = [];
  isLoading = false;
  errorMessage: string | null = null;

  constructor(private managerService: ManagerService) {}

  ngOnInit(): void {
    this.loadData();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadData(): void {
    this.isLoading = true;
    this.errorMessage = null;
    
    this.managerService.getAllRecognitions()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (items) => {
          // Sort so the latest recognition is at the top
          this.recognitions = items.sort((a, b) => 
            new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
          );
          this.isLoading = false;
        },
        error: (err) => {
          this.errorMessage = err.message || 'Failed to load recognitions';
          this.isLoading = false;
        }
      });
  }

  getBadgeTheme(categoryName: string, points: number) {
    const icons: Record<string, string> = {
      'Leader': 'bi-rocket-takeoff-fill',
      'Team Player': 'bi-people-fill',
      'Problem Solver': 'bi-cpu-fill',
      'Innovator': 'bi-lightbulb-fill',
      'Leadership': 'bi-rocket-takeoff-fill',
      'Teamwork': 'bi-people-fill',
      'Innovation': 'bi-lightbulb-fill',
      'Excellence': 'bi-star-fill'
    };

    let themeColor: string;
    if (points >= 8) themeColor = '#22c55e';      // Green
    else if (points >= 6) themeColor = '#f59e0b'; // Amber
    else themeColor = '#ef4444';                  // Red

    return {
      color: themeColor,
      icon: icons[categoryName] || 'bi-award-fill'
    };
  }
}