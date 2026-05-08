import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UserService } from '../../../features/user/user.service';
import { AuthService } from '../../../core/auth/auth.service';


@Component({
  selector: 'app-follow-button',
  imports: [CommonModule],
  template: `
    <button
      (click)="toggleFollow()"
      [disabled]="isLoading"
      class="px-3 py-1 rounded text-sm font-medium transition"
      [ngClass]="{
        'bg-blue-500 text-white hover:bg-blue-600': !isFollowing && !isLoading,
        'bg-gray-300 text-gray-700 hover:bg-gray-400': isFollowing && !isLoading,
        'bg-gray-400 text-gray-600 cursor-not-allowed': isLoading
      }">
      {{ buttonText }}
    </button>
  `,
  styleUrl: './follow-button.css',
})
export class FollowButton {
  @Input() targetUserId!: string;
  @Input() isFollowing = false;
  @Output() followChanged = new EventEmitter<boolean>();

  isLoading = false;

  constructor(
    private userService: UserService,
    private authService: AuthService
  ) { }

  get buttonText(): string {
    if (this.isLoading) return '...';
    return this.isFollowing ? 'Unfollow' : 'Follow';
  }

  toggleFollow(): void {
    const currentUserId = this.authService.getUserId();
    if (!currentUserId || currentUserId === this.targetUserId) return;

    this.isLoading = true;
    this.userService.followUser(this.targetUserId).subscribe({
      next: () => {
        this.isFollowing = !this.isFollowing;
        this.followChanged.emit(this.isFollowing);
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Follow error', err);
        this.isLoading = false;
      }
    });
  }
}
