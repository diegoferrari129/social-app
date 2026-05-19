import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { PostService } from '../../post.service';

@Component({
  selector: 'app-post',
  imports: [CommonModule, FormsModule],
  templateUrl: './create-post-form.html',
  styleUrl: './create-post-form.css',
})
export class CreatePostForm {
  private postService = inject(PostService);
  private router = inject(Router);

  content = '';
  postImg = '';
  errorMessage = '';
  isSubmitting = false;

  onSubmit() {
    if (!this.content) {
      this.errorMessage = 'Title and content cannot be empty';
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = '';

    this.postService.createPost({
      content: this.content,
      postImg: this.postImg || undefined
    }).subscribe({
      next: () => {
        this.router.navigate(['/feed']);
      },
      error: (err) => {
        this.errorMessage = 'error occured in your post creation';
        this.isSubmitting = false;
        console.error(err);
      }
    });
  }
}
