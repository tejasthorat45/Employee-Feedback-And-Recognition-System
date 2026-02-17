
import { Component, Injectable, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminCategoryService, CategoryReadDto, CategoryCreateDto, CategoryUpdateDto } from '../services/admin-category.service';

export interface Category {
  id: string;
  name: string;
  description: string;
  disabled?: boolean; // disabled=true -> Inactive
}

@Component({
  standalone: true,
  selector: 'app-category-management',
  imports: [CommonModule, FormsModule ],
  templateUrl: './category-management.component.html',
  styleUrls: ['./category-management.component.css']
})
@Injectable({ providedIn: 'root' })
export class CategoryManagementComponent implements OnInit {
  categories: Category[] = [];
  loading = false;
  error: string | null = null;

  // Inline Add/Edit form state (ID removed from the form model for add; still kept in object)
  formOpen = false;
  editIndex: number | null = null;
  formModel: Category = { id: '', name: '', description: '', disabled: false };

  constructor(private categoryService: AdminCategoryService) {}

  ngOnInit(): void {
    this.loadCategories();
  }

  /** Load categories from API */
  loadCategories(): void {
    this.loading = true;
    this.error = null;
    this.categoryService.getAll().subscribe({
      next: (data: CategoryReadDto[]) => {
        this.categories = data.map(dto => ({
          id: dto.categoryId,
          name: dto.categoryName,
          description: dto.description || '',
          disabled: !dto.isActive
        }));
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load categories';
        this.loading = false;
        console.error(err);
      }
    });
  }

  /** --- ID helpers --- */

  /** Extract numeric suffix from IDs like "CAT-001" -> 1 */
  private parseCatNumber(id: string): number {
    if (!id) return 0;
    const match = id.match(/\d+$/);
    return match ? parseInt(match[0], 10) : 0;
  }

  /** Format a number -> "CAT-XXX" (3-digit zero padding) */
  private formatCatId(num: number): string {
    const padded = String(num).padStart(3, '0');
    return `CAT-${padded}`;
  }

  /** Find max numeric suffix from existing IDs and return the next ID */
  private getNextCategoryId(list: Category[]): string {
    const maxNum = list.reduce((max, c) => {
      const n = this.parseCatNumber(c.id);
      return n > max ? n : max;
    }, 0);
    return this.formatCatId(maxNum + 1);
  }

  /** --- Form actions --- */

  /** Open empty form for adding a new category (ID is not part of the form) */
  openAddForm(): void {
    this.editIndex = null;
    this.formModel = { id: '', name: '', description: '', disabled: false };
    this.formOpen = true;
  }

  /** Open prefilled form for updating a category (ID displayed read-only) */
  editCategory(index: number): void {
    const cat = this.categories[index];
    this.editIndex = index;
    this.formModel = { ...cat };
    this.formOpen = true;
  }

  /** Disable/Enable -> toggles disabled flag */
  toggleDisabled(index: number): void {
    const cat = this.categories[index];
    const newStatus = !cat.disabled;
    
    const dto: CategoryUpdateDto = {
      categoryName: cat.name,
      description: cat.description,
      isActive: !newStatus
    };

    this.categoryService.update(cat.id, dto).subscribe({
      next: () => {
        cat.disabled = newStatus;
      },
      error: (err) => {
        this.error = 'Failed to update category status';
        console.error(err);
      }
    });
  }

  /** Save new or updated category */
  saveCategory(): void {
    const name = (this.formModel.name || '').trim();
    const desc = (this.formModel.description || '').trim();

    if (!name || !desc) {
      return; // basic guard; add validation UI if needed
    }

    if (this.editIndex === null) {
      // ADD: auto-generate ID and call API
      const id = this.getNextCategoryId(this.categories);

      const createDto: CategoryCreateDto = {
        categoryId: id,
        categoryName: name,
        description: desc
      };

      this.categoryService.create(createDto).subscribe({
        next: (created) => {
          const newCat: Category = {
            id: created.categoryId,
            name: created.categoryName,
            description: created.description || '',
            disabled: !created.isActive
          };
          this.categories = [...this.categories, newCat];
          this.closeForm();
        },
        error: (err) => {
          this.error = 'Failed to create category';
          console.error(err);
        }
      });
    } else {
      // UPDATE: keep ID unchanged; update other fields via API
      const currentId = this.categories[this.editIndex].id;

      const updateDto: CategoryUpdateDto = {
        categoryName: name,
        description: desc,
        isActive: !this.formModel.disabled
      };

      this.categoryService.update(currentId, updateDto).subscribe({
        next: () => {
          const updated: Category = {
            id: currentId,
            name,
            description: desc,
            disabled: !!this.formModel.disabled
          };
          const next = [...this.categories];
          next[this.editIndex!] = updated;
          this.categories = next;
          this.closeForm();
        },
        error: (err) => {
          this.error = 'Failed to update category';
          console.error(err);
        }
      });
    }
  }

  /** Close the inline form */
  closeForm(): void {
    this.formOpen = false;
    this.editIndex = null;
    this.formModel = { id: '', name: '', description: '', disabled: false };
  }
}
