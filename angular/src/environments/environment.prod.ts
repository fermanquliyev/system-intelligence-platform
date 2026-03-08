import { Environment } from '@abp/ng.core';

const baseUrl = 'http://localhost:4200';

const oAuthConfig = {
  issuer: 'http://localhost:44397/',
  redirectUri: baseUrl,
  clientId: 'SystemIntelligencePlatform_App',
  responseType: 'code',
  scope: 'offline_access SystemIntelligencePlatform',
  requireHttps: false,
};

export const environment = {
  production: true,
  application: {
    baseUrl,
    name: 'SystemIntelligencePlatform',
  },
  oAuthConfig,
  apis: {
    default: {
      url: 'http://localhost:44397',
      rootNamespace: 'SystemIntelligencePlatform',
    },
    AbpAccountPublic: {
      url: oAuthConfig.issuer,
      rootNamespace: 'AbpAccountPublic',
    },
  },
  remoteEnv: {
    url: '/getEnvConfig',
    mergeStrategy: 'deepmerge'
  }
} as Environment;
