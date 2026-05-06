import { Routes } from '@angular/router';
import { Home } from './features/home/home';
import { Login } from './core/auth/pages/login/login';

export const routes: Routes = [
  { path: '', redirectTo: '/home', pathMatch: 'full' },
  { path: 'home', component: Home },
  { path: 'login', component: Login }
];
