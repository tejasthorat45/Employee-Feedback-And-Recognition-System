
import { Routes } from '@angular/router';
import { AdminDashboardComponent } from './admin-dashboard/admin-dashboard.component';
import { AdminReportsComponent } from './admin-reports/admin-reports.component';
import { AdminSettingsComponent } from './admin-settings/admin-settings.component';
import { CategoryManagementComponent } from './category-management/category-management.component';
import { UserManagementComponent } from './user-management/user-management.component';
import { AdminLayoutComponent } from './admin-layout/admin-layout.component';


import { authGuard } from '../core/guards/auth.guard';
import { roleGuard } from '../core/guards/role.guard';
import { DepartmentManagementComponent } from './department-management/department-management.component';

export const ADMIN_ROUTES: Routes = [
  {
    path: '',
    component: AdminLayoutComponent,
    // ⬇️ add guards + roles here
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Admin'] },
    children: [
      { path: '', component: AdminDashboardComponent },
      { path: 'users', component: UserManagementComponent },
      { path: 'categories', component: CategoryManagementComponent },
      { path: 'reports', component: AdminReportsComponent },
      { path: 'settings', component: AdminSettingsComponent },
      { path: 'departments', component: DepartmentManagementComponent },
      { path: '**', component: AdminDashboardComponent  },

    ]
  }
];
