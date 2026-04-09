import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LocalizationPipe, RestService } from '@abp/ng.core';
import { MonitoredApplicationService } from '../proxy/monitored-applications/monitored-application.service';

@Component({
  selector: 'app-log-clusters',
  templateUrl: './log-clusters.component.html',
  imports: [CommonModule, FormsModule, LocalizationPipe],
})
export class LogClustersComponent implements OnInit {
  private rest = inject(RestService);
  private apps = inject(MonitoredApplicationService);

  clusters: any[] = [];
  skip = 0;
  take = 50;
  applicationFilter: string | null = null;
  applications: { id: string; name: string }[] = [];
  lastRunResult: number | null = null;
  error = '';

  ngOnInit() {
    this.apps.getList({ skipCount: 0, maxResultCount: 200 } as any).subscribe((r: any) => {
      this.applications = (r.items || []).map((a: any) => ({ id: a.id, name: a.name }));
    });
    this.load();
  }

  load() {
    this.error = '';
    this.rest
      .request<any, any>(
        {
          method: 'GET',
          url: '/api/app/log-cluster',
          params: { skipCount: this.skip, maxResultCount: this.take },
        },
        { apiName: 'Default' },
      )
      .subscribe({
        next: r => (this.clusters = r.items || []),
        error: e => (this.error = e?.error?.error?.message || e.message || 'Request failed'),
      });
  }

  runClustering() {
    this.error = '';
    const params: Record<string, string> = {};
    if (this.applicationFilter) params['applicationId'] = this.applicationFilter;
    this.rest
      .request<any, number>(
        {
          method: 'POST',
          url: '/api/app/log-cluster/run-clustering',
          params,
        },
        { apiName: 'Default' },
      )
      .subscribe({
        next: n => {
          this.lastRunResult = n;
          this.load();
        },
        error: e => (this.error = e?.error?.error?.message || e.message || 'Request failed'),
      });
  }
}
