import { authGuard, permissionGuard } from '@abp/ng.core';
import { Routes } from '@angular/router';

export const APP_ROUTES: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'dashboard',
  },
  {
    path: 'about',
    loadComponent: () => import('./home/home.component').then(c => c.HomeComponent),
  },
  {
    path: 'account',
    loadChildren: () => import('@abp/ng.account').then(c => c.createRoutes()),
  },
  {
    path: 'identity',
    loadChildren: () => import('@abp/ng.identity').then(c => c.createRoutes()),
  },
  {
    path: 'setting-management',
    loadChildren: () => import('@abp/ng.setting-management').then(c => c.createRoutes()),
  },
  {
    path: 'dashboard',
    loadComponent: () => import('./dashboard/dashboard.component').then(c => c.DashboardComponent),
    canActivate: [authGuard, permissionGuard],
  },
  {
    path: 'applications',
    loadComponent: () => import('./applications/applications.component').then(c => c.ApplicationsComponent),
    canActivate: [authGuard, permissionGuard],
  },
  {
    path: 'incidents',
    loadComponent: () => import('./incidents/incidents.component').then(c => c.IncidentsComponent),
    canActivate: [authGuard, permissionGuard],
  },
  {
    path: 'incidents/:id',
    loadComponent: () => import('./incidents/incident-detail.component').then(c => c.IncidentDetailComponent),
    canActivate: [authGuard, permissionGuard],
  },
  {
    path: 'instance-settings',
    loadComponent: () => import('./instance-settings/instance-settings.component').then(c => c.InstanceSettingsComponent),
    canActivate: [authGuard, permissionGuard],
  },
  {
    path: 'timeline',
    loadComponent: () => import('./timeline/global-timeline.component').then(c => c.GlobalTimelineComponent),
    canActivate: [authGuard, permissionGuard],
  },
  {
    path: 'log-search',
    loadComponent: () => import('./log-search/log-search-page.component').then(c => c.LogSearchPageComponent),
    canActivate: [authGuard, permissionGuard],
  },
  {
    path: 'alert-rules',
    loadComponent: () => import('./alert-rules/alert-rules.component').then(c => c.AlertRulesComponent),
    canActivate: [authGuard, permissionGuard],
  },
  {
    path: 'metrics',
    loadComponent: () => import('./metrics/metrics-explorer.component').then(c => c.MetricsExplorerComponent),
    canActivate: [authGuard, permissionGuard],
  },
  {
    path: 'grouped-logs',
    loadComponent: () => import('./log-clusters/log-clusters.component').then(c => c.LogClustersComponent),
    canActivate: [authGuard, permissionGuard],
  },
  {
    path: 'playbooks',
    loadComponent: () => import('./playbooks/playbooks-page.component').then(c => c.PlaybooksPageComponent),
    canActivate: [authGuard, permissionGuard],
  },
  {
    path: 'log-sources',
    loadComponent: () => import('./log-sources/log-sources-page.component').then(c => c.LogSourcesPageComponent),
    canActivate: [authGuard, permissionGuard],
  },
];
