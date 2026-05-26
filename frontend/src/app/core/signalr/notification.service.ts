import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Notification {
  id: string;
  userId: string;
  type: string;
  fromUserId: string;
  fromUserName: string;
  postId?: string;
  messageText?: string;
  isRead: boolean;
  createdAt: Date;
}

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private apiUrl = '/api';

  constructor(private http: HttpClient) { }

  getNotifications(page: number = 1, pageSize: number = 10): Observable<Notification[]> {
    return this.http.get<Notification[]>(`${this.apiUrl}/notification?page=${page}&pageSize=${pageSize}`);
  }

  markAsRead(notificationId: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/notification/mark-read/${notificationId}`, {});
  }

  markAllAsRead(): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/notification/mark-all-read`, {});
  }
}
