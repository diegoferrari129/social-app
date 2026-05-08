import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Post, PostComment } from './post.model';

@Injectable({
  providedIn: 'root',
})
export class PostService {
  private http = inject(HttpClient);
  private apiUrl = '/api';

  getFeed(page: number = 1, pageSize: number = 10): Observable<Post[]> {
    return this.http.get<Post[]>(`${this.apiUrl}/post/feed?page=${page}&pageSize=${pageSize}`);
  }

  createPost(postData: { title: string; content: string; postImg?: string }): Observable<Post> {
    return this.http.post<Post>(`${this.apiUrl}/post/create`, postData);
  }

  getPostById(id: string): Observable<Post> {
    return this.http.get<Post>(`${this.apiUrl}/post/${id}`);
  }

  getPostsByUserId(userId: string, page = 1, pageSize = 10): Observable<Post[]> {
    return this.http.get<Post[]>(`${this.apiUrl}/post/user/${userId}?page=${page}&pageSize=${pageSize}`);
  }

  toggleLike(postId: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/post/${postId}/like`, {});
  }

  addComment(postId: string, text: string): Observable<PostComment> {
    return this.http.post<PostComment>(`${this.apiUrl}/post/${postId}/comment`, { text });
  }

  updatePost(id: string, postData: { title: string; content: string; postImg?: string }): Observable<Post> {
    return this.http.put<Post>(`${this.apiUrl}/post/${id}`, postData);
  }

  deletePost(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/post/${id}`);
  }

  deleteComment(postId: string, commentId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/post/${postId}/comment/${commentId}`);
  }
}


