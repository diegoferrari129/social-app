import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { UserService, UserProfile } from '../../user.service';
import { AuthService } from '../../../../core/auth/auth.service';
import { FollowButton } from '../../../../shared/components/follow-button/follow-button';
import { UserPostList } from '../../../post/components/user-post-list/user-post-list';
import { SuggestedUsers } from '../suggested-users/suggested-users';


@Component({
  selector: 'app-profile',
  imports: [CommonModule, FormsModule, RouterModule, FollowButton, UserPostList, SuggestedUsers],
  templateUrl: './profile.html',
  styleUrl: './profile.css',
})
export class Profile implements OnInit, OnDestroy {
  user: UserProfile | null = null;
  loading = false;
  error = '';
  editMode = false;
  editForm = { name: '', bio: '', imgUrl: '' };
  saving = false;
  isFollowing = false;

  private sub?: Subscription;
  private routeSub?: Subscription;

  constructor(
    private userService: UserService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.routeSub = this.route.params.subscribe(params => {
      const userId = params['id'];
      if (userId) {
        this.loadProfile(userId);
      } else {
        this.router.navigate(['/login']);
      }
    });
  }

  loadProfile(userId: string): void {
    this.loading = true;
    this.sub = this.userService.getUserProfile(userId).subscribe({
      next: (data) => {
        this.user = data;
        this.isFollowing = data.isFollowed || false;
        this.editForm = {
          name: data.name,
          bio: data.bio,
          imgUrl: data.imgUrl
        };
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load profile';
        this.loading = false;
        console.error(err);
      }
    });
  }

  onFollowChanged(newState: boolean): void {
    this.isFollowing = newState;
    if (this.user) {
      this.user.followersCount += newState ? 1 : -1;
    }
  }

  get isOwnProfile(): boolean {
    if (!this.user) return false;
    const currentUserId = this.authService.getUserId();
    return this.user.id === currentUserId;
  }

  toggleEdit(): void {
    this.editMode = !this.editMode;
    if (this.editMode && this.user) {
      this.editForm = {
        name: this.user.name,
        bio: this.user.bio,
        imgUrl: this.user.imgUrl
      };
    }
  }

  saveProfile(): void {
    if (!this.user) return;
    this.saving = true;
    this.userService.updateUserProfile(this.user.id, this.editForm).subscribe({
      next: () => {

        if (this.user) {
          this.user.name = this.editForm.name;
          this.user.bio = this.editForm.bio;
          this.user.imgUrl = this.editForm.imgUrl;
        }
        this.editMode = false;
        this.saving = false;
      },
      error: (err) => {
        console.error(err);
        alert('Failed to update profile');
        this.saving = false;
      }
    });
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
    this.routeSub?.unsubscribe();
  }
}
