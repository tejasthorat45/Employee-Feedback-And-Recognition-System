import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { EmployeeService, Feedback, Category } from '../service/employee.service';


@Component({
  selector: 'app-submit-feedback',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './submit-feedback.component.html',
  styleUrl: './submit-feedback.component.css'
})
export class SubmitFeedbackComponent implements OnInit {

  feedbackForm!: FormGroup;

  employees: any[] = [];

  categories: Category[] = [];

  // SIGNALS for UI State
  filteredEmployees = signal<any[]>([]);
  selectedEmp = signal<any>(null);
  isSearching = signal<boolean>(false);

  // Settings-related properties
  allowAnonymous = true;
  minFeedbackLength = 10;
  requireCategory = true;

  constructor(private fb: FormBuilder, private empService: EmployeeService) { }

  ngOnInit(): void {

    this.employees = this.empService.getAllEmployees();

    // Load categories from API
    this.empService.getCategories().subscribe(cats => {
      this.categories = cats;
    });

    // Load settings and apply them
    this.empService.getSettings().subscribe(settings => {
      if (settings) {
        this.allowAnonymous = settings.feedbackSettings.allowAnonymousFeedback;
        this.minFeedbackLength = settings.feedbackSettings.minimumFeedbackLength;
        this.requireCategory = settings.feedbackSettings.requireCategorySelection;
        
        // Update form validators based on settings
        this.updateFormValidators();
      }
    });

    this.feedbackForm = this.fb.group({
      employeeName: ['', Validators.required],
      employeeId: ['', Validators.required], // Enable this field
      category: ['', this.requireCategory ? Validators.required : []],
      comments: ['', [Validators.required, Validators.minLength(this.minFeedbackLength)]],
      isAnonymous: [false],
      submissionDate: [new Date().toISOString().substring(0, 10), Validators.required]
    });

  }

  onNameChange(event: any) {
    const query = event.target.value.trim();
    
    // Reset selected signal and employee ID when user types
    this.selectedEmp.set(null);
    this.feedbackForm.get('employeeId')?.setValue('', { emitEvent: false });

    if (query.length > 1) {
      this.isSearching.set(true);
      
      // Use API search
      this.empService.searchEmployees(query).subscribe({
        next: (results) => {
          this.filteredEmployees.set(results);
          this.isSearching.set(false);
        },
        error: (err) => {
          console.error('Search error:', err);
          this.isSearching.set(false);
        }
      });
    } else {
      this.filteredEmployees.set([]);
      this.isSearching.set(false);
    }
  }

  onEmployeeIdChange(event: any) {
    const empId = event.target.value.trim();
    
    // Reset name field and selected employee
    this.selectedEmp.set(null);
    this.feedbackForm.get('employeeName')?.setValue('', { emitEvent: false });

    if (empId.length > 1) {
      this.isSearching.set(true);
      
      // Search by employee ID
      this.empService.searchEmployees(empId).subscribe(results => {
        this.filteredEmployees.set(results);
        this.isSearching.set(false);
        
        // Auto-select if exact match found
        const exactMatch = results.find(emp => 
          emp.id?.toLowerCase() === empId.toLowerCase() || 
          emp.userId?.toLowerCase() === empId.toLowerCase()
        );
        
        if (exactMatch) {
          this.selectEmployee(exactMatch);
        }
      });
    } else {
      this.filteredEmployees.set([]);
      this.isSearching.set(false);
    }
  }

  selectEmployee(emp: any) {
    // Set the signal
    this.selectedEmp.set(emp);
    
    // Patch the Reactive Form with all available data
    this.feedbackForm.patchValue({
      employeeName: emp.name || emp.fullName,
      employeeId: emp.id || emp.userId
    }, { emitEvent: false });
    
    // Clear the dropdown list
    this.filteredEmployees.set([]);
  }

  private updateFormValidators(): void {
    const categoryControl = this.feedbackForm.get('category');
    const commentsControl = this.feedbackForm.get('comments');

    if (categoryControl) {
      categoryControl.setValidators(this.requireCategory ? Validators.required : []);
      categoryControl.updateValueAndValidity();
    }

    if (commentsControl) {
      commentsControl.setValidators([
        Validators.required,
        Validators.minLength(this.minFeedbackLength)
      ]);
      commentsControl.updateValueAndValidity();
    }
  }

  onSubmit(): void {
    if (this.feedbackForm.valid) {
      const formValue = this.feedbackForm.getRawValue();

      // Validate employee ID is provided
      if (!formValue.employeeId || formValue.employeeId.trim() === '') {
        alert('Please enter or select a valid Employee ID');
        return;
      }

      const isAnonymous = formValue.isAnonymous;

      // Check if anonymous feedback is allowed
      if (isAnonymous && !this.allowAnonymous) {
        alert('Anonymous feedback is not allowed by system settings.');
        return;
      }

      const finalData = {
        targetUserId: formValue.employeeId,
        category: formValue.category,
        comments: formValue.comments,
        isAnonymous: isAnonymous
      };

      this.empService.saveFeedback(finalData).subscribe({
        next: (response) => {
          console.log('Feedback saved successfully', response);
          alert('✅ Feedback submitted successfully!');
          this.resetForm();
        },
        error: (error) => {
          console.error('Error saving feedback:', error);
          const errorMsg = error.error?.message || 'Error submitting feedback. Please try again.';
          alert('❌ ' + errorMsg);
        }
      });
    } else {
      alert('Please fill in all required fields and select an employee');
      this.feedbackForm.markAllAsTouched();
    }
  }

  resetForm(): void {
    this.feedbackForm.reset({
      submissionDate: new Date().toISOString().substring(0, 10),
      isAnonymous: false
    });
    this.selectedEmp.set(null);
    this.filteredEmployees.set([]);
  }
}