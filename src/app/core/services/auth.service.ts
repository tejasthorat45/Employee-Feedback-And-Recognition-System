import { Injectable } from '@angular/core';
import { BehaviorSubject, map, Observable } from 'rxjs';
import { TokenService } from './token.service';
import { User } from '../models/user.model';
import { Role } from '../models/role.model';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private _user$ = new BehaviorSubject<User | null>(null);
  
  // NEW: Key for saving user to Local Storage
  private readonly USER_KEY = 'mock_logged_in_user';

  /** Current user stream */
  readonly user$ = this._user$.asObservable();
  /** Logged-in flag */
  readonly isLoggedIn$ = this.user$.pipe(map(u => !!u));
  /** Roles stream */
  readonly roles$ = this.user$.pipe(map(u => u?.roles ?? []));

  constructor(private tokenSvc: TokenService) {
    // 1. TRY TO LOAD FROM LOCAL STORAGE (For Mock/Local Dev)
    const savedUser = localStorage.getItem(this.USER_KEY);
    if (savedUser) {
      this._user$.next(JSON.parse(savedUser));
    } 
    // 2. FALLBACK: TRY TO LOAD FROM TOKEN (For Future Real Backend)
    else {
      const token = this.tokenSvc.getToken();
      if (token) this.hydrateUserFromToken(token);
    }
  }

  //get current user value
  getCurrentUserId(): string | null {
    return this._user$.getValue()?.id ?? null;
  }
  
  getCurrentUserId$(): Observable<string | null> {
    return this.user$.pipe(map(u => u?.id ?? null));
  }

  getCurrentUserName$(): Observable<string | null> {
    return this.user$.pipe(map(u => u?.name ?? null));
  }

  /** Use this when backend returns a JWT */
  loginWithToken(token: string): void {
    this.tokenSvc.setToken(token);
    this.hydrateUserFromToken(token);
    
    // IMPORTANT: Also save to localStorage after hydrating from token
    const user = this._user$.getValue();
    if (user) {
      localStorage.setItem(this.USER_KEY, JSON.stringify(user));
    }
  }

  /** FIXED: Now saves to Local Storage */
  loginWithUser(user: User): void {
    // 1. Update the in-memory variable
    this._user$.next(user);
    
    // 2. SAVE to Local Storage (This fixes the refresh issue!)
    localStorage.setItem(this.USER_KEY, JSON.stringify(user));
  }

  /** Update user's name (for profile updates) */
  updateUserName(name: string): void {
    const currentUser = this._user$.getValue();
    if (currentUser) {
      const updatedUser = { ...currentUser, name };
      this._user$.next(updatedUser);
      localStorage.setItem(this.USER_KEY, JSON.stringify(updatedUser));
    }
  }

  /** FIXED: Clears Local Storage on logout */
  logout(): void {
    this.tokenSvc.clearToken();
    localStorage.removeItem(this.USER_KEY); // Clear the saved user
    this._user$.next(null);
  }

  // --- Role Helpers (Unchanged) ---
  hasRole$(role: Role): Observable<boolean> {
    return this.roles$.pipe(map(roles => roles.includes(role)));
  }

  hasAnyRole$(required: Role[]): Observable<boolean> {
    return this.roles$.pipe(map(roles => required.some(r => roles.includes(r))));
  }

  hasAllRoles$(required: Role[]): Observable<boolean> {
    return this.roles$.pipe(map(roles => required.every(r => roles.includes(r))));
  }

  private hydrateUserFromToken(token: string): void {
    const payload = this.tokenSvc.decodePayload<any>(token);
    const user: User | null = payload
      ? {
        id: payload.userId ?? payload.nameid ?? payload.sub ?? 'unknown',  // Priority: explicit userId, then nameid, then sub
        name: payload.unique_name ?? payload.name ?? payload.fullName ?? '',
        email: payload.email ?? '',
        roles: this.extractRoles(payload),
      }
      : null;
    this._user$.next(user);
  }

  private extractRoles(payload: any): Role[] {
    // JWT can have role as single string or array
    let roleValue = payload.role ?? payload.roles ?? payload[ClaimTypes_Role] ?? [];
    
    // Convert to array if single value
    const roleArray = Array.isArray(roleValue) ? roleValue : [roleValue];
    
    return roleArray
      .map((r: string) => normalizeRole(r))
      .filter(Boolean) as Role[];
  }
}

// ClaimTypes.Role constant from backend
const ClaimTypes_Role = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';

function normalizeRole(r: string): Role | null {
  if (!r) return null;
  const x = (r || '').trim().toLowerCase();
  if (x === 'admin') return 'Admin';
  if (x === 'manager') return 'Manager';
  if (x === 'employee') return 'Employee';
  return null;
}