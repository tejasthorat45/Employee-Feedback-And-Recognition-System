import { Component, HostListener, inject, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, NavigationEnd, RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { filter, Subscription } from 'rxjs';
import { AuthService } from '../../core/services/auth.service'; 
import { User } from '../../core/models/user.model';
import { ManagerApiService } from '../service/manager-api.service';
import { NotificationItem } from '../models/manager.models';

@Component({
  selector: 'app-manager-layout',
  standalone: true,
  imports: [ 
    CommonModule,
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
  ],
  templateUrl: './manager-layout.component.html',
  styleUrls: ['./manager-layout.component.css']
})
export class ManagerLayoutComponent implements OnInit, OnDestroy {
  private authService = inject(AuthService);
  private router = inject(Router);
  private apiService = inject(ManagerApiService);
  private sub = new Subscription();

  isSidebarOpen = false;
  isProfileOpen = false;
  isNotificationOpen = false;
  notificationCount = 0;
  notifications: NotificationItem[] = [];
  isLoadingNotifications = false;
  currentUser: User | null = null;

  constructor() {
    // Auto-close sidebar/popup on navigation
    this.sub.add(
      this.router.events
        .pipe(filter((e): e is NavigationEnd => e instanceof NavigationEnd))
        .subscribe(() => {
          if (!this.isDesktop()) this.isSidebarOpen = false;
          this.isProfileOpen = false;
          this.isNotificationOpen = false;
        })
    );
  }

  ngOnInit(): void {
    // Reactively update user details (Name, Email, ID) from the Service/Token
    this.sub.add(
      this.authService.user$.subscribe(user => {
        this.currentUser = user;
        // Optional: Redirect to login if user becomes null (token expires/logout)
        if (!user) {
          this.router.navigate(['/login']);
        }
      })
    );
    // Load initial notification count
    this.loadNotificationCount();
  }

  ngOnDestroy(): void {
    this.sub.unsubscribe();
  }

  loadNotificationCount(): void {
    this.apiService.getNotificationCount().subscribe({
      next: (count) => {
        this.notificationCount = count.unread;
      },
      error: (err) => console.error('Failed to load notification count:', err)
    });
  }

  toggleNotifications(): void {
    this.isNotificationOpen = !this.isNotificationOpen;
    this.isProfileOpen = false; // Close profile popup
    
    if (this.isNotificationOpen && this.notifications.length === 0) {
      this.loadNotifications();
    }
  }

  loadNotifications(): void {
    this.isLoadingNotifications = true;
    this.apiService.getNotifications().subscribe({
      next: (notifications) => {
        this.notifications = notifications.slice(0, 10); // Show latest 10
        this.isLoadingNotifications = false;
      },
      error: (err) => {
        console.error('Failed to load notifications:', err);
        this.isLoadingNotifications = false;
      }
    });
  }

  markAsRead(notification: NotificationItem): void {
    if (notification.isRead) return;
    
    this.apiService.markAsRead(notification.notificationId).subscribe({
      next: () => {
        notification.isRead = true;
        this.notificationCount = Math.max(0, this.notificationCount - 1);
      },
      error: (err) => console.error('Failed to mark as read:', err)
    });
  }

  markAllAsRead(): void {
    if (this.notificationCount === 0) return;
    
    this.apiService.markAllAsRead().subscribe({
      next: () => {
        this.notifications.forEach(n => n.isRead = true);
        this.notificationCount = 0;
      },
      error: (err) => console.error('Failed to mark all as read:', err)
    });
  }

  getTimeAgo(dateString: string): string {
    const date = new Date(dateString);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMins / 60);
    const diffDays = Math.floor(diffHours / 24);

    if (diffMins < 1) return 'Just now';
    if (diffMins < 60) return `${diffMins}m ago`;
    if (diffHours < 24) return `${diffHours}h ago`;
    if (diffDays < 7) return `${diffDays}d ago`;
    return date.toLocaleDateString();
  }

  onLogout(): void {
    this.authService.logout(); // Clears localStorage via TokenService
    this.router.navigate(['/login']);
  }

  toggleSidebar(): void {
    this.isSidebarOpen = !this.isSidebarOpen;
  }

  closeSidebar(): void {
    this.isSidebarOpen = false;
  }

  toggleProfile(): void {
    this.isProfileOpen = !this.isProfileOpen;
    this.isNotificationOpen = false; // Close notification popup
  }

  getInitials(name: string | undefined): string {
    if (!name) return 'U';
    return name.split(' ').map(n => n[0]).join('').toUpperCase();
  }

  @HostListener('document:click', ['$event'])
  clickout(event: any) {
    if (!event.target.closest('.profile-container')) {
      this.isProfileOpen = false;
    }
    if (!event.target.closest('.notification-container')) {
      this.isNotificationOpen = false;
    }
  }

  @HostListener('document:keydown.escape')
  onEsc(): void {
    this.isSidebarOpen = false;
    this.isProfileOpen = false;
    this.isNotificationOpen = false;
  }

  private isDesktop(): boolean {
    return window.matchMedia('(min-width: 1400px)').matches;
  }
}