import { Routes } from '@angular/router';
import { Home } from './features/home/home';
import { Login } from './core/auth/pages/login/login';
import { Feed } from './features/post/components/feed/feed';
import { CreatePostForm } from './features/post/components/create-post-form/create-post-form';
import { PostDetail } from './features/post/components/post-detail/post-detail';
import { PostEdit } from './features/post/components/post-edit/post-edit';
import { UserPostList } from './features/post/components/user-post-list/user-post-list';
import { Profile } from './features/user/components/profile/profile';
import { Register } from './core/auth/pages/register/register';
import { Chat } from './features/chat/chat';

export const routes: Routes = [
  { path: '', redirectTo: '/home', pathMatch: 'full' },
  { path: 'home', component: Home },
  { path: 'login', component: Login },
  { path: 'feed', component: Feed },
  { path: 'create-post', component: CreatePostForm },
  { path: 'post/:id', component: PostDetail },
  { path: 'post/edit/:id', component: PostEdit },
  { path: 'user/:userId/posts', component: UserPostList },
  { path: 'profile/:id', component: Profile },
  { path: 'register', component: Register },
  { path: 'chat', component: Chat },
  { path: 'chat/:userId', component: Chat }
];
