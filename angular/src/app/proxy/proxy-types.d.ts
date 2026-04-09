export {};

declare global {
  type DashboardDto = any;
  type CostEstimateInput = any;
  type CostEstimateDto = any;
  type LogIngestionDto = any;
  type LogIngestionResultDto = any;

  type ApiKeyResultDto = any;
  type CreateMonitoredApplicationDto = any;
  type MonitoredApplicationDto = any;
  type UpdateMonitoredApplicationDto = any;

  type CreateIncidentCommentDto = any;
  type IncidentCommentDto = any;
  type IncidentDto = any;
  type IncidentSearchRequestDto = any;
  type IncidentSearchResultDto = any;
  type GetIncidentListInput = any;
  type IncidentStatus = number;

  type ApplyMigrationsResultDto = any;
  type InstanceConfigurationSnapshotDto = any;
  type UpdateInstanceFeaturesDto = any;
  type UpdateInstanceSettingsDto = any;
  type InstanceFeatureStateDto = any;
  type InstanceSettingStateDto = any;

  type CreateWebhookDto = any;
  type WebhookRegistrationDto = any;
}
