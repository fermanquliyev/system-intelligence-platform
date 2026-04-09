import { Environment } from '@abp/ng.core';

const baseUrl = 'http://localhost:4200';

// Use HTTP when API runs in Docker (docker-compose). Use https://localhost:44397 when running API locally with HTTPS.
const authServerUrl = 'http://localhost:44397';
const oAuthConfig = {
  issuer: `${authServerUrl}/`,
  redirectUri: baseUrl,
  clientId: 'SystemIntelligencePlatform_App',
  responseType: 'code',
  scope: 'offline_access SystemIntelligencePlatform',
  requireHttps: false,
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
      url: authServerUrl,
      rootNamespace: 'SystemIntelligencePlatform',
    },
    AbpAccountPublic: {
      url: oAuthConfig.issuer,
      rootNamespace: 'AbpAccountPublic',
    },
  },
} as Environment;
