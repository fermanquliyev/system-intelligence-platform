import { Injectable } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { Observable } from 'rxjs';

export interface IncidentDto {
  id: string;
  applicationId: string;
  applicationName: string;
  title: string;
  description?: string;
  severity: number;
  status: number;
  hashSignature: string;
  occurrenceCount: number;
  firstOccurrence: string;
  lastOccurrence: string;
  sentimentScore?: number;
  keyPhrases?: string;
  entities?: string;
  aiAnalyzedAt?: string;
  resolvedAt?: string;
  comments: IncidentCommentDto[];
}

export interface IncidentCommentDto {
  id: string;
  incidentId: string;
  content: string;
  creationTime: string;
  creatorId: string;
}

export interface IncidentSearchResultDto {
  totalCount: number;
  items: IncidentSearchItemDto[];
}

export interface IncidentSearchItemDto {
  id: string;
  title: string;
  description?: string;
  severity: string;
  applicationName: string;
  keyPhrases?: string;
  entities?: string;
}

export const severityOptions = [
  { value: 0, label: 'Low' },
  { value: 1, label: 'Medium' },
  { value: 2, label: 'High' },
  { value: 3, label: 'Critical' },
];

export const statusOptions = [
  { value: 0, label: 'Open' },
  { value: 1, label: 'Acknowledged' },
  { value: 2, label: 'In Progress' },
  { value: 3, label: 'Resolved' },
  { value: 4, label: 'Closed' },
];

@Injectable({ providedIn: 'root' })
export class IncidentService {
  apiName = 'default';
  url = '/api/app/incident';

  constructor(private rest: RestService) {}

  getList(params: any): Observable<any> {
    return this.rest.request({ method: 'GET', url: this.url, params }, { apiName: this.apiName });
  }

  get(id: string): Observable<IncidentDto> {
    return this.rest.request({ method: 'GET', url: `${this.url}/${id}` }, { apiName: this.apiName });
  }

  resolve(id: string): Observable<IncidentDto> {
    return this.rest.request({ method: 'POST', url: `${this.url}/${id}/resolve` }, { apiName: this.apiName });
  }

  updateStatus(id: string, status: number): Observable<IncidentDto> {
    return this.rest.request({ method: 'PUT', url: `${this.url}/${id}/status`, params: { status } }, { apiName: this.apiName });
  }

  addComment(incidentId: string, content: string): Observable<IncidentCommentDto> {
    return this.rest.request({ method: 'POST', url: `${this.url}/${incidentId}/comments`, body: { content } }, { apiName: this.apiName });
  }

  getComments(incidentId: string, params: any): Observable<any> {
    return this.rest.request({ method: 'GET', url: `${this.url}/${incidentId}/comments`, params }, { apiName: this.apiName });
  }

  search(body: { query: string; skip?: number; take?: number }): Observable<IncidentSearchResultDto> {
    return this.rest.request({ method: 'POST', url: '/api/app/incidents/search', body }, { apiName: this.apiName });
  }
}
