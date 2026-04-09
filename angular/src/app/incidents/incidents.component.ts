import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { NgxDatatableModule } from '@swimlane/ngx-datatable';
import {
  ListService,
  PagedResultDto,
  LocalizationPipe,
  PermissionDirective,
  LocalizationService,
} from '@abp/ng.core';
import {
  NgxDatatableDefaultDirective,
  NgxDatatableListDirective,
} from '@abp/ng.theme.shared';
import {
  IncidentService,
} from '../proxy/incidents/incident.service';

const severityOptions = [
  { value: 0, key: '::Enum:IncidentSeverity.0' },
  { value: 1, key: '::Enum:IncidentSeverity.1' },
  { value: 2, key: '::Enum:IncidentSeverity.2' },
  { value: 3, key: '::Enum:IncidentSeverity.3' },
];

const statusOptions = [
  { value: 0, key: '::Enum:IncidentStatus.0' },
  { value: 1, key: '::Enum:IncidentStatus.1' },
  { value: 2, key: '::Enum:IncidentStatus.2' },
  { value: 3, key: '::Enum:IncidentStatus.3' },
  { value: 4, key: '::Enum:IncidentStatus.4' },
];

@Component({
  selector: 'app-incidents',
  templateUrl: './incidents.component.html',
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    NgxDatatableModule,
    NgxDatatableListDirective,
    NgxDatatableDefaultDirective,
    PermissionDirective,
    LocalizationPipe,
  ],
  providers: [ListService],
})
export class IncidentsComponent implements OnInit {
  readonly list = inject(ListService);
  private incidentService = inject(IncidentService);
  private localization = inject(LocalizationService);

  data = { items: [], totalCount: 0 } as PagedResultDto<IncidentDto>;
  severityOptions = severityOptions;
  statusOptions = statusOptions;

  filterSeverity: number | null = null;
  filterStatus: number | null = null;
  searchQuery = '';

  searchResults: any = null;

  ngOnInit() {
    this.list.hookToQuery(query => {
      const params: any = { ...query };
      if (this.filterSeverity !== null) params.severity = this.filterSeverity;
      if (this.filterStatus !== null) params.status = this.filterStatus;
      return this.incidentService.getList(params);
    }).subscribe((response: any) => {
      this.data = response;
    });
  }

  applyFilters() {
    this.list.get();
  }

  clearFilters() {
    this.filterSeverity = null;
    this.filterStatus = null;
    this.list.get();
  }

  search() {
    if (!this.searchQuery.trim()) return;
    this.incidentService.search({ query: this.searchQuery }).subscribe(result => {
      this.searchResults = result;
    });
  }

  clearSearch() {
    this.searchQuery = '';
    this.searchResults = null;
  }

  getSeverityClass(severity: number): string {
    switch (severity) {
      case 3: return 'bg-danger';
      case 2: return 'bg-warning text-dark';
      case 1: return 'bg-info';
      default: return 'bg-secondary';
    }
  }

  getSeverityLabel(severity: number): string {
    const key = this.severityOptions.find(o => o.value === severity)?.key;
    return key ? this.localization.instant(key) : this.localization.instant('::Incidents.Unknown');
  }

  getStatusLabel(status: number): string {
    const key = this.statusOptions.find(o => o.value === status)?.key;
    return key ? this.localization.instant(key) : this.localization.instant('::Incidents.Unknown');
  }
}
