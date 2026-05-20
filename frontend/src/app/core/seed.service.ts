import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class SeedService {
  private apiUrl = '/api';

  constructor(private http: HttpClient) { }

  populateDatabase(): Observable<any> {
    return this.http.post(`${this.apiUrl}/seed/populate`, {});
  }
}
