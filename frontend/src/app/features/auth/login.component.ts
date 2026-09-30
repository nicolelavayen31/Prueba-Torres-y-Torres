import { HttpErrorResponse } from '@angular/common/http';
import { Component, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, MatButtonModule, MatFormFieldModule, MatIconModule, MatInputModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
})
export class LoginComponent {
  readonly busy = signal(false);
  readonly errorMessage = signal('');
  readonly registerMode = signal(false);

  readonly form = this.formBuilder.nonNullable.group({
    displayName: ['', [Validators.maxLength(80)]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.maxLength(128)]],
  });

  constructor(
    private readonly formBuilder: FormBuilder,
    private readonly auth: AuthService,
    private readonly router: Router,
  ) {}

  submit(): void {
    if (this.form.invalid || this.busy()) {
      this.form.markAllAsTouched();
      return;
    }

    this.busy.set(true);
    this.errorMessage.set('');
    const { displayName, email, password } = this.form.getRawValue();
    const request = this.registerMode()
      ? this.auth.register(displayName, email, password)
      : this.auth.login(email, password);

    request.subscribe({
      next: (response) => {
        this.busy.set(false);
        if (!response.succeeded) {
          this.errorMessage.set(response.error?.message ?? 'No se pudo iniciar sesión.');
          return;
        }
        void this.router.navigateByUrl('/');
      },
      error: (error: HttpErrorResponse) => {
        this.busy.set(false);
        this.errorMessage.set(
          error.error?.error?.message ?? error.error?.message ?? 'No fue posible conectar con AuthService.',
        );
      },
    });
  }

  toggleMode(): void {
    this.registerMode.update((enabled) => !enabled);
    const displayName = this.form.controls.displayName;
    const password = this.form.controls.password;

    displayName.setValidators(this.registerMode()
      ? [Validators.required, Validators.maxLength(80)]
      : [Validators.maxLength(80)]);
    password.setValidators(this.registerMode()
      ? [Validators.required, Validators.minLength(12), Validators.maxLength(128), Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$/)]
      : [Validators.required, Validators.maxLength(128)]);
    displayName.updateValueAndValidity();
    password.updateValueAndValidity();
    this.errorMessage.set('');
  }
}