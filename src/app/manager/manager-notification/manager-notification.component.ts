import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ManagerApiService } from '../service/manager-api.service';
import { NotificationItem } from '../models/manager.models';

@Component({
  selector: 'app-manager-notification',
  imports: [CommonModule],
  templateUrl: './manager-notification.component.html',
  styleUrl: './manager-notification.component.css'
})
export class ManagerNotificationComponent implements OnInit {
  private apiService = inject(ManagerApiService);

  notifications: NotificationItem[] = [];
  isLoading = true;
  error: string | null = null;

  get unreadCount(): number {
    return this.notifications.filter(n => !n.isRead).length;
  }

  get hasNotifications(): boolean {
    return this.notifications.length > 0;
  }

  ngOnInit(): void {
    this.loadNotifications();
  }

  loadNotifications(): void {
    this.isLoading = true;
    this.error = null;

    this.apiService.getNotifications().subscribe({
      next: (notifications) => {
        this.notifications = notifications;
        this.isLoading = false;
      },
      error: (err) => {
        this.error = err.message || 'Failed to load notifications';
        this.isLoading = false;
      }
    });
  }

  markAsRead(notification: NotificationItem): void {
    if (notification.isRead) return;

    this.apiService.markAsRead(notification.notificationId).subscribe({
      next: () => {
        notification.isRead = true;
      },
      error: (err) => {
        console.error('Failed to mark notification as read:', err);
      }
    });
  }

  markAllAsRead(): void {
    if (this.unreadCount === 0) return;

    this.apiService.markAllAsRead().subscribe({
      next: () => {
        this.notifications.forEach(n => n.isRead = true);
      },
      error: (err) => {
        console.error('Failed to mark all notifications as read:', err);
      }
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
    if (diffMins < 60) return `${diffMins} min${diffMins > 1 ? 's' : ''} ago`;
    if (diffHours < 24) return `${diffHours} hour${diffHours > 1 ? 's' : ''} ago`;
    if (diffDays < 7) return `${diffDays} day${diffDays > 1 ? 's' : ''} ago`;
    return date.toLocaleDateString();
  }
}
