import { Injectable } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { Observable } from 'rxjs';

export interface DashboardDto {
  totalApplications: number;
  totalOpenIncidents: number;
  totalCriticalIncidents: number;
  totalLogsToday: number;
  severityDistribution: { [key: string]: number };
  incidentTrend: IncidentTrendItemDto[];
  recentIncidents: any[];
}

export interface IncidentTrendItemDto {
  date: string;
  count: number;
}

@Injectable({ providedIn: 'root' })
export class DashboardService {
  constructor(private rest: RestService) {}

  get(applicationId?: string): Observable<DashboardDto> {
    const params: any = {};
    if (applicationId) {
      params.applicationId = applicationId;
    }
    return this.rest.request(
      { method: 'GET', url: '/api/app/dashboard', params },
      { apiName: 'default' }
    );
  }
}
