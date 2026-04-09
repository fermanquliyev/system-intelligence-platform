import { RoutesService, eLayoutType } from '@abp/ng.core';
import { inject, provideAppInitializer } from '@angular/core';

export const APP_ROUTE_PROVIDER = [
  provideAppInitializer(() => {
    configureRoutes();
  }),
];

function configureRoutes() {
  const routes = inject(RoutesService);
  routes.add([
      {
        path: '/dashboard',
        name: '::Menu:Dashboard',
        iconClass: 'fas fa-tachometer-alt',
        order: 1,
        layout: eLayoutType.application,
        requiredPolicy: 'SystemIntelligencePlatform.Dashboard',
      },
      {
        path: '/about',
        name: '::Menu:About',
        iconClass: 'fas fa-info-circle',
        order: 2,
        layout: eLayoutType.empty,
      },
      {
        path: '/applications',
        name: '::Menu:Applications',
        iconClass: 'fas fa-server',
        order: 3,
        layout: eLayoutType.application,
        requiredPolicy: 'SystemIntelligencePlatform.Applications',
      },
      {
        path: '/incidents',
        name: '::Menu:Incidents',
        iconClass: 'fas fa-exclamation-triangle',
        order: 4,
        layout: eLayoutType.application,
        requiredPolicy: 'SystemIntelligencePlatform.Incidents',
      },
      {
        path: '/instance-settings',
        name: '::Menu:InstanceSettings',
        iconClass: 'fas fa-sliders-h',
        order: 5,
        layout: eLayoutType.application,
        requiredPolicy: 'SystemIntelligencePlatform.InstanceConfiguration',
      },
      {
        path: '/timeline',
        name: '::Menu:GlobalTimeline',
        iconClass: 'fas fa-stream',
        order: 6,
        layout: eLayoutType.application,
        requiredPolicy: 'SystemIntelligencePlatform.Incidents.Timeline',
      },
      {
        path: '/log-search',
        name: '::Menu:LogSearch',
        iconClass: 'fas fa-search',
        order: 7,
        layout: eLayoutType.application,
        requiredPolicy: 'SystemIntelligencePlatform.LogEvents.Search',
      },
      {
        path: '/alert-rules',
        name: '::Menu:AlertRules',
        iconClass: 'fas fa-bell',
        order: 8,
        layout: eLayoutType.application,
        requiredPolicy: 'SystemIntelligencePlatform.AlertRules',
      },
      {
        path: '/metrics',
        name: '::Menu:Metrics',
        iconClass: 'fas fa-chart-line',
        order: 9,
        layout: eLayoutType.application,
        requiredPolicy: 'SystemIntelligencePlatform.Metrics',
      },
      {
        path: '/grouped-logs',
        name: '::Menu:GroupedLogs',
        iconClass: 'fas fa-layer-group',
        order: 10,
        layout: eLayoutType.application,
        requiredPolicy: 'SystemIntelligencePlatform.LogEvents',
      },
      {
        path: '/playbooks',
        name: '::Menu:Playbooks',
        iconClass: 'fas fa-tasks',
        order: 11,
        layout: eLayoutType.application,
        requiredPolicy: 'SystemIntelligencePlatform.Playbooks',
      },
      {
        path: '/log-sources',
        name: '::Menu:LogSources',
        iconClass: 'fas fa-plug',
        order: 12,
        layout: eLayoutType.application,
        requiredPolicy: 'SystemIntelligencePlatform.LogSources',
      },
  ]);
}
