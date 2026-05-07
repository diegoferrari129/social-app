import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { PostService } from '../../post.service';
import { Post, PostComment } from '../../post.model';

@Component({
  selector: 'app-post-detail',
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './post-detail.html',
  styleUrl: './post-detail.css',
})
export class PostDetail implements OnInit, OnDestroy {
  private postService = inject(PostService);
  private route = inject(ActivatedRoute);

  post: Post | null = null;
  loading = false;
  error = '';
  commentText = '';

  private subscription?: Subscription;
  private routeSub?: Subscription;

  ngOnInit(): void {
    this.routeSub = this.route.params.subscribe((params: any) => {
      const id = params['id'];
      if (id) this.loadPost(id);
    });
  }

  loadPost(id: string): void {
    this.loading = true;
    this.subscription = this.postService.getPostById(id).subscribe({
      next: (data: Post) => {
        this.post = data;
        this.loading = false;
      },
      error: (err: any) => {
        this.error = 'Failed to load post';
        this.loading = false;
        console.error(err);
      }
    });
  }

  onLike(postId: string): void {
    this.postService.toggleLike(postId).subscribe({
      next: () => {
        if (this.post) this.loadPost(this.post.id);
      },
      error: (err: any) => console.error(err)
    });
  }

  addComment(): void {
    if (!this.commentText.trim() || !this.post) return;

    this.postService.addComment(this.post.id, this.commentText).subscribe({
      next: (newComment: PostComment) => {
        this.post!.comments.push(newComment);
        this.commentText = '';
      },
      error: (err: any) => console.error(err)
    });
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
    this.routeSub?.unsubscribe();
  }
}
