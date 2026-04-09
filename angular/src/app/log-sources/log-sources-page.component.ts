import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LocalizationPipe, PermissionDirective, RestService } from '@abp/ng.core';

@Component({
  selector: 'app-log-sources-page',
  templateUrl: './log-sources-page.component.html',
  imports: [CommonModule, FormsModule, LocalizationPipe, PermissionDirective],
})
export class LogSourcesPageComponent implements OnInit {
  private rest = inject(RestService);

  items: any[] = [];
  name = '';
  sourceType = 0;
  settingsJson = '{}';
  error = '';

  ngOnInit() {
    this.load();
  }

  load() {
    this.rest
      .request<any, any>(
        { method: 'GET', url: '/api/app/log-source-configuration', params: { skipCount: 0, maxResultCount: 100 } },
        { apiName: 'Default' },
      )
      .subscribe({
        next: r => (this.items = r.items || []),
        error: e => (this.error = e?.error?.error?.message || e.message || 'Request failed'),
      });
  }

  create() {
    this.rest
      .request<any, any>(
        {
          method: 'POST',
          url: '/api/app/log-source-configuration',
          body: { name: this.name, sourceType: this.sourceType, settingsJson: this.settingsJson, isEnabled: true },
        },
        { apiName: 'Default' },
      )
      .subscribe(() => {
        this.name = '';
        this.load();
      });
  }

  toggle(x: any) {
    this.rest
      .request<any, any>(
        {
          method: 'PUT',
          url: `/api/app/log-source-configuration/${x.id}/set-enabled`,
          params: { isEnabled: !x.isEnabled },
        },
        { apiName: 'Default' },
      )
      .subscribe(() => this.load());
  }

  delete(id: string) {
    if (!confirm('Delete?')) return;
    this.rest
      .request<any, void>({ method: 'DELETE', url: `/api/app/log-source-configuration/${id}` }, { apiName: 'Default' })
      .subscribe(() => this.load());
  }
}
