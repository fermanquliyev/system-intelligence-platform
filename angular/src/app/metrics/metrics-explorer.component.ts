import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LocalizationPipe, PermissionDirective, RestService } from '@abp/ng.core';
import { MonitoredApplicationService } from '../proxy/monitored-applications/monitored-application.service';

@Component({
  selector: 'app-metrics-explorer',
  templateUrl: './metrics-explorer.component.html',
  imports: [CommonModule, FormsModule, LocalizationPipe, PermissionDirective],
})
export class MetricsExplorerComponent implements OnInit {
  private rest = inject(RestService);
  private apps = inject(MonitoredApplicationService);

  applications: { id: string; name: string }[] = [];
  applicationId = '';
  metricName = 'cpu_percent';
  fromUtc = '';
  toUtc = '';
  maxPoints = 200;
  series: { name: string; points: { timestamp: string; value: number }[] } | null = null;
  correlation: { name: string; points: { timestamp: string; value: number }[] }[] = [];
  incidentIdForCorrelation = '';
  ingestValue = 1;
  error = '';

  ngOnInit() {
    const now = new Date();
    const dayAgo = new Date(now.getTime() - 86400000);
    this.toUtc = now.toISOString().slice(0, 16);
    this.fromUtc = dayAgo.toISOString().slice(0, 16);

    this.apps.getList({ skipCount: 0, maxResultCount: 100 } as any).subscribe((r: any) => {
      this.applications = (r.items || []).map((a: any) => ({ id: a.id, name: a.name }));
      if (this.applications.length && !this.applicationId) {
        this.applicationId = this.applications[0].id;
      }
    });
  }

  loadSeries() {
    this.error = '';
    this.series = null;
    const from = new Date(this.fromUtc).toISOString();
    const to = new Date(this.toUtc).toISOString();
    this.rest
      .request<any, any>(
        {
          method: 'GET',
          url: '/api/app/metric/series',
          params: {
            applicationId: this.applicationId,
            name: this.metricName,
            fromUtc: from,
            toUtc: to,
            maxPoints: this.maxPoints,
          },
        },
        { apiName: 'Default' },
      )
      .subscribe({
        next: s => (this.series = s),
        error: e => (this.error = e?.error?.error?.message || e.message || 'Request failed'),
      });
  }

  loadCorrelation() {
    this.error = '';
    this.correlation = [];
    if (!this.incidentIdForCorrelation) return;
    this.rest
      .request<any, any[]>(
        {
          method: 'GET',
          url: `/api/app/metric/correlation-for-incident/${this.incidentIdForCorrelation}`,
        },
        { apiName: 'Default' },
      )
      .subscribe({
        next: list => (this.correlation = list || []),
        error: e => (this.error = e?.error?.error?.message || e.message || 'Request failed'),
      });
  }

  ingestSample() {
    this.error = '';
    const ts = new Date().toISOString();
    this.rest
      .request<any, void>(
        {
          method: 'POST',
          url: '/api/app/metric/ingest',
          body: {
            samples: [
              {
                applicationId: this.applicationId,
                name: this.metricName,
                timestamp: ts,
                value: this.ingestValue,
              },
            ],
          },
        },
        { apiName: 'Default' },
      )
      .subscribe({
        next: () => this.loadSeries(),
        error: e => (this.error = e?.error?.error?.message || e.message || 'Request failed'),
      });
  }

  polylinePoints(points: { timestamp: string; value: number }[] | undefined): string {
    if (!points?.length) return '';
    const values = points.map(p => p.value);
    const min = Math.min(...values);
    const max = Math.max(...values);
    const range = max - min || 1;
    const w = 100;
    const h = 36;
    return points
      .map((p, i) => {
        const x = points.length === 1 ? w / 2 : (i / (points.length - 1)) * w;
        const y = h - ((p.value - min) / range) * (h - 4) - 2;
        return `${x},${y}`;
      })
      .join(' ');
  }
}
