import { authGuard, permissionGuard } from '@abp/ng.core';
import { Routes } from '@angular/router';

export const APP_ROUTES: Routes = [
  {
    path: '',
    pathMatch: 'full',
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
    path: 'tenant-management',
    loadChildren: () => import('@abp/ng.tenant-management').then(c => c.createRoutes()),
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
    path: 'subscription',
    loadComponent: () => import('./subscription/subscription.component').then(c => c.SubscriptionComponent),
    canActivate: [authGuard],
  },
];
