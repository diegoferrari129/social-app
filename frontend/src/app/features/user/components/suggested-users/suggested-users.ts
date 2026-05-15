import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { UserService, SuggestedUser } from '../../user.service';
import { AuthService } from '../../../../core/auth/auth.service';
import { FollowButton } from '../../../../shared/components/follow-button/follow-button';

@Component({
  selector: 'app-suggested-users',
  imports: [CommonModule, RouterModule, FollowButton],
  templateUrl: './suggested-users.html',
  styleUrls: ['./suggested-users.css']
})
export class SuggestedUsers implements OnInit {
  private userService = inject(UserService);
  private authService = inject(AuthService);
  suggestedUsers: SuggestedUser[] = [];
  loading = false;

  ngOnInit(): void {
    this.loadSuggestedUsers();
  }

  loadSuggestedUsers(): void {
    this.loading = true;
    this.userService.getSuggestedUsers(5).subscribe({
      next: (data) => {
        this.suggestedUsers = data;
        this.loading = false;
      },
      error: (err) => {
        console.error(err);
        this.loading = false;
      }
    });
  }
}
