import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { PostService } from '../../post.service';
import { Post } from '../../post.model';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../../../core/auth/auth.service';

@Component({
  selector: 'app-feed',
  imports: [CommonModule, RouterModule],
  templateUrl: './feed.html',
  styleUrl: './feed.css',
})
export class Feed implements OnInit, OnDestroy {
  private postService = inject(PostService);
  private authService = inject(AuthService);

  posts = signal<Post[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);

  currentUserId = this.authService.getUserId();

  private feedSubscription?: Subscription;

  commentsState = new Map<string, { showing: boolean; visibleCount: number }>();

  ngOnInit() {
    this.loadFeed();
  }

  loadFeed() {
    this.loading.set(true);
    this.error.set(null);

    this.feedSubscription = this.postService.getFeed().subscribe({
      next: (data) => {
        this.posts.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('error feed:', err);
        this.error.set('error in loading the feed');
        this.loading.set(false);
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

  async onLike(postId: string) {
    const userId = this.authService.getUserId();
    if (!userId) return;
    this.posts.update(posts =>
      posts.map(post =>
        post.id === postId
          ? {
            ...post,
            likes: post.likes.includes(userId)
              ? post.likes.filter(id => id !== userId)
              : [...post.likes, userId]
          }
          : post
      )
    );
    this.postService.toggleLike(postId).subscribe({
      error: (err) => {
        console.error('Errore nel like', err);
        this.loadFeed();
      }
    });
  }

  ngOnDestroy() {
    this.feedSubscription?.unsubscribe();
  }
}
