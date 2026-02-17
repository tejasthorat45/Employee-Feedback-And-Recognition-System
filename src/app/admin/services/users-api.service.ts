
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '../../core/services/api.service';

export interface UserReadDto {
  userId: string;
  fullName: string;
  email: string;
  role: 'Admin' | 'Manager' | 'Employee' | string;
  departmentId: string;
  departmentName?: string;
  isActive: boolean;
  createdAt: string;
}

export interface UserCreateDto {
  userId: string;       // if you require custom ID format
  fullName: string;
  email: string;
  role: 'Admin' | 'Manager' | 'Employee';
  departmentName?: string;
  password?: string;    // only if admin creates with password
}

export interface UserUpdateDto {
  fullName: string;
  roleName: string;
  departmentId: string;
  isActive: boolean;
}

export interface UserStatsDto {
  total: number;
  admins: number;
  managers: number;
  employees: number;
  active: number;
  inactive: number;
}

@Injectable({ providedIn: 'root' })
export class UsersApiService {
  constructor(private api: ApiService) {}

  getAll(): Observable<UserReadDto[]> {
    return this.api.get<UserReadDto[]>('/api/users');
  }

  getById(id: string): Observable<UserReadDto> {
    return this.api.get<UserReadDto>(`/api/users/${id}`);
  }

  create(dto: UserCreateDto): Observable<UserReadDto> {
    return this.api.post<UserReadDto>('/api/users', dto);
  }

  update(id: string, dto: UserUpdateDto): Observable<void> {
    return this.api.put<void>(`/api/users/${id}`, dto);
  }

  delete(id: string): Observable<void> {
    return this.api.delete<void>(`/api/users/${id}`);
  }

  getStats(): Observable<UserStatsDto> {
    return this.api.get<UserStatsDto>('/api/users/stats');
  }
}