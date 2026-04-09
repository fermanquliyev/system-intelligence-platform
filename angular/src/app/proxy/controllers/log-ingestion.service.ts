import { RestService, Rest } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class LogIngestionService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  ingest = (apiKey: string, input: LogIngestionDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, LogIngestionResultDto>({
      method: 'POST',
      url: '/api/ingest',
      body: input,
    },
    { apiName: this.apiName,...config });
}