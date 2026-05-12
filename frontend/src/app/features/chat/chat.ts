import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { ChatService, ChatPreview, Message } from '../chat/chat.service';
import { SignalRService } from '../../core/signalr/signalr.service';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-chat',
  imports: [CommonModule, FormsModule],
  templateUrl: './chat.html',
  styleUrl: './chat.css',
})
export class Chat implements OnInit, OnDestroy {
  conversations: ChatPreview[] = [];
  selectedConversation: ChatPreview | null = null;
  messages: Message[] = [];
  newMessageText = '';
  loading = false;
  private subs: Subscription[] = [];

  private chatService = inject(ChatService);
  private signalR = inject(SignalRService);
  private auth = inject(AuthService);


  ngOnInit(): void {
    this.loadConversations();
    this.subs.push(this.signalR.message$.subscribe(msg => {
      if (this.selectedConversation && msg.chatId === this.selectedConversation.id) {
        this.messages.push({
          id: '',
          conversationId: msg.chatId,
          senderId: msg.fromUserId,
          text: msg.message,
          sentAt: msg.timestamp,
          isRead: false
        });
      }
      this.loadConversations();
    }));
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

  selectConversation(conv: ChatPreview): void {
    this.selectedConversation = conv;
    this.loadMessages(conv.id);
  }

  loadMessages(conversationId: string): void {
    this.chatService.getMessages(conversationId).subscribe({
      next: (msgs) => this.messages = msgs,
      error: (err) => console.error(err)
    });
  }

  sendMessage(): void {
    if (!this.newMessageText.trim() || !this.selectedConversation) return;
    this.signalR.sendMessage(this.selectedConversation.otherUserId, this.newMessageText)
      .catch(err => console.error(err));
    this.newMessageText = '';
  }

  get currentUserId(): string | null {
    return this.auth.getUserId();
  }

  ngOnDestroy(): void {
    this.subs.forEach(sub => sub.unsubscribe());
  }
}
