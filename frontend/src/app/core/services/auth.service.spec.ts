import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';

describe('AuthService', () => {
  let service: AuthService;
  let http: HttpTestingController;

  beforeEach(() => {
    sessionStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(AuthService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.verify();
    sessionStorage.clear();
  });

  it('registers an account, stores its bearer token, and clears it on logout', () => {
    const session = {
      accountId: 4,
      email: 'member@example.test',
      displayName: 'Team Member',
      accessToken: 'access-token-test',
      accessExpiresAtUtc: '2026-09-29T20:00:00Z',
      refreshToken: 'refresh-token-test',
      refreshExpiresAtUtc: '2026-10-13T20:00:00Z',
    };

    service.register('Team Member', 'member@example.test', 'SecurePassword123').subscribe((response) => {
      expect(response.succeeded).toBeTrue();
      expect(service.isAuthenticated()).toBeTrue();
      expect(service.session()).toEqual(session);
      expect(service.readToken()).toBe('access-token-test');
    });

    const request = http.expectOne(`${environment.authApiUrl}/register`);
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({
      displayName: 'Team Member',
      email: 'member@example.test',
      password: 'SecurePassword123',
    });
    request.flush({ succeeded: true, data: session, error: null });

    service.logout();
    expect(service.isAuthenticated()).toBeFalse();
    expect(service.readToken()).toBeNull();
  });
});