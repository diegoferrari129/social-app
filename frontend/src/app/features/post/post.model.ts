export interface PostComment {
  id: string;
  userId: string;
  userName: string;
  text: string;
  createdAt: string;
}

export interface Post {
  id: string;
  content: string;
  postImg?: string;
  userId: string;
  userName: string;
  likes: string[];
  comments: PostComment[];
  createdAt: string;
}
