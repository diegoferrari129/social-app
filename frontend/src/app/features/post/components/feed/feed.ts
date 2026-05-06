import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { PostService } from '../../post.service';
import { Post } from '../../post.model';
@Component({
  selector: 'app-feed',
  imports: [CommonModule],
  templateUrl: './feed.html',
  styleUrl: './feed.css',
})
export class Feed {
  private postService = inject(PostService);

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
        console.error('Errore nel feed:', err);
        this.error.set('Errore nel caricamento del feed');
        this.loading.set(false);
      }
    });
  }

  ngOnDestroy() {
    this.feedSubscription?.unsubscribe();
  }
}
