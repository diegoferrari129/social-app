import { Routes } from '@angular/router';
import { Home } from './features/home/home';
import { Login } from './core/auth/pages/login/login';
import { Feed } from './features/post/components/feed/feed';
import { CreatePostForm } from './features/post/components/post/create-post-form';

export const routes: Routes = [
  { path: '', redirectTo: '/home', pathMatch: 'full' },
  { path: 'home', component: Home },
  { path: 'login', component: Login },
  { path: 'feed', component: Feed },
  { path: 'create-post', component: CreatePostForm }
];
