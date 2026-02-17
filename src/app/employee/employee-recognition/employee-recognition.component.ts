import { CommonModule } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { EmployeeService, Recognition, Badge } from '../service/employee.service';

@Component({
  selector: 'app-employee-recognition',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './employee-recognition.component.html',
  styleUrl: './employee-recognition.component.css'
})
export class EmployeeRecognitionComponent implements OnInit {
  
  
  private fb = inject(FormBuilder);
  private empService = inject(EmployeeService);

  recognitionForm: FormGroup;
  employees: any[] = []; 
  badges: Badge[] = [];

  // 2. SIGNALS for UI State (The Angular 19 Way)
  filteredEmployees = signal<any[]>([]); 
  selectedEmp = signal<any>(null);
  isSearching = signal<boolean>(false);

  // Settings-related properties
  maxPoints = 10;
  monthlyBudget = 100;

  constructor() {
    this.recognitionForm = this.fb.group({
      employeeName: ['', Validators.required],
      employeeId: ['', Validators.required], // Enable this field
      badgeType: ['', Validators.required],
      points: [5, [Validators.required, Validators.min(1), Validators.max(this.maxPoints)]],
      comment: ['', [Validators.required, Validators.minLength(5)]]
    });
  }

  ngOnInit(): void {
    this.employees = this.empService.getAllEmployees();

    // Load badges from API
    this.empService.getBadges().subscribe(badges => {
      this.badges = badges;
    });

    // Load settings and apply them
    this.empService.getSettings().subscribe(settings => {
      if (settings) {
        this.maxPoints = settings.recognitionSettings.maxPointsPerRecognition;
        this.monthlyBudget = settings.recognitionSettings.monthlyPointsBudgetPerEmployee;
        
        // Update form validators based on settings
        const pointsControl = this.recognitionForm.get('points');
        if (pointsControl) {
          pointsControl.setValidators([
            Validators.required,
            Validators.min(1),
            Validators.max(this.maxPoints)
          ]);
          pointsControl.updateValueAndValidity();
        }
      }
    });
  }

  // Getter for easy access in HTML
  get f() { return this.recognitionForm.controls; }

  // Dynamic color helper
  getBadgeTheme() {
    const badge = this.recognitionForm.get('badgeType')?.value;
    if (badge === 'Leader') return 'border-danger text-danger';
    if (badge === 'Team Player') return 'border-success text-success';
    if (badge === 'Problem Solver') return 'border-info text-info';
    if (badge === 'Innovator') return 'border-warning text-warning';
    return 'border-primary';
  }

  onNameChange(event: any) {
    const query = event.target.value.trim();
    
    // Reset selected signal and employee ID
    this.selectedEmp.set(null);
    this.recognitionForm.get('employeeId')?.setValue('', { emitEvent: false });

    if (query.length > 1) {
      this.isSearching.set(true);
      
      // Use API search
      this.empService.searchEmployees(query).subscribe(results => {
        this.filteredEmployees.set(results);
        this.isSearching.set(false);
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
    this.recognitionForm.get('employeeName')?.setValue('', { emitEvent: false });

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
    this.recognitionForm.patchValue({
      employeeName: emp.name || emp.fullName,
      employeeId: emp.id || emp.userId
    }, { emitEvent: false });
    
    // Clear the dropdown list
    this.filteredEmployees.set([]);
  }

  onSubmit() {
    const rawForm = this.recognitionForm.getRawValue();
    
    // Validate employee ID is provided
    if (!rawForm.employeeId || rawForm.employeeId.trim() === '') {
      alert('Please enter or select a valid Employee ID');
      this.recognitionForm.markAllAsTouched();
      return;
    }
    
    // Check signal value using parentheses: this.selectedEmp()
    if (this.recognitionForm.valid) {
      // Validate points against max setting
      if (rawForm.points > this.maxPoints) {
        alert(`Maximum points per recognition is ${this.maxPoints}. Please adjust.`);
        return;
      }

      const recognitionData = {
        toUserId: rawForm.employeeId,
        badgeId: rawForm.badgeType,
        points: rawForm.points,
        message: rawForm.comment
      };

      this.empService.saveRecognition(recognitionData).subscribe({
        next: (response) => {
          console.log('Recognition saved successfully', response);
          const empName = this.selectedEmp()?.name || this.selectedEmp()?.fullName || rawForm.employeeId;
          alert(`✅ Recognition sent to ${empName}!`);
          this.resetForm();
        },
        error: (error) => {
          console.error('Error saving recognition:', error);
          const errorMsg = error.error?.message || 'Error sending recognition. Please try again.';
          alert('❌ ' + errorMsg);
        }
      });
    } else {
      alert('Please fill in all required fields and select an employee');
    }
  }

  resetForm(): void {
    this.recognitionForm.reset({
      points: 5,
      badgeType: '',
      employeeName: ''
    });
    this.selectedEmp.set(null);
    this.filteredEmployees.set([]);
  }
}