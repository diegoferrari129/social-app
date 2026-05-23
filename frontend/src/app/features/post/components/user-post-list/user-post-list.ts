import { Component, OnInit, OnDestroy, Input } from '@angular/core';
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
  @Input() userId!: string;
  posts: Post[] = [];
  loading = false;
  error = '';
  commentsState = new Map<string, { showing: boolean; visibleCount: number }>();
  private sub?: Subscription;
  private routeSub?: Subscription;

  constructor(
    private postService: PostService,
    private route: ActivatedRoute,
    private authService: AuthService
  ) { }

  ngOnInit(): void {
    if (this.userId) {
      this.loadPosts();
    }
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

  getCommentsState(postId: string) {
    return this.commentsState.get(postId);
  }

  toggleComments(postId: string): void {
    if (this.commentsState.has(postId)) {
      const state = this.commentsState.get(postId)!;
      state.showing = !state.showing;
      this.commentsState.set(postId, state);
    } else {
      this.commentsState.set(postId, { showing: true, visibleCount: 10 });
    }
  }

  loadMoreComments(postId: string): void {
    const state = this.commentsState.get(postId);
    if (state) {
      state.visibleCount += 10;
      this.commentsState.set(postId, state);
    }
  }

  onLike(postId: string): void {
    this.postService.toggleLike(postId).subscribe({
      next: () => {
        // Aggiorna localmente il contatore dei like
        const post = this.posts.find(p => p.id === postId);
        if (post) {
          const userId = this.authService.getUserId();
          if (userId) {
            if (post.likes.includes(userId)) {
              post.likes = post.likes.filter(id => id !== userId);
            } else {
              post.likes.push(userId);
            }
          }
        }
      },
      error: (err) => console.error(err)
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
