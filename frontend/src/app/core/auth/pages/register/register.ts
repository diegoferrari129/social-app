import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../../auth.service';

@Component({
  selector: 'app-register',
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register {
  firstName = '';
  lastName = '';
  email = '';
  password = '';
  errorMessage = '';

  constructor(private authService: AuthService, private router: Router) { }

  onSubmit(): void {
    this.authService.register(this.firstName, this.lastName, this.email, this.password).subscribe({
      next: (res) => {
        this.authService.saveToken(res.token);
        localStorage.setItem('userId', res.user.id);
        localStorage.setItem('userName', res.user.name);
        this.authService.updateAuthState(true);
        this.router.navigate(['/feed']);
      },
      error: (err) => {
        this.errorMessage = err.error?.message || 'Error during registration';
        console.error(err);
      }
    });
  }

}
