import { Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';
import { ApiService } from '../../core/services/api.service';

interface ProfileData {
  userId: string;
  fullName: string;
  email: string;
  phone: string | null;
  role: string;
  departmentId: string;
  departmentName: string;
  createdAt: string;
}

@Component({
  selector: 'app-manager-profile',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './manager-profile.component.html',
  styleUrl: './manager-profile.component.css'
})
export class ManagerProfileComponent implements OnInit {
  private authService = inject(AuthService);
  private apiService = inject(ApiService);
  private router = inject(Router);

  isEditing = false;
  isLoading = false;
  isSaving = false;
  errorMessage = '';
  successMessage = '';

  manager = {
    id: '',
    name: '',
    email: '',
    phone: '',
    department: '',
    departmentId: '',
    joinDate: '',
    role: '',
    location: 'Pune, India'
  };

  // Validation error flags
  emailError = '';
  phoneError = '';
  nameError = '';

  ngOnInit() {
    this.loadProfile();
  }

  loadProfile(): void {
    this.isLoading = true;
    this.errorMessage = '';
    
    this.apiService.get<ProfileData>('/api/users/profile').subscribe({
      next: (profile) => {
        // Load location from localStorage (not in database)
        const savedLocation = localStorage.getItem(`user_location_${profile.userId}`);
        
        this.manager = {
          id: profile.userId,
          name: profile.fullName,
          email: profile.email,
          phone: profile.phone || '',
          department: profile.departmentName,
          departmentId: profile.departmentId,
          joinDate: profile.createdAt.split('T')[0],
          role: profile.role,
          location: savedLocation || 'Pune, India'
        };
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Failed to load profile:', err);
        this.isLoading = false;
        
        // Fallback to auth service data
        this.authService.user$.subscribe(user => {
          if (user) {
            this.manager.id = user.id;
            this.manager.name = user.name;
            this.manager.email = user.email;
          }
        });
      }
    });
  }

  // Validates phone: only digits, max 10
  validatePhone(event: any) {
    const value = event.target.value;
    this.manager.phone = value.replace(/[^0-9]/g, '').slice(0, 10);
    this.phoneError = '';
  }

  // Validates email format on blur
  validateEmail() {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!this.manager.email) {
      this.emailError = 'Email is required';
    } else if (!emailRegex.test(this.manager.email)) {
      this.emailError = 'Please enter a valid email address';
    } else {
      this.emailError = '';
    }
  }

  // Validates name
  validateName() {
    if (!this.manager.name || this.manager.name.trim().length < 2) {
      this.nameError = 'Name must be at least 2 characters';
    } else {
      this.nameError = '';
    }
  }

  getInitials(name: string): string {
    if (!name) return 'U';
    return name.split(' ').map(n => n[0]).join('').toUpperCase();
  }

  toggleEdit() {
    this.isEditing = !this.isEditing;
    this.errorMessage = '';
    this.successMessage = '';
    this.emailError = '';
    this.phoneError = '';
    this.nameError = '';
  }

  saveChanges() {
    // Run all validations
    this.validateName();
    this.validateEmail();
    
    // Phone validation
    if (this.manager.phone && this.manager.phone.length !== 10) {
      this.phoneError = 'Phone number must be exactly 10 digits';
    }

    // Check if any errors exist
    if (this.nameError || this.emailError || this.phoneError) {
      this.errorMessage = 'Please fix the validation errors';
      return;
    }

    this.isSaving = true;
    this.errorMessage = '';
    this.successMessage = '';

    const updatePayload = {
      fullName: this.manager.name,
      email: this.manager.email,
      phone: this.manager.phone || null
    };

    this.apiService.put('/api/users/profile', updatePayload).subscribe({
      next: () => {
        this.isSaving = false;
        this.isEditing = false;
        this.successMessage = 'Profile updated successfully!';
        
        // Save location to localStorage (not in database)
        localStorage.setItem(`user_location_${this.manager.id}`, this.manager.location);
        
        // Update auth service with new name
        this.authService.updateUserName(this.manager.name);
        
        // Clear success message after 3 seconds
        setTimeout(() => this.successMessage = '', 3000);
      },
      error: (err) => {
        this.isSaving = false;
        this.errorMessage = err.error?.message || 'Failed to update profile. Please try again.';
      }
    });
  }

  onLogout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}