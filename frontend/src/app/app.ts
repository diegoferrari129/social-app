import { Component, signal, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Navbar } from "./core/layouts/navbar/navbar";
import { SignalRService } from './core/signalr/signalr.service';
import { AuthService } from './core/auth/auth.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Navbar],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  protected readonly title = signal('frontend');
  constructor(
    private authService: AuthService,
    private signalR: SignalRService
  ) { }

  ngOnInit(): void {
    if (this.authService.isAuthenticated()) {
      this.signalR.startConnections();
    }
    this.authService.authState$.subscribe(isAuth => {
      if (isAuth) this.signalR.startConnections();
    });
  }
}
