import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';

export interface LoginResponse {
  token: string;
  user: any;
}

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private apiUrl = '/api';
  private authState = new BehaviorSubject<boolean>(this.isAuthenticated());
  authState$ = this.authState.asObservable();

  constructor(private http: HttpClient) { }

  login(email: string, password: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/user/login`, { email, password })
      .pipe(
        tap(response => {
          this.saveToken(response.token);
          if (response.user?.id) localStorage.setItem('userId', response.user.id);
          if (response.user?.name) localStorage.setItem('userName', response.user.name);
          this.authState.next(true);
        })
      );
  }
  saveToken(token: string): void {
    localStorage.setItem('token', token);
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  getUserId(): string | null {
    return localStorage.getItem('userId');
  }

  getUserName(): string | null {
    return localStorage.getItem('userName');
  }

  register(firstName: string, lastName: string, email: string, password: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/user/create`, {
      firstName,
      lastName,
      email,
      password
    });
  }

  updateAuthState(isAuthenticated: boolean): void {
    this.authState.next(isAuthenticated);
  }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('userId');
    localStorage.removeItem('userName');
    this.authState.next(false);
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }
}
