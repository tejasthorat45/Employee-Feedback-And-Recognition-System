import { CommonModule } from '@angular/common';
import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { ManagerService } from '../service/manager.service';
import { ManagerDashboard, RecentActivity, CategoryStats } from '../models/manager.models';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';

@Component({
  selector: 'app-manager-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './manager-dashboard.component.html'
})
export class ManagerDashboardComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  // Dashboard data from API
  data: ManagerDashboard = {
    totalFeedback: 0,
    pendingReviews: 0,
    acknowledged: 0,
    resolved: 0,
    engagementScore: 0,
    totalRecognitions: 0,
    totalRecognitionPoints: 0
  };

  // Animated display values
  display = {
    totalFeedback: 0,
    pendingReviews: 0,
    engagementScore: 0,
    acknowledged: 0
  };

  // UI State
  searchQuery: string = '';
  statusFilter: string = 'All';
  isLoading: boolean = true;
  errorMessage: string | null = null;

  private timer: any;
  activities: (RecentActivity & { time: string; colorClass: string })[] = [];
  categoryData: { name: string; count: number; percent: number; colorClass: string }[] = [];

  constructor(private managerService: ManagerService) {}

  ngOnInit() {
    this.loadDashboardData();
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
    if (this.timer) clearInterval(this.timer);
  }

  /**
   * Load all dashboard data from API
   */
  loadDashboardData(): void {
    this.isLoading = true;
    this.errorMessage = null;

    // Load dashboard stats
    this.managerService.loadDashboard()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (dashboard) => {
          this.data = dashboard;
          this.animate();
        },
        error: (err) => {
          this.errorMessage = err.message || 'Failed to load dashboard';
          this.isLoading = false;
        }
      });

    // Load recent activity
    this.managerService.getRecentActivity(10)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (activities) => {
          this.activities = activities.map(a => ({
            ...a,
            time: this.calculateTimeAgo(a.createdAt),
            colorClass: this.getActivityColorClass(a.status)
          }));
        }
      });

    // Load category distribution
    this.loadCategoryDistribution();
  }

  /**
   * Get color class based on status
   */
  private getActivityColorClass(status: string): string {
    switch (status) {
      case 'Resolved':
      case 'Completed':
        return 'emerald';
      case 'Acknowledged':
        return 'blue';
      default:
        return 'amber';
    }
  } //this is used to calculate time ago from a given date string

  calculateTimeAgo(dateString: string): string {
    if (!dateString) return 'Just now';
    const now = new Date();
    const past = new Date(dateString);
    const diffInMs = now.getTime() - past.getTime();
    const diffInMins = Math.floor(diffInMs / (1000 * 60));

    if (diffInMins <= 1) return '1 min ago';
    if (diffInMins < 60) return `${diffInMins} min ago`;
    const diffInHours = Math.floor(diffInMins / 60);
    if (diffInHours < 24) return `${diffInHours} hours ago`;
    const diffInDays = Math.floor(diffInHours / 24);
    if (diffInDays === 1) return 'Yesterday';
    if (diffInDays < 7) return `${diffInDays} days ago`;
    return past.toLocaleDateString();
  }

  loadCategoryDistribution(): void {
    this.managerService.getCategoryDistribution(this.statusFilter)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (categories) => {
          const total = categories.reduce((sum, c) => sum + c.feedbackCount, 0) || 1;
          const colors = ['bg-indigo-500', 'bg-purple-500', 'bg-blue-500', 'bg-emerald-500', 'bg-amber-500'];

          this.categoryData = categories.map((c, index) => ({
            name: c.categoryName,
            count: c.feedbackCount,
            percent: Math.round((c.feedbackCount / total) * 100),
            colorClass: colors[index % colors.length]
          }));

          this.isLoading = false;
        },
        error: () => {
          this.isLoading = false;
        }
      });
  }  //this is used to handle filter changes for category distribution

  onFilterChange(status: string): void {
    this.statusFilter = status;
    this.loadCategoryDistribution();
  }

  get filteredActivities() {
    if (!this.searchQuery.trim()) return this.activities;
    
    const q = this.searchQuery.toLowerCase();
    return this.activities.filter(a =>
      a.userName.toLowerCase().includes(q) ||
      a.title.toLowerCase().includes(q) ||
      a.detail.toLowerCase().includes(q)
    );
  }//this is used to animate the dashboard statistics on load

  animate(): void {
    const steps = 60;
    const interval = 1000 / steps;
    let i = 0;

    if (this.timer) clearInterval(this.timer);

    this.timer = setInterval(() => {
      i++;
      const ratio = i / steps;
      this.display.totalFeedback = Math.round(this.data.totalFeedback * ratio);
      this.display.pendingReviews = Math.round(this.data.pendingReviews * ratio);
      this.display.engagementScore = Math.round(this.data.engagementScore * ratio);
      this.display.acknowledged = Math.round(this.data.acknowledged * ratio);

      if (i === steps) {
        clearInterval(this.timer);
        this.isLoading = false;
      }
    }, interval);
  }

  /**
   * Refresh dashboard data
   */
  refreshData(): void {
    this.managerService.refreshAll();
    this.loadDashboardData();
  }

  downloadReport() {
    const doc = new jsPDF();
    const timestamp = new Date().toLocaleString();

    doc.setFillColor(79, 70, 229);
    doc.rect(0, 0, 210, 40, 'F');
    doc.setTextColor(255, 255, 255);
    doc.setFontSize(22);
    doc.setFont('helvetica', 'bold');
    doc.text('Performance Dashboard Report', 14, 22);
    doc.setFontSize(10);
    doc.setFont('helvetica', 'normal');
    doc.text(`Generated on: ${timestamp}`, 14, 32);

    doc.setTextColor(50, 50, 50);
    doc.setFontSize(14);
    doc.text('Executive Summary', 14, 55);

    autoTable(doc, {
      startY: 60,
      head: [['Metric', 'Value']],
      body: [
        ['Total Feedback Received', this.data.totalFeedback.toString()],
        ['Pending Action Items', this.data.pendingReviews.toString()],
        ['Acknowledged/Resolved', this.data.acknowledged.toString()],
        ['Engagement Score', `${this.data.engagementScore}%`]
      ],
      theme: 'striped',
      headStyles: { fillColor: [79, 70, 229], textColor: 255 },
      styles: { fontSize: 11, cellPadding: 5 }
    }); //this is used to add category distribution table in the PDF

    const finalY = (doc as any).lastAutoTable.finalY + 15;
    doc.text('Category Distribution', 14, finalY);

    autoTable(doc, {
      startY: finalY + 5,
      head: [['Category', 'Count', 'Percentage']],
      body: this.categoryData.map(c => [c.name, c.count.toString(), `${c.percent}%`]),
      theme: 'grid',
      headStyles: { fillColor: [79, 70, 229] }
    });//this is used to add footer note in the PDF

    const pageHeight = doc.internal.pageSize.height;
    doc.setFontSize(9);
    doc.setTextColor(150);
    doc.text('This is an automated system report for management review.', 14, pageHeight - 10);

    doc.save(`Performance_Report_${Date.now()}.pdf`);
  }//this is used to calculate time ago from a given date string  
}