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
        path: '/',
        name: '::Menu:Home',
        iconClass: 'fas fa-home',
        order: 1,
        layout: eLayoutType.application,
      },
      {
        path: '/dashboard',
        name: '::Menu:Dashboard',
        iconClass: 'fas fa-tachometer-alt',
        order: 2,
        layout: eLayoutType.application,
        requiredPolicy: 'SystemIntelligencePlatform.Dashboard',
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
        path: '/subscription',
        name: '::Menu:Subscription',
        iconClass: 'fas fa-credit-card',
        order: 5,
        layout: eLayoutType.application,
      },
  ]);
}
