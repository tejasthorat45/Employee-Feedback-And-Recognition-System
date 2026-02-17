import {
  Component,
  OnInit,
  OnDestroy,
  HostBinding,
  signal
} from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { Subscription, debounceTime, distinctUntilChanged, filter } from 'rxjs';
import { AdminSettingsService } from '../services/admin-settings.service';

@Component({
  selector: 'app-admin-settings',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './admin-settings.component.html',
  styleUrls: ['./admin-settings.component.css']
})
export class AdminSettingsComponent implements OnInit, OnDestroy {
  // Dark theme class on host
  @HostBinding('class.theme-dark') get isDark() {
    return this.darkTheme();
  }
  darkTheme = signal(false);

  settingsForm!: FormGroup;
  loading = signal(true);
  saving = signal(false);

  showSaveToast = false;
  showErrorToast = false;

  private sub?: Subscription;
  private toastTimer?: any;

  constructor(
    private fb: FormBuilder,
    private settingsService: AdminSettingsService
  ) {}

  ngOnInit(): void {
    // Theme: localStorage -> OS preference -> light
    const stored = localStorage.getItem('adminTheme');
    if (stored) {
      this.darkTheme.set(stored === 'dark');
    } else if (window.matchMedia?.('(prefers-color-scheme: dark)').matches) {
      this.darkTheme.set(true);
    }

    // Build form
    this.settingsForm = this.fb.group({
      feedbackSettings: this.fb.group({
        allowAnonymousFeedback: [false],
        minimumFeedbackLength: [
          20,
          [Validators.required, Validators.min(1), Validators.max(2000)]
        ],
        requireCategorySelection: [true]
      }),
      recognitionSettings: this.fb.group({
        maxPointsPerRecognition: [
          10,
          [Validators.required, Validators.min(1), Validators.max(100)]
        ],
        monthlyPointsBudgetPerEmployee: [
          100,
          [Validators.required, Validators.min(1), Validators.max(1000)]
        ]
      }),
      notificationSettings: this.fb.group({
        enableEmailNotifications: [true],
        notifyOnFeedbackReceived: [true],
        notifyOnRecognitionReceived: [true]
      }),
      userSettings: this.fb.group({
        allowPublicEmployeeRegistration: [false],
        sessionTimeout: [
          30,
          [Validators.required, Validators.min(5), Validators.max(480)]
        ]
      })
    });

    // Initial load
    this.loadSettings();

    // Optional: light autosave feel (debounced). Remove if you don't want it.
    this.sub = this.settingsForm.valueChanges
      .pipe(
        debounceTime(800),
        distinctUntilChanged(),
        filter(() => this.settingsForm.valid && this.settingsForm.dirty)
      )
      .subscribe(() => this.saveSettings(true));
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
    clearTimeout(this.toastTimer);
  }

  toggleTheme() {
    const next = !this.darkTheme();
    this.darkTheme.set(next);
    localStorage.setItem('adminTheme', next ? 'dark' : 'light');
  }

  loadSettings() {
    this.loading.set(true);
    this.settingsService.getSettings().subscribe({
      next: (res) => {
        if (res) this.settingsForm.patchValue(res, { emitEvent: false });
        this.settingsForm.markAsPristine();
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  saveSettings(silent = false) {
    if (!this.settingsForm.valid) return;
    this.saving.set(true);
    this.settingsService.saveSettings(this.settingsForm.value).subscribe({
      next: () => {
        this.saving.set(false);
        this.settingsForm.markAsPristine();
        if (!silent) this.showToast('success');
      },
      error: () => {
        this.saving.set(false);
        if (!silent) this.showToast('error');
      }
    });
  }

  private showToast(type: 'success' | 'error') {
    clearTimeout(this.toastTimer);
    this.showSaveToast = type === 'success';
    this.showErrorToast = type === 'error';
    this.toastTimer = setTimeout(() => {
      this.showSaveToast = false;
      this.showErrorToast = false;
    }, 3000);
  }

  // Shorthands (optional)
  get fFb()  { return this.settingsForm.get('feedbackSettings')!; }
  get fRec() { return this.settingsForm.get('recognitionSettings')!; }
  get fNot() { return this.settingsForm.get('notificationSettings')!; }
  get fUsr() { return this.settingsForm.get('userSettings')!; }
}