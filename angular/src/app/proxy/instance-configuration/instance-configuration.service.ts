import { RestService, Rest } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class InstanceConfigurationService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  applyMigrations = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ApplyMigrationsResultDto>({
      method: 'POST',
      url: '/api/app/instance-configuration/apply-migrations',
    },
    { apiName: this.apiName,...config });
  

  getSnapshot = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, InstanceConfigurationSnapshotDto>({
      method: 'GET',
      url: '/api/app/instance-configuration/snapshot',
    },
    { apiName: this.apiName,...config });
  

  updateFeatures = (input: UpdateInstanceFeaturesDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'PUT',
      url: '/api/app/instance-configuration/features',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  updateSettings = (input: UpdateInstanceSettingsDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'PUT',
      url: '/api/app/instance-configuration/settings',
      body: input,
    },
    { apiName: this.apiName,...config });
}