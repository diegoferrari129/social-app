import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { PostService } from '../../post.service';
import { AuthService } from '../../../../core/auth/auth.service';
import { Post } from '../../post.model';
@Component({
  selector: 'app-post-edit',
  imports: [CommonModule, FormsModule],
  templateUrl: './post-edit.html',
  styleUrl: './post-edit.css',
})
export class PostEdit implements OnInit, OnDestroy {
  post: Post | null = null;
  loading = false;
  saving = false;
  error = '';
  content = '';
  postImg = '';

  private sub?: Subscription;
  private routeSub?: Subscription;

  constructor(
    private postService: PostService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.routeSub = this.route.params.subscribe((params: any) => {
      const id = params['id'];
      if (id) this.loadPost(id);
    });
  }

  loadPost(id: string): void {
    this.loading = true;
    this.sub = this.postService.getPostById(id).subscribe({
      next: (data) => {
        this.post = data;
        const currentUserId = this.authService.getUserId();
        if (currentUserId !== this.post.userId) {
          this.error = 'You are not the owner of this post';
          this.router.navigate(['/feed']);
          return;
        }
        this.content = data.content;
        this.postImg = data.postImg || '';
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load post';
        this.loading = false;
        console.error(err);
      }
    });
  }

  onSubmit(): void {
    if (!this.post) return;
    this.saving = true;
    this.postService.updatePost(this.post.id, {
      content: this.content,
      postImg: this.postImg || undefined
    }).subscribe({
      next: () => {
        this.router.navigate(['/post', this.post!.id]);
      },
      error: (err) => {
        this.error = 'Update failed';
        this.saving = false;
        console.error(err);
      }
    });
  }

  deletePost(): void {
    if (!this.post) return;
    const confirmDelete = confirm('Are you sure you want to delete this post?');
    if (!confirmDelete) return;
    this.saving = true;
    this.postService.deletePost(this.post.id).subscribe({
      next: () => {
        this.router.navigate(['/feed']);
      },
      error: (err) => {
        this.error = 'Delete failed';
        this.saving = false;
        console.error(err);
      }
    });
  }

  cancel(): void {
    if (this.post) {
      this.router.navigate(['/post', this.post.id]);
    } else {
      this.router.navigate(['/feed']);
    }
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
    this.routeSub?.unsubscribe();
  }
}
