import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../../auth/auth.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-navbar',
  imports: [CommonModule, RouterModule],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css',
})
export class Navbar implements OnInit, OnDestroy {
  isAuthenticated = false;
  userName = '';
  menuOpen = false;
  currentUserId: string | null = null;
  private authSubscription?: Subscription;

  constructor(private authService: AuthService, private router: Router) { }

  ngOnInit(): void {
    this.isAuthenticated = this.authService.isAuthenticated();
    if (this.isAuthenticated) {
      this.loadUserInfo();
    }

    this.authSubscription = this.authService.authState$.subscribe(isAuth => {
      this.isAuthenticated = isAuth;
      if (isAuth) this.loadUserInfo();
    });
  }

  loadUserInfo(): void {
    this.userName = this.authService.getUserName() || 'Utente';
    this.currentUserId = this.authService.getUserId();
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  toggleMenu() {
    this.menuOpen = !this.menuOpen;
  }

  ngOnDestroy(): void {
    this.authSubscription?.unsubscribe();
  }
}
