import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '../../core/services/api.service';

export interface DepartmentReadDto {
  departmentId: string;
  departmentName: string;
  description: string | null;
  isActive: boolean;
  createdAt: string;
}

export interface DepartmentCreateDto {
  departmentId: string;
  departmentName: string;
  description?: string;
}

export interface DepartmentUpdateDto {
  departmentName: string;
  description?: string;
  isActive: boolean;
}

@Injectable({ providedIn: 'root' })
export class AdminDepartmentService {
  constructor(private api: ApiService) {}

  getAll(): Observable<DepartmentReadDto[]> {
    return this.api.get<DepartmentReadDto[]>('/api/departments');
  }
  
  getById(departmentId: string): Observable<DepartmentReadDto> {
    return this.api.get<DepartmentReadDto>(`/api/departments/${departmentId}`);
  }
  
  create(payload: DepartmentCreateDto): Observable<DepartmentReadDto> {
    return this.api.post<DepartmentReadDto>('/api/departments', payload);
  }
  
  update(departmentId: string, payload: DepartmentUpdateDto): Observable<void> {
    return this.api.put<void>(`/api/departments/${departmentId}`, payload);
  }
  
  delete(departmentId: string): Observable<void> {
    return this.api.delete<void>(`/api/departments/${departmentId}`);
  }
}