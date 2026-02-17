import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { AdminDepartmentService, DepartmentReadDto, DepartmentCreateDto, DepartmentUpdateDto } from '../services/admin-dapartment.service';

export interface Department {
  departmentId: string;
  departmentName: string;
  description: string;
  isActive: boolean;
  createdAt: string;
}

@Component({
  selector: 'app-department-management',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './department-management.component.html',
  styleUrls: ['./department-management.component.css']
})
export class DepartmentManagementComponent {
  private svc = inject(AdminDepartmentService);
  private fb = inject(FormBuilder);

  departments = signal<Department[]>([]);
  loading = signal<boolean>(false);
  error = signal<string | null>(null);

  addMode = signal<boolean>(false);
  editRowId = signal<string | null>(null);

  addForm = this.fb.group({
    departmentName: ['', [Validators.required, Validators.minLength(2)]],
    description: ['']
  });

  editForm = this.fb.group({
    departmentName: ['', [Validators.required, Validators.minLength(2)]],
    description: ['']
  });

  constructor() {
    this.loadDepartments();
  }

  loadDepartments(): void {
    this.loading.set(true);
    this.error.set(null);
    this.svc.getAll().subscribe({
      next: (rows) => {
        const mapped = rows.map(dto => ({
          departmentId: dto.departmentId,
          departmentName: dto.departmentName,
          description: dto.description || '',
          isActive: dto.isActive,
          createdAt: dto.createdAt
        }));
        this.departments.set(mapped);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load departments.');
        this.loading.set(false);
      }
    });
  }

  /** Generates next departmentId like dept001, dept002, ... */
  private generateNextDepartmentId(): string {
    const prefix = 'dept';
    const nums = this.departments()
      .map(d => (d.departmentId || '').toLowerCase())
      .filter(id => id.startsWith(prefix))
      .map(id => {
        const match = id.slice(prefix.length).match(/^\d+$/)?.[0] ?? '0';
        return parseInt(match, 10) || 0;
      });

    const nextNum = (nums.length ? Math.max(...nums) : 0) + 1;
    return `${prefix}${String(nextNum).padStart(3, '0')}`;
  }

  /** UI: Add */
  startAdd(): void {
    this.addForm.reset({ departmentName: '', description: '' });
    this.addMode.set(true);
  }
  cancelAdd(): void {
    this.addMode.set(false);
  }
  saveAdd(): void {
    if (this.addForm.invalid) return;

    const payload: DepartmentCreateDto = {
      departmentId: this.generateNextDepartmentId(),
      departmentName: this.addForm.value.departmentName!.trim(),
      description: (this.addForm.value.description ?? '').trim()
    };

    this.loading.set(true);
    this.svc.create(payload).subscribe({
      next: (created) => {
        const newDept: Department = {
          departmentId: created.departmentId,
          departmentName: created.departmentName,
          description: created.description || '',
          isActive: created.isActive,
          createdAt: created.createdAt
        };
        this.departments.set([...this.departments(), newDept]);
        this.addMode.set(false);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to create department.');
        this.loading.set(false);
      }
    });
  }

  /** UI: Edit inline */
  startEdit(row: Department): void {
    this.editRowId.set(row.departmentId);
    this.editForm.reset({
      departmentName: row.departmentName,
      description: row.description
    });
  }
  cancelEdit(): void {
    this.editRowId.set(null);
  }
  saveEdit(row: Department): void {
    if (this.editForm.invalid) return;

    const payload: DepartmentUpdateDto = {
      departmentName: this.editForm.value.departmentName!.trim(),
      description: (this.editForm.value.description ?? '').trim(),
      isActive: row.isActive
    };

    this.loading.set(true);
    this.svc.update(row.departmentId, payload).subscribe({
      next: () => {
        const updated: Department = {
          ...row,
          departmentName: payload.departmentName,
          description: payload.description || ''
        };
        this.departments.set(
          this.departments().map(d => d.departmentId === updated.departmentId ? updated : d)
        );
        this.editRowId.set(null);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to update department.');
        this.loading.set(false);
      }
    });
  }

  /** UI: Delete */
  delete(row: Department): void {
    if (!confirm(`Delete department "${row.departmentName}"?`)) return;

    this.loading.set(true);
    this.error.set(null);
    this.svc.delete(row.departmentId).subscribe({
      next: () => {
        this.departments.set(this.departments().filter(d => d.departmentId !== row.departmentId));
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Delete error:', err);
        this.error.set(err?.error?.message || 'Failed to delete department. It may have associated users.');
        this.loading.set(false);
      }
    });
  }

  trackById = (_: number, item: Department) => item.departmentId;
}