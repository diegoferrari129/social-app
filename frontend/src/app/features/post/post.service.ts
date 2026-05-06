import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Post } from './post.model';

@Injectable({
  providedIn: 'root',
})
export class PostService {
  private http = inject(HttpClient);
  private apiUrl = '/api';

  getFeed(page: number = 1, pageSize: number = 10): Observable<Post[]> {
    return this.http.get<Post[]>(`${this.apiUrl}/post/feed?page=${page}&pageSize=${pageSize}`);
  }
}


