import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto, PagedResultRequestDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class IncidentService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  addComment = (incidentId: string, input: CreateIncidentCommentDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, IncidentCommentDto>({
      method: 'POST',
      url: `/api/app/incident/comment/${incidentId}`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, IncidentDto>({
      method: 'GET',
      url: `/api/app/incident/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getComments = (incidentId: string, input: PagedResultRequestDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<IncidentCommentDto>>({
      method: 'GET',
      url: `/api/app/incident/comments/${incidentId}`,
      params: { skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetIncidentListInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<IncidentDto>>({
      method: 'GET',
      url: '/api/app/incident',
      params: { applicationId: input.applicationId, severity: input.severity, status: input.status, filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  resolve = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, IncidentDto>({
      method: 'POST',
      url: `/api/app/incident/${id}/resolve`,
    },
    { apiName: this.apiName,...config });
  

  search = (input: IncidentSearchRequestDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, IncidentSearchResultDto>({
      method: 'POST',
      url: '/api/app/incident/search',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  updateStatus = (id: string, status: IncidentStatus, config?: Partial<Rest.Config>) =>
    this.restService.request<any, IncidentDto>({
      method: 'PUT',
      url: `/api/app/incident/${id}/status`,
      params: { status },
    },
    { apiName: this.apiName,...config });

  assign = (id: string, input: { userId?: string | null }, config?: Partial<Rest.Config>) =>
    this.restService.request<any, IncidentDto>({
      method: 'POST',
      url: `/api/app/incident/${id}/assign`,
      body: input,
    },
    { apiName: this.apiName,...config });

  getMergedChildren = (canonicalIncidentId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, IncidentDto[]>({
      method: 'GET',
      url: `/api/app/incident/merged-children/${canonicalIncidentId}`,
    },
    { apiName: this.apiName,...config });

  getGlobalTimeline = (input: {
    skipCount?: number;
    maxResultCount?: number;
    sorting?: string;
    applicationId?: string;
    severity?: number;
    fromUtc?: string;
    toUtc?: string;
  }, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<any>>({
      method: 'GET',
      url: '/api/app/incident/global-timeline',
      params: {
        skipCount: input.skipCount,
        maxResultCount: input.maxResultCount,
        sorting: input.sorting,
        applicationId: input.applicationId,
        severity: input.severity,
        fromUtc: input.fromUtc,
        toUtc: input.toUtc,
      },
    },
    { apiName: this.apiName,...config });

  getRootCauseTimeline = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, any[]>({
      method: 'GET',
      url: `/api/app/incident/${id}/root-cause-timeline`,
    },
    { apiName: this.apiName,...config });
}