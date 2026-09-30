import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { provideRouter, Router } from '@angular/router';
import { provideAnimations } from '@angular/platform-browser/animations';
import { LoginComponent } from './login.component';
import { environment } from '../../../environments/environment';

describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;
  let http: HttpTestingController;
  let router: Router;

  beforeEach(async () => {
    sessionStorage.clear();

    await TestBed.configureTestingModule({
      imports: [LoginComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        provideAnimations(),
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    http = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router);
    fixture.detectChanges();
  });

  afterEach(() => {
    http.verify();
    sessionStorage.clear();
  });

  it('should create the component', () => {
    expect(component).toBeTruthy();
  });

  it('should start in login mode (not register)', () => {
    expect(component.registerMode()).toBeFalse();
  });

  it('should toggle to register mode and back', () => {
    component.toggleMode();
    expect(component.registerMode()).toBeTrue();

    component.toggleMode();
    expect(component.registerMode()).toBeFalse();
  });

  it('should not submit when form is invalid', () => {
    component.submit();
    expect(component.busy()).toBeFalse();
    http.expectNone(`${environment.authApiUrl}/login`);
  });

  it('should call login endpoint and navigate on success', () => {
    const navigateSpy = spyOn(router, 'navigateByUrl');
    component.form.patchValue({ email: 'user@test.com', password: 'secret123' });

    component.submit();
    expect(component.busy()).toBeTrue();

    const req = http.expectOne(`${environment.authApiUrl}/login`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ email: 'user@test.com', password: 'secret123' });
    req.flush({
      succeeded: true,
      data: {
        accountId: 1,
        email: 'user@test.com',
        displayName: 'Test',
        accessToken: 'token-abc',
        accessExpiresAtUtc: '2026-12-31T00:00:00Z',
        refreshToken: 'refresh-xyz',
        refreshExpiresAtUtc: '2026-12-31T00:00:00Z',
      },
      error: null,
    });

    expect(component.busy()).toBeFalse();
    expect(navigateSpy).toHaveBeenCalledWith('/');
  });

  it('should display error message when login fails', () => {
    component.form.patchValue({ email: 'user@test.com', password: 'wrong' });
    component.submit();

    const req = http.expectOne(`${environment.authApiUrl}/login`);
    req.flush({
      succeeded: false,
      data: null,
      error: { code: 'invalid_credentials', message: 'Credenciales inválidas.' },
    });

    expect(component.busy()).toBeFalse();
    expect(component.errorMessage()).toBe('Credenciales inválidas.');
  });

  it('should call register endpoint in register mode', () => {
    component.toggleMode();
    component.form.patchValue({
      displayName: 'New User',
      email: 'new@test.com',
      password: 'SecurePass123',
    });

    component.submit();

    const req = http.expectOne(`${environment.authApiUrl}/register`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({
      displayName: 'New User',
      email: 'new@test.com',
      password: 'SecurePass123',
    });
    req.flush({
      succeeded: true,
      data: {
        accountId: 2,
        email: 'new@test.com',
        displayName: 'New User',
        accessToken: 'token-new',
        accessExpiresAtUtc: '2026-12-31T00:00:00Z',
        refreshToken: 'refresh-new',
        refreshExpiresAtUtc: '2026-12-31T00:00:00Z',
      },
      error: null,
    });
  });

  it('should handle HTTP error responses gracefully', () => {
    component.form.patchValue({ email: 'user@test.com', password: 'pass' });
    component.submit();

    const req = http.expectOne(`${environment.authApiUrl}/login`);
    req.flush(
      { message: 'Error del servidor' },
      { status: 500, statusText: 'Internal Server Error' },
    );

    expect(component.busy()).toBeFalse();
    expect(component.errorMessage()).toBeTruthy();
  });
});
