import { Environment } from '@abp/ng.core';

const baseUrl = 'http://localhost:4200';

// HTTP to match API in Docker (docker-compose). Use https when API runs locally with HTTPS.
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
  production: true,
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
  remoteEnv: {
    url: '/getEnvConfig',
    mergeStrategy: 'deepmerge'
  }
} as Environment;
