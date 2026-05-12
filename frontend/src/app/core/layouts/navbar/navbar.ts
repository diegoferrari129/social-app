import { Component, OnInit, OnDestroy, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../../auth/auth.service';
import { Subscription } from 'rxjs';
import { SignalRService } from '../../signalr/signalr.service';

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
  unreadCount = 0;

  private authSubscription?: Subscription;
  private notificationSub?: Subscription;

  private authService = inject(AuthService);
  private router = inject(Router);
  private signalR = inject(SignalRService);

  ngOnInit(): void {
    this.isAuthenticated = this.authService.isAuthenticated();
    if (this.isAuthenticated) {
      this.loadUserInfo();
    }

    this.authSubscription = this.authService.authState$.subscribe(isAuth => {
      this.isAuthenticated = isAuth;
      if (isAuth) this.loadUserInfo();
    });

    this.notificationSub = this.signalR.notification$.subscribe(notification => {
      this.unreadCount++;
      console.log(`Nuova notifica: ${notification.fromUserName} ${notification.type}`);
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
    this.notificationSub?.unsubscribe();
  }
}
