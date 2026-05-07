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

  private feedSubscription?: Subscription;

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
