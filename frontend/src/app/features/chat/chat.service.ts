import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface ChatPreview {
  id: string;
  otherUserId: string;
  otherUserName: string;
  lastMessage: string;
  lastMessageTime: Date;
}

export interface Message {
  id: string;
  conversationId: string;
  senderId: string;
  text: string;
  sentAt: Date;
  isRead: boolean;
}

@Injectable({ providedIn: 'root' })
export class ChatService {
  private apiUrl = '/api';

  constructor(private http: HttpClient) { }

  getConversations(): Observable<ChatPreview[]> {
    return this.http.get<ChatPreview[]>(`${this.apiUrl}/chat/conversations`);
  }

  getMessages(conversationId: string): Observable<Message[]> {
    return this.http.get<Message[]>(`${this.apiUrl}/chat/messages/${conversationId}`);
  }

  markMessagesAsRead(conversationId: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/chat/messages/mark-read/${conversationId}`, {});
  }

  getOrCreateConversation(otherUserId: string): Observable<ChatPreview> {
    return this.http.post<ChatPreview>(`${this.apiUrl}/chat/start/${otherUserId}`, {});
  }
}
