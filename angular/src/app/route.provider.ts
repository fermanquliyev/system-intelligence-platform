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
        path: '/books',
        name: '::Menu:Books',
        iconClass: 'fas fa-book',
        order: 10,
        layout: eLayoutType.application,
        requiredPolicy: 'SystemIntelligencePlatform.Books',
      },
  ]);
}
