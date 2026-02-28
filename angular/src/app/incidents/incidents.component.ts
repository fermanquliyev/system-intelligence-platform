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
} from '@abp/ng.core';
import {
  NgxDatatableDefaultDirective,
  NgxDatatableListDirective,
} from '@abp/ng.theme.shared';
import {
  IncidentService,
  IncidentDto,
  severityOptions,
  statusOptions,
} from '../proxy/incidents/incident.service';

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
    return this.severityOptions.find(o => o.value === severity)?.label || 'Unknown';
  }

  getStatusLabel(status: number): string {
    return this.statusOptions.find(o => o.value === status)?.label || 'Unknown';
  }
}
