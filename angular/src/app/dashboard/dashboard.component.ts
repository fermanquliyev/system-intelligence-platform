import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { LocalizationPipe, LocalizationService } from '@abp/ng.core';
import { DashboardService } from '../proxy/dashboard/dashboard.service';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  imports: [CommonModule, RouterModule, LocalizationPipe],
})
export class DashboardComponent implements OnInit, OnDestroy {
  private dashboardService = inject(DashboardService);
  private localization = inject(LocalizationService);
  dashboard: DashboardDto | null = null;
  private refreshInterval: any;

  ngOnInit() {
    this.loadDashboard();
    this.refreshInterval = setInterval(() => this.loadDashboard(), 30000);
  }

  ngOnDestroy() {
    if (this.refreshInterval) {
      clearInterval(this.refreshInterval);
    }
  }

  loadDashboard() {
    this.dashboardService.get().subscribe(data => {
      this.dashboard = data;
    });
  }

  getSeverityClass(severity: string): string {
    switch (severity) {
      case 'Critical': return 'bg-danger';
      case 'High': return 'bg-warning text-dark';
      case 'Medium': return 'bg-info';
      default: return 'bg-secondary';
    }
  }

  getSeverityKeys(): string[] {
    return this.dashboard ? Object.keys(this.dashboard.severityDistribution) : [];
  }

  getSeverityLabel(severity: string | number): string {
    const map: Record<string, string> = {
      '0': '::Enum:IncidentSeverity.0',
      '1': '::Enum:IncidentSeverity.1',
      '2': '::Enum:IncidentSeverity.2',
      '3': '::Enum:IncidentSeverity.3',
      Low: '::Enum:IncidentSeverity.0',
      Medium: '::Enum:IncidentSeverity.1',
      High: '::Enum:IncidentSeverity.2',
      Critical: '::Enum:IncidentSeverity.3',
    };
    const key = map[String(severity)];
    return key ? this.localization.instant(key) : this.localization.instant('::Incidents.Unknown');
  }

  getMaxTrend(): number {
    if (!this.dashboard || this.dashboard.incidentTrend.length === 0) return 1;
    return Math.max(...this.dashboard.incidentTrend.map(t => t.count), 1);
  }
}
