import { Injectable } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { Observable } from 'rxjs';

export interface InstanceConfigurationSnapshotDto {
  features: InstanceFeatureStateDto[];
  settings: InstanceSettingStateDto[];
}

export interface InstanceFeatureStateDto {
  id: string;
  name: string;
  displayName: string;
  description?: string;
  isEnabled: boolean;
  displayOrder: number;
}

export interface InstanceSettingStateDto {
  key: string;
  displayName: string;
  description?: string;
  category: string;
  isSecret: boolean;
  effectiveDisplayValue: string;
  isOverriddenInDatabase: boolean;
}

export interface UpdateInstanceFeaturesDto {
  features: Record<string, boolean>;
}

export interface UpdateInstanceSettingsDto {
  values: Record<string, string | null | undefined>;
}

export interface ApplyMigrationsResultDto {
  success: boolean;
  message?: string;
}

@Injectable({ providedIn: 'root' })
export class InstanceConfigurationService {
  apiName = 'default';
  private base = '/api/app/instance-configuration';

  constructor(private rest: RestService) {}

  get(): Observable<InstanceConfigurationSnapshotDto> {
    return this.rest.request(
      { method: 'GET', url: `${this.base}/snapshot` },
      { apiName: this.apiName },
    );
  }

  updateFeatures(body: UpdateInstanceFeaturesDto): Observable<void> {
    return this.rest.request(
      { method: 'PUT', url: `${this.base}/features`, body },
      { apiName: this.apiName },
    );
  }

  updateSettings(body: UpdateInstanceSettingsDto): Observable<void> {
    return this.rest.request(
      { method: 'PUT', url: `${this.base}/settings`, body },
      { apiName: this.apiName },
    );
  }

  applyMigrations(): Observable<ApplyMigrationsResultDto> {
    return this.rest.request(
      { method: 'POST', url: `${this.base}/apply-migrations` },
      { apiName: this.apiName },
    );
  }
}
