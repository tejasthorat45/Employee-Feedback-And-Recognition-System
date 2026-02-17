import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '../../core/services/api.service';

export interface AuthResponseDto {
  token: string;
  expiresAt?: string;
  user: {
    userId: string;
    fullName?: string;
    name?: string;
    email: string;
    role: 'Admin' | 'Manager' | 'Employee' | string;
    department?: string;
  };
}

@Injectable({ providedIn: 'root' })
export class LoginService {
  constructor(private api: ApiService) {}

  // Backend LoginRequestDto expects Email + Password
  login(payload: { email: string; password: string }): Observable<AuthResponseDto> {
    return this.api.post<AuthResponseDto>('/api/auth/login', payload);
  }
}