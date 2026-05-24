import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { UserService } from '../../../features/user/user.service';


@Component({
  selector: 'app-search-bar',
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './search-bar.html',
  styleUrl: './search-bar.css',
})
export class SearchBar {
  searchQuery = '';
  results: any[] = [];

  constructor(private userService: UserService) { }

  onSearch(): void {
    if (this.searchQuery.trim().length < 2) {
      this.results = [];
      return;
    }
    this.userService.searchUsers(this.searchQuery).subscribe({
      next: (data) => this.results = data,
      error: (err) => console.error(err)
    });
  }

  clearSearch(): void {
    this.searchQuery = '';
    this.results = [];
  }
}
