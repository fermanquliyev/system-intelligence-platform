import { RestService, Rest } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class AiCopilotService {
  private restService = inject(RestService);
  apiName = 'Default';

  getAnalysis = (incidentId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, any>({
      method: 'GET',
      url: `/api/app/ai-copilot/analysis/${incidentId}`,
    },
    { apiName: this.apiName, ...config });

  postFollowUp = (incidentId: string, input: { message: string }, config?: Partial<Rest.Config>) =>
    this.restService.request<any, any>({
      method: 'POST',
      url: `/api/app/ai-copilot/follow-up/${incidentId}`,
      body: input,
    },
    { apiName: this.apiName, ...config });

  getConversation = (incidentId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, any[]>({
      method: 'GET',
      url: `/api/app/ai-copilot/conversation/${incidentId}`,
    },
    { apiName: this.apiName, ...config });
}
