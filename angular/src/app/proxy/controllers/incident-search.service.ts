import { RestService, Rest } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class IncidentSearchService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  search = (input: IncidentSearchRequestDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, IncidentSearchResultDto>({
      method: 'POST',
      url: '/api/app/incidents/search',
      body: input,
    },
    { apiName: this.apiName,...config });
}