import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface UserProfile {
  id: string;
  name: string;
  email: string;
  bio: string;
  imgUrl: string;
  followers: string[];
  following: string[];
}

@Injectable({
  providedIn: 'root',
})
export class UserService {
  private apiUrl = '/api';

  constructor(private http: HttpClient) { }

  getUserProfile(id: string): Observable<UserProfile> {
    return this.http.get<UserProfile>(`${this.apiUrl}/user/${id}`);
  }

  updateUserProfile(userId: string, data: { name: string; bio: string; imgUrl: string }): Observable<any> {
    return this.http.patch(`${this.apiUrl}/user/update/${userId}`, data);
  }
}
