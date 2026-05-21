import { Component, Input, Output, EventEmitter, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { ChatService, ChatPreview, Message } from './chat.service';
import { SignalRService } from '../../core/signalr/signalr.service';
import { AuthService } from '../../core/auth/auth.service';
import { ViewChild, ElementRef } from '@angular/core';

@Component({
  selector: 'app-chat',
  imports: [CommonModule, FormsModule],
  templateUrl: './chat.html',
  styleUrls: ['./chat.css']
})
export class Chat implements OnInit, OnDestroy {
  @Input() targetUserId!: string;
  @Output() close = new EventEmitter<void>();
  @ViewChild('scrollAnchor') scrollAnchor!: ElementRef;

  conversation: ChatPreview | null = null;
  messages: Message[] = [];
  newMessageText = '';
  loading = false;
  isSending = false;
  private subs: Subscription[] = [];
  private lastReceivedMsgId = '';

  private chatService = inject(ChatService);
  private signalR = inject(SignalRService);
  private auth = inject(AuthService);

  ngOnInit(): void {
    this.loadConversation();
    this.subs.push(this.signalR.message$.subscribe(msg => {
      if (msg.fromUserId === this.auth.getUserId()) return;
      const msgId = `${msg.fromUserId}_${msg.message}_${new Date(msg.timestamp).getTime()}`;
      if (this.lastReceivedMsgId === msgId) return;
      this.lastReceivedMsgId = msgId;
      if (this.conversation && msg.chatId === this.conversation.id) {
        this.messages.push({
          id: '',
          conversationId: msg.chatId,
          senderId: msg.fromUserId,
          text: msg.message,
          sentAt: msg.timestamp,
          isRead: false
        });
        this.scrollToBottom();
        this.chatService.markMessagesAsRead(this.conversation.id).subscribe(() => {
          this.chatService.notifyUnreadCountChanged();
        });
      }
    }));
  }

  loadConversation(): void {
    this.loading = true;
    this.chatService.getOrCreateConversation(this.targetUserId).subscribe({
      next: (conv) => {
        this.conversation = conv;
        this.loadMessages(conv.id);
        this.chatService.markMessagesAsRead(conv.id).subscribe(() => {
          this.chatService.notifyUnreadCountChanged();
        });
        this.loading = false;
      },
      error: (err) => {
        console.error(err);
        this.loading = false;
      }
    });
  }

  loadMessages(convId: string): void {
    this.chatService.getMessages(convId).subscribe({
      next: (msgs) => {
        this.messages = msgs;
        setTimeout(() => this.scrollToBottom(), 0);
      },
      error: (err) => console.error(err)
    });
  }

  private scrollToBottom(): void {
    setTimeout(() => {
      this.scrollAnchor?.nativeElement.scrollIntoView({ behavior: 'smooth' });
    }, 100);
  }

  sendMessage(): void {
    if (!this.newMessageText.trim() || !this.conversation || this.isSending) return;
    const tempMessage: Message = {
      id: 'temp' + Date.now(),
      conversationId: this.conversation.id,
      senderId: this.auth.getUserId()!,
      text: this.newMessageText,
      sentAt: new Date(),
      isRead: false
    };
    this.messages.push(tempMessage);
    setTimeout(() => this.scrollToBottom(), 0);

    this.isSending = true;
    this.signalR.sendMessage(this.targetUserId, this.newMessageText)
      .catch(err => console.error(err))
      .finally(() => {
        this.isSending = false;
        this.newMessageText = '';
      });
  }

  get currentUserId(): string | null {
    return this.auth.getUserId();
  }

  closePopup(): void {
    this.close.emit();
  }

  ngOnDestroy(): void {
    this.subs.forEach(sub => sub.unsubscribe());
  }
}
