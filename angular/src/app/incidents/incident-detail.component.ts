import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { LocalizationPipe, PermissionDirective, LocalizationService } from '@abp/ng.core';
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
  selector: 'app-incident-detail',
  templateUrl: './incident-detail.component.html',
  imports: [CommonModule, FormsModule, RouterModule, LocalizationPipe, PermissionDirective],
})
export class IncidentDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private incidentService = inject(IncidentService);
  private localization = inject(LocalizationService);

  incident: IncidentDto | null = null;
  newComment = '';
  severityOptions = severityOptions;
  statusOptions = statusOptions;

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.loadIncident(id);
  }

  loadIncident(id: string) {
    this.incidentService.get(id).subscribe(data => {
      this.incident = data;
    });
  }

  resolve() {
    if (!this.incident) return;
    this.incidentService.resolve(this.incident.id).subscribe(data => {
      this.incident = data;
    });
  }

  addComment() {
    if (!this.incident || !this.newComment.trim()) return;
    this.incidentService.addComment(this.incident.id, { content: this.newComment }).subscribe(() => {
      this.newComment = '';
      this.loadIncident(this.incident!.id);
    });
  }

  getSeverityLabel(severity: number): string {
    const key = this.severityOptions.find(o => o.value === severity)?.key;
    return key ? this.localization.instant(key) : this.localization.instant('::Incidents.Unknown');
  }

  getStatusLabel(status: number): string {
    const key = this.statusOptions.find(o => o.value === status)?.key;
    return key ? this.localization.instant(key) : this.localization.instant('::Incidents.Unknown');
  }

  getSeverityClass(severity: number): string {
    switch (severity) {
      case 3: return 'bg-danger';
      case 2: return 'bg-warning text-dark';
      case 1: return 'bg-info';
      default: return 'bg-secondary';
    }
  }

  getKeyPhrasesList(): string[] {
    return this.incident?.keyPhrases?.split(', ').filter(k => k) || [];
  }

  getEntitiesList(): string[] {
    return this.incident?.entities?.split(', ').filter(e => e) || [];
  }
}
