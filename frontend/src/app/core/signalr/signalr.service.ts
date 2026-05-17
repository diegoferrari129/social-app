import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { AuthService } from '../auth/auth.service';

export interface ChatMessage {
  chatId: string;
  fromUserId: string;
  fromUserName: string;
  message: string;
  timestamp: Date;
}

export interface Notification {
  type: string;
  fromUserId: string;
  fromUserName: string;
  postId?: string;
  commentText?: string;
  timestamp: Date;
}

@Injectable({ providedIn: 'root' })
export class SignalRService {
  private chatHubConnection?: signalR.HubConnection;
  private notificationsHubConnection?: signalR.HubConnection;

  private messageSubject = new Subject<ChatMessage>();
  public message$ = this.messageSubject.asObservable();

  private notificationSubject = new Subject<Notification>();
  public notification$ = this.notificationSubject.asObservable();

  private chatStarted = false;
  private notifStarted = false;

  constructor(private authService: AuthService) { }

  startConnections(): void {
    if (this.chatStarted && this.notifStarted) return;
    const token = this.authService.getToken();
    if (!token) return;

    this.chatHubConnection = new signalR.HubConnectionBuilder()
      .withUrl('/chatHub', { accessTokenFactory: () => token, transport: signalR.HttpTransportType.LongPolling })
      .withAutomaticReconnect()
      .build();

    this.chatHubConnection.start()
      .then(() => console.log('ChatHub connected'))
      .catch(err => console.error('ChatHub connection error:', err));

    this.chatHubConnection.on('ReceiveMessage', (chatId: string, fromUserId: string, fromUserName: string, message: string, timestamp: string) => {
      this.messageSubject.next({ chatId, fromUserId, fromUserName, message, timestamp: new Date(timestamp) });
    });

    this.notificationsHubConnection = new signalR.HubConnectionBuilder()
      .withUrl('/notificationsHub', { accessTokenFactory: () => token })
      .withAutomaticReconnect()
      .build();

    this.notificationsHubConnection.start()
      .then(() => console.log('NotificationsHub connected'))
      .catch(err => console.error('NotificationsHub connection error:', err));

    this.notificationsHubConnection.on('NewNotification', (notification: Notification) => {
      this.notificationSubject.next(notification);
    });
  }

  async sendMessage(toUserId: string, message: string): Promise<void> {
    if (this.chatHubConnection?.state === signalR.HubConnectionState.Connected) {
      await this.chatHubConnection.invoke('SendMessage', toUserId, message);
    } else {
      console.error('ChatHub not connected');
    }
  }


}
