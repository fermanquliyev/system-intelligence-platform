import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LocalizationPipe } from '@abp/ng.core';
import { LogSearchProxyService } from '../proxy/log-search/log-search-proxy.service';

@Component({
  selector: 'app-log-search-page',
  templateUrl: './log-search-page.component.html',
  imports: [CommonModule, FormsModule, LocalizationPipe],
})
export class LogSearchPageComponent implements OnInit {
  private logSearch = inject(LogSearchProxyService);

  query = '';
  useFullText = true;
  revealSensitive = false;
  items: any[] = [];
  totalCount = 0;
  skipCount = 0;
  maxResultCount = 20;
  saved: any[] = [];

  ngOnInit() {
    this.loadSaved();
  }

  search() {
    this.logSearch
      .search({
        query: this.query,
        useFullText: this.useFullText,
        revealSensitive: this.revealSensitive,
        skipCount: this.skipCount,
        maxResultCount: this.maxResultCount,
      })
      .subscribe(r => {
        this.items = r.items || [];
        this.totalCount = r.totalCount || 0;
      });
  }

  loadSaved() {
    this.logSearch.getSavedList().subscribe(r => {
      this.saved = (r as any).items || [];
    });
  }

  saveQuery() {
    const name = prompt('Name');
    if (!name) return;
    this.logSearch
      .createSaved({
        name,
        filterJson: JSON.stringify({ query: this.query, useFullText: this.useFullText }),
      })
      .subscribe(() => this.loadSaved());
  }
}
