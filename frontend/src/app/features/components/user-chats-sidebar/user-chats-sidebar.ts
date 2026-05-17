import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ChatService, ChatPreview } from '../../chat/chat.service';
import { AuthService } from '../../../core/auth/auth.service';
import { Subscription } from 'rxjs';
import { SignalRService } from '../../../core/signalr/signalr.service';

@Component({
  selector: 'app-user-chats-sidebar',
  imports: [CommonModule, RouterModule],
  templateUrl: './user-chats-sidebar.html',
  styleUrls: ['./user-chats-sidebar.css']
})
export class UserChatsSidebarComponent implements OnInit {
  private chatService = inject(ChatService);
  private auth = inject(AuthService);
  private signalR = inject(SignalRService);
  conversations: ChatPreview[] = [];
  loading = false;
  private messageSubscription?: Subscription;

  ngOnInit(): void {
    this.loadConversations();
    this.messageSubscription = this.signalR.message$.subscribe(msg => {
      const currentUserId = this.auth.getUserId();
      if (msg.fromUserId !== currentUserId) {
        const conversation = this.conversations.find(c => c.otherUserId === msg.fromUserId);
        if (conversation) {
          conversation.unreadCount = (conversation.unreadCount || 0) + 1;
          conversation.lastMessage = msg.message;
          conversation.lastMessageTime = msg.timestamp;
          this.conversations = [...this.conversations];
        } else {
          this.loadConversations();
        }
      }
    });
  }

  loadConversations(): void {
    this.loading = true;
    this.chatService.getConversations().subscribe({
      next: (data) => {
        this.conversations = data;
        this.loading = false;
      },
      error: (err) => {
        console.error(err);
        this.loading = false;
      }
    });
  }

  ngOnDestroy(): void {
    this.messageSubscription?.unsubscribe();
  }
}
