import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Subscription } from 'rxjs';
import { NotificationService, Notification as ApiNotification } from '../../../core/signalr/notification.service';
import { SignalRService, Notification as SignalRNotification } from '../../../core/signalr/signalr.service';

@Component({
  selector: 'app-notification-dropdown',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './notification-dropdown.html',
  styleUrls: ['./notification-dropdown.css']
})
export class NotificationDropdownComponent implements OnInit, OnDestroy {
  notifications: ApiNotification[] = [];
  unreadCount = 0;
  isOpen = false;
  loading = false;
  private subscription?: Subscription;

  private notificationService = inject(NotificationService);
  private signalR = inject(SignalRService);

  ngOnInit(): void {
    this.loadNotifications();

    this.subscription = this.signalR.notification$.subscribe((signalRNotif: SignalRNotification) => {
      this.unreadCount++;
      const newNotif: ApiNotification = {
        id: Date.now().toString(),
        userId: '',
        type: signalRNotif.type,
        fromUserId: signalRNotif.fromUserId,
        fromUserName: signalRNotif.fromUserName,
        postId: signalRNotif.postId,
        messageText: signalRNotif.commentText,
        isRead: false,
        createdAt: signalRNotif.timestamp
      };
      this.notifications.unshift(newNotif);
      if (this.notifications.length > 20) this.notifications.pop();
    });
  }

  loadNotifications(): void {
    this.loading = true;
    this.notificationService.getNotifications(1, 10).subscribe({
      next: (data) => {
        this.notifications = data;
        this.unreadCount = data.filter(n => !n.isRead).length;
        this.loading = false;
      },
      error: (err) => {
        console.error(err);
        this.loading = false;
      }
    });
  }

  toggleDropdown(): void {
    this.isOpen = !this.isOpen;
    if (this.isOpen) {
      this.markAllAsRead();
    }
  }

  markAllAsRead(): void {
    if (this.unreadCount === 0) return;
    this.notificationService.markAllAsRead().subscribe({
      next: () => {
        this.unreadCount = 0;
        this.notifications.forEach(n => n.isRead = true);
      },
      error: (err) => console.error(err)
    });
  }

  markAsRead(notificationId: string): void {
    this.notificationService.markAsRead(notificationId).subscribe({
      next: () => {
        const notif = this.notifications.find(n => n.id === notificationId);
        if (notif) notif.isRead = true;
        this.unreadCount = this.notifications.filter(n => !n.isRead).length;
      },
      error: (err) => console.error(err)
    });
  }

  getNotificationIcon(type: string): string {
    switch (type) {
      case 'follow': return 'follow';
      case 'like': return 'likes';
      case 'comment': return 'comments';
      case 'message': return 'messages';
      default: return 'notifications';
    }
  }

  getNotificationText(notif: ApiNotification): string {
    switch (notif.type) {
      case 'follow': return `${notif.fromUserName} started following you`;
      case 'like': return `${notif.fromUserName} liked your post`;
      case 'comment': return `${notif.fromUserName} commented: "${notif.messageText}"`;
      case 'message': return `${notif.fromUserName} sent you a message: "${notif.messageText}"`;
      default: return `${notif.fromUserName} did something`;
    }
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
  }
}
