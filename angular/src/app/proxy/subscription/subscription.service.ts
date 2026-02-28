import { Injectable } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { Observable } from 'rxjs';

export interface SubscriptionDto {
  plan: string;
  status: string;
  currentPeriodStart?: string;
  currentPeriodEnd?: string;
  cancelAtPeriodEnd?: boolean;
}

export interface UsageDto {
  errorsProcessed: number;
  errorsLimit: number;
  applicationsCount: number;
  applicationsLimit: number;
  periodStart: string;
  periodEnd: string;
}

@Injectable({ providedIn: 'root' })
export class SubscriptionService {
  apiName = 'default';
  url = '/api/app/subscription';

  constructor(private rest: RestService) {}

  getCurrent(): Observable<SubscriptionDto> {
    return this.rest.request({ method: 'GET', url: this.url }, { apiName: this.apiName });
  }

  getUsage(): Observable<UsageDto> {
    return this.rest.request({ method: 'GET', url: `${this.url}/usage` }, { apiName: this.apiName });
  }

  createCheckout(plan: string): Observable<any> {
    return this.rest.request({ method: 'POST', url: `${this.url}/checkout`, params: { plan } }, { apiName: this.apiName });
  }
}
