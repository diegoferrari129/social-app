export interface Comment {
  id: string;
  userId: string;
  userName: string;
  text: string;
  createdAt: string;
}

export interface Post {
  id: string;
  title: string;
  content: string;
  postImg?: string;
  userId: string;
  userName: string;
  likes: string[];
  comments: Comment[];
  createdAt: string;
}
