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
