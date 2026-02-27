import { Environment } from '@abp/ng.core';

const baseUrl = 'http://localhost:4200';

const oAuthConfig = {
  issuer: 'https://localhost:44397/',
  redirectUri: baseUrl,
  clientId: 'SystemIntelligencePlatform_App',
  responseType: 'code',
  scope: 'offline_access SystemIntelligencePlatform',
  requireHttps: true,
};

export const environment = {
  production: false,
  application: {
    baseUrl,
    name: 'SystemIntelligencePlatform',
  },
  oAuthConfig,
  apis: {
    default: {
      url: 'https://localhost:44397',
      rootNamespace: 'SystemIntelligencePlatform',
    },
    AbpAccountPublic: {
      url: oAuthConfig.issuer,
      rootNamespace: 'AbpAccountPublic',
    },
  },
} as Environment;
