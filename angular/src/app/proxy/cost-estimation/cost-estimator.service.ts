import { RestService, Rest } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class CostEstimatorService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  calculateByInput = (input: CostEstimateInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CostEstimateDto>({
      method: 'POST',
      url: '/api/app/cost-estimator/calculate',
      body: input,
    },
    { apiName: this.apiName,...config });
}