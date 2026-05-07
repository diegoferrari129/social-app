import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { Subscription } from 'rxjs';
import { PostService } from '../../post.service';
import { Post } from '../../post.model';
import { AuthService } from '../../../../core/auth/auth.service';

@Component({
  selector: 'app-user-post-list',
  imports: [CommonModule, RouterModule],
  templateUrl: './user-post-list.html',
  styleUrl: './user-post-list.css',
})
export class UserPostList implements OnInit, OnDestroy {
  posts: Post[] = [];
  loading = false;
  error = '';
  userId = '';
  private sub?: Subscription;
  private routeSub?: Subscription;

  constructor(
    private postService: PostService,
    private route: ActivatedRoute,
    private authService: AuthService
  ) { }

  ngOnInit(): void {
    this.routeSub = this.route.params.subscribe((params: any) => {
      this.userId = params['userId'];
      if (this.userId) this.loadPosts();
    });
  }

  loadPosts(): void {
    this.loading = true;
    this.sub = this.postService.getPostsByUserId(this.userId).subscribe({
      next: (data) => {
        this.posts = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load posts';
        this.loading = false;
        console.error(err);
      }
    });
  }

  get isOwnProfile(): boolean {
    const currentUserId = this.authService.getUserId();
    return currentUserId === this.userId;
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
    this.routeSub?.unsubscribe();
  }
}
