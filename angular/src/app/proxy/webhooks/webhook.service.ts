import { RestService, Rest } from '@abp/ng.core';
import type { ListResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class WebhookService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  create = (input: CreateWebhookDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, WebhookRegistrationDto>({
      method: 'POST',
      url: '/api/app/webhook',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/webhook/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<WebhookRegistrationDto>>({
      method: 'GET',
      url: '/api/app/webhook',
    },
    { apiName: this.apiName,...config });
  

  toggle = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, WebhookRegistrationDto>({
      method: 'POST',
      url: `/api/app/webhook/${id}/toggle`,
    },
    { apiName: this.apiName,...config });
}