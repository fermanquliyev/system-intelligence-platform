import { Injectable } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { Observable } from 'rxjs';

export interface MonitoredApplicationDto {
  id: string;
  name: string;
  description?: string;
  environment?: string;
  isActive: boolean;
  creationTime: string;
}

export interface CreateMonitoredApplicationDto {
  name: string;
  description?: string;
  environment?: string;
}

export interface UpdateMonitoredApplicationDto {
  name: string;
  description?: string;
  environment?: string;
  isActive: boolean;
}

export interface ApiKeyResultDto {
  applicationId: string;
  apiKey: string;
}

@Injectable({ providedIn: 'root' })
export class ApplicationService {
  apiName = 'default';
  url = '/api/app/monitored-application';

  constructor(private rest: RestService) {}

  getList(params: any): Observable<any> {
    return this.rest.request({ method: 'GET', url: this.url, params }, { apiName: this.apiName });
  }

  get(id: string): Observable<MonitoredApplicationDto> {
    return this.rest.request({ method: 'GET', url: `${this.url}/${id}` }, { apiName: this.apiName });
  }

  create(body: CreateMonitoredApplicationDto): Observable<ApiKeyResultDto> {
    return this.rest.request({ method: 'POST', url: this.url, body }, { apiName: this.apiName });
  }

  update(id: string, body: UpdateMonitoredApplicationDto): Observable<MonitoredApplicationDto> {
    return this.rest.request({ method: 'PUT', url: `${this.url}/${id}`, body }, { apiName: this.apiName });
  }

  delete(id: string): Observable<void> {
    return this.rest.request({ method: 'DELETE', url: `${this.url}/${id}` }, { apiName: this.apiName });
  }

  regenerateApiKey(id: string): Observable<ApiKeyResultDto> {
    return this.rest.request({ method: 'POST', url: `${this.url}/${id}/regenerate-api-key` }, { apiName: this.apiName });
  }
}
