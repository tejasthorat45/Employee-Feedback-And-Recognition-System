
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UserProfileComponent } from '../user-profile/user-profile.component';
import { UsersApiService, UserReadDto } from '../services/users-api.service';

export interface User {
  id: string;
  name: string;
  role: string;
  feedback: number;
  recognition: number;
  isActive: boolean;
  email: string;
  departmentId: string;
  departmentName?: string;
  createdAt: string;
}

@Component({
  standalone: true,
  selector: 'app-user-management',
  imports: [CommonModule, FormsModule, UserProfileComponent],
  templateUrl: './user-management.component.html',
  styleUrls: ['./user-management.component.css']
})
export class UserManagementComponent implements OnInit {
  users: User[] = [];
  loading = false;
  error: string | null = null;

  searchTerm = '';
  selectedUser: User | null = null;

  constructor(private usersApiService: UsersApiService) {}

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.loading = true;
    this.error = null;
    this.usersApiService.getAll().subscribe({
      next: (data: UserReadDto[]) => {
        this.users = data.map(dto => ({
          id: dto.userId,
          name: dto.fullName,
          role: dto.role,
          email: dto.email,
          departmentId: dto.departmentId,
          departmentName: dto.departmentName,
          feedback: 0,
          recognition: 0,
          isActive: dto.isActive,
          createdAt: dto.createdAt
        }));
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load users';
        this.loading = false;
        console.error(err);
      }
    });
  }

  get filteredUsers() {
    const q = this.searchTerm.trim().toLowerCase();
    if (!q) return this.users;

    return this.users.filter(u => {
      const name = (u.name ?? '').toLowerCase();
      const role = (u.role ?? '').toLowerCase();
      const idStr = String(u.id ?? '').toLowerCase();
      return name.includes(q) || role.includes(q) || idStr.includes(q);
    });
  }

  onSearchChange(): void {
    // No-op (kept for your API compatibility)
  }

  clearSearch(): void {
    this.searchTerm = '';
  }

  // Open profile from card click
  openProfile(user: User) {
    this.selectedUser = user;
  }

  // Close popup
  closeProfile() {
    this.selectedUser = null;
  }

  // Actions from profile
  editUser(userId: string) {
    console.log('Edit user', userId);
    // Hook: open an edit modal or inline form if desired.
  }

  toggleUserStatus(userId: string) {
    const idx = this.users.findIndex(u => u.id === userId);
    if (idx >= 0) {
      const current = this.users[idx];
      const newStatus = !current.isActive;
      
      // Update via API - backend requires all fields
      this.usersApiService.update(userId, {
        fullName: current.name,
        roleName: current.role,
        departmentId: current.departmentId,
        isActive: newStatus
      }).subscribe({
        next: () => {
          const next: User = { ...current, isActive: newStatus };
          this.users[idx] = next;
          if (this.selectedUser && this.selectedUser.id === userId) {
            this.selectedUser = next;
          }
        },
        error: (err) => {
          this.error = 'Failed to update user status';
          console.error(err);
        }
      });
    }
  }

  resetPassword(userId: string) {
    alert(`Password reset initiated for user ID ${userId}.`);
  }
}

