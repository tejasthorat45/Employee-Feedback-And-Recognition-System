import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '../../core/services/api.service';

export interface CategoryReadDto {
  categoryId: string;
  categoryName: string;
  description: string | null;
  isActive: boolean;
  createdAt: string;
}

export interface CategoryCreateDto {
  categoryId: string;
  categoryName: string;
  description?: string;
}

export interface CategoryUpdateDto {
  categoryName: string;
  description?: string;
  isActive: boolean;
}

@Injectable({ providedIn: 'root' })
export class AdminCategoryService {
  constructor(private api: ApiService) {}

  getAll(): Observable<CategoryReadDto[]> {
    return this.api.get<CategoryReadDto[]>('/api/categories');
  }

  getById(id: string): Observable<CategoryReadDto> {
    return this.api.get<CategoryReadDto>(`/api/categories/${id}`);
  }

  create(dto: CategoryCreateDto): Observable<CategoryReadDto> {
    return this.api.post<CategoryReadDto>('/api/categories', dto);
  }

  update(id: string, dto: CategoryUpdateDto): Observable<void> {
    return this.api.put<void>(`/api/categories/${id}`, dto);
  }

  delete(id: string): Observable<void> {
    return this.api.delete<void>(`/api/categories/${id}`);
  }
}
