import { HttpClient } from '@angular/common/http';
import { Injectable, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthSession } from '../models/api.models';

interface AuthEnvelope<T> {
  succeeded: boolean;
  data: T;
  error: { code: string; message: string } | null;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly tokenKey = 'orderflow.access-token';
  private readonly sessionKey = 'orderflow.session';
  private readonly currentSession = signal<AuthSession | null>(this.readSession());

  readonly session = this.currentSession.asReadonly();
  readonly isAuthenticated = signal(Boolean(this.readToken()));

  constructor(private readonly http: HttpClient) {}

  login(email: string, password: string): Observable<AuthEnvelope<AuthSession>> {
    return this.http
      .post<AuthEnvelope<AuthSession>>(`${environment.authApiUrl}/login`, { email, password })
      .pipe(tap((response) => this.persistSession(response)));
  }

  register(displayName: string, email: string, password: string): Observable<AuthEnvelope<AuthSession>> {
    return this.http
      .post<AuthEnvelope<AuthSession>>(`${environment.authApiUrl}/register`, { displayName, email, password })
      .pipe(tap((response) => this.persistSession(response)));
  }

  readToken(): string | null {
    return sessionStorage.getItem(this.tokenKey);
  }

  logout(): void {
    sessionStorage.removeItem(this.tokenKey);
    sessionStorage.removeItem(this.sessionKey);
    this.currentSession.set(null);
    this.isAuthenticated.set(false);
  }

  private readSession(): AuthSession | null {
    try {
      const serialized = sessionStorage.getItem(this.sessionKey);
      return serialized ? (JSON.parse(serialized) as AuthSession) : null;
    } catch {
      return null;
    }
  }

  private persistSession(response: AuthEnvelope<AuthSession>): void {
    if (!response.succeeded || !response.data.accessToken) {
      return;
    }

    sessionStorage.setItem(this.tokenKey, response.data.accessToken);
    sessionStorage.setItem(this.sessionKey, JSON.stringify(response.data));
    this.currentSession.set(response.data);
    this.isAuthenticated.set(true);
  }
}