import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ChatService, ChatPreview } from '../../chat/chat.service';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'app-user-chats-sidebar',
  imports: [CommonModule, RouterModule],
  templateUrl: './user-chats-sidebar.html',
  styleUrls: ['./user-chats-sidebar.css']
})
export class UserChatsSidebarComponent implements OnInit {
  private chatService = inject(ChatService);
  private auth = inject(AuthService);
  conversations: ChatPreview[] = [];
  loading = false;

  ngOnInit(): void {
    this.loadConversations();
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
}
