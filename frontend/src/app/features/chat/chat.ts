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
  isSending = false;
  private subs: Subscription[] = [];
  private lastReceivedMsgId = '';

  private chatService = inject(ChatService);
  private signalR = inject(SignalRService);
  private auth = inject(AuthService);
  private route = inject(ActivatedRoute);


  ngOnInit(): void {
    this.loadConversations();

    this.subs.push(this.signalR.message$.subscribe(msg => {
      const msgId = `${msg.fromUserId}_${msg.message}_${new Date(msg.timestamp).getTime()}`;
      if (this.lastReceivedMsgId === msgId) return; // già ricevuto
      this.lastReceivedMsgId = msgId;

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

    this.subs.push(this.route.params.subscribe(params => {
      const userId = params['userId'];
      if (userId) {
        setTimeout(() => {
          const existing = this.conversations.find(c => c.otherUserId === userId);
          if (existing) {
            this.selectConversation(existing);
          } else {
            this.openChatWithUser(userId);
          }
        }, 300);
      }
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
    if (this.selectedConversation?.id === conv.id) return;
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
    if (!this.newMessageText.trim() || !this.selectedConversation || this.isSending) return;
    this.isSending = true;
    this.signalR.sendMessage(this.selectedConversation.otherUserId, this.newMessageText)
      .catch(err => console.error(err))
      .finally(() => {
        this.isSending = false;
        this.newMessageText = '';
      });
  }

  get currentUserId(): string | null {
    return this.auth.getUserId();
  }

  openChatWithUser(userId: string): void {
    this.chatService.getOrCreateConversation(userId).subscribe({
      next: (chat) => {

        this.conversations = this.conversations.filter(c => c.id !== chat.id);
        this.conversations = [chat, ...this.conversations];
        this.selectConversation(chat);
      },
      error: (err) => console.error(err)
    });
  }


  ngOnDestroy(): void {
    this.subs.forEach(sub => sub.unsubscribe());
  }
}
