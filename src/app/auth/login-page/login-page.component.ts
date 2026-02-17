// src/app/auth/login-page/login-page.component.ts
import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators, FormGroup } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { LoginService } from '../service/login.service';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login-page',
  standalone: true,
  templateUrl: './login-page.component.html',
  styleUrls: ['./login-page.component.css'],
  imports: [CommonModule, ReactiveFormsModule]
})
export class LoginPageComponent implements OnInit {
  role?: string;
  form: FormGroup;
  loading = false;

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private loginService: LoginService,
    private route: ActivatedRoute,
    private auth: AuthService,
  ) {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  ngOnInit() {
    this.route.queryParamMap.subscribe(params => {
      this.role = params.get('role') ?? undefined;
    });
  }
  

  get f() { return this.form.controls; }

  onLogin() {

    debugger;
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      
      return;
      
    }

    this.loading = true;

    const payload = {
      email: this.form.value.email,
      password: this.form.value.password
    };

    this.loginService.login(payload).subscribe({
      next: (res) => {
        // Save token + hydrate user from token (keeps your guards working)
        this.auth.loginWithToken(res.token);

        // Role normalization for redirect:
        const r = (this.role ?? res.user?.role ?? 'employee').toString().toLowerCase();
        const normalizedRole =
          r === 'admin' ? 'Admin' :
          r === 'manager' ? 'Manager' : 'Employee';

        const target =
          normalizedRole === 'Admin'   ? '/admin'   :
          normalizedRole === 'Manager' ? '/manager' :
                                         '/employee';

        this.router.navigate([target]);
      },
      error: () => {
        alert('Invalid credentials or inactive user.');
      },
      complete: () => this.loading = false
    });
  }

  goToRegister() {
    // Only employee is allowed through public registration
    this.router.navigate(['/auth/register-page'], { queryParams: { role: 'employee' } });
  }

  onForgotPassword() {
    alert('Forgot password clicked.');
  }
}