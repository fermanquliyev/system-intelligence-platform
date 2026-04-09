import { RestService, Rest } from '@abp/ng.core';
import type { PagedAndSortedResultRequestDto, PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class MonitoredApplicationService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  create = (input: CreateMonitoredApplicationDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ApiKeyResultDto>({
      method: 'POST',
      url: '/api/app/monitored-application',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/monitored-application/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, MonitoredApplicationDto>({
      method: 'GET',
      url: `/api/app/monitored-application/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: PagedAndSortedResultRequestDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<MonitoredApplicationDto>>({
      method: 'GET',
      url: '/api/app/monitored-application',
      params: { sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  regenerateApiKey = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ApiKeyResultDto>({
      method: 'POST',
      url: `/api/app/monitored-application/${id}/regenerate-api-key`,
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: UpdateMonitoredApplicationDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, MonitoredApplicationDto>({
      method: 'PUT',
      url: `/api/app/monitored-application/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });
}