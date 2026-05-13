import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { ChatService, ChatPreview, Message } from '../chat/chat.service';
import { SignalRService } from '../../core/signalr/signalr.service';
import { AuthService } from '../../core/auth/auth.service';
import { ActivatedRoute, RouterModule } from '@angular/router';

@Component({
  selector: 'app-chat',
  imports: [CommonModule, FormsModule, RouterModule],
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
  private route = inject(ActivatedRoute);


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

      this.route.params.subscribe(params => {
        const userId = params['userId'];
        if (userId) {
          this.openChatWithUser(userId);
        }
      });
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
    this.chatService.markMessagesAsRead(conv.id).subscribe();
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

  openChatWithUser(userId: string): void {
    this.chatService.getOrCreateConversation(userId).subscribe({
      next: (chat) => {
        const exists = this.conversations.some(c => c.id === chat.id);
        if (!exists) {
          this.conversations = [chat, ...this.conversations];
        }
        this.selectConversation(chat);
      },
      error: (err) => console.error(err)
    });
  }


  ngOnDestroy(): void {
    this.subs.forEach(sub => sub.unsubscribe());
  }
}
