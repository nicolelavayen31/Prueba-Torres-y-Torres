import { Routes } from '@angular/router';
import { authGuard } from './core/auth.guard';
import { LoginComponent } from './features/auth/login.component';

export const routes: Routes = [
	{ path: 'login', component: LoginComponent },
	{
		path: '',
		canActivate: [authGuard],
		loadComponent: () => import('./features/workspace/workspace.component').then((module) => module.WorkspaceComponent),
	},
	{ path: '**', redirectTo: '' },
];
