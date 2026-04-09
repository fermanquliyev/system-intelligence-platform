import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class LogSearchProxyService {
  private restService = inject(RestService);
  apiName = 'Default';

  search = (input: Record<string, unknown>, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<any>>({
      method: 'GET',
      url: '/api/app/log-search/search',
      params: input as any,
    },
    { apiName: this.apiName, ...config });

  getSavedList = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, { items: any[] }>({
      method: 'GET',
      url: '/api/app/log-search/saved',
    },
    { apiName: this.apiName, ...config });

  createSaved = (input: { name: string; filterJson: string }, config?: Partial<Rest.Config>) =>
    this.restService.request<any, any>({
      method: 'POST',
      url: '/api/app/log-search/saved',
      body: input,
    },
    { apiName: this.apiName, ...config });

  deleteSaved = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/log-search/saved/${id}`,
    },
    { apiName: this.apiName, ...config });
}
