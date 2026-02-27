import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LocalizationPipe } from '@abp/ng.core';
import { DashboardService, DashboardDto } from '../proxy/dashboard/dashboard.service';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  imports: [CommonModule, LocalizationPipe],
})
export class DashboardComponent implements OnInit, OnDestroy {
  private dashboardService = inject(DashboardService);
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

  getMaxTrend(): number {
    if (!this.dashboard || this.dashboard.incidentTrend.length === 0) return 1;
    return Math.max(...this.dashboard.incidentTrend.map(t => t.count), 1);
  }
}
