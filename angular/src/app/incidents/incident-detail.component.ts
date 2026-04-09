import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { LocalizationPipe, PermissionDirective, LocalizationService } from '@abp/ng.core';
import { IncidentService } from '../proxy/incidents/incident.service';
import { IncidentCopilotStore } from './incident-copilot.store';

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
  providers: [IncidentCopilotStore],
})
export class IncidentDetailComponent implements OnInit, OnDestroy {
  private route = inject(ActivatedRoute);
  private incidentService = inject(IncidentService);
  private localization = inject(LocalizationService);
  readonly copilotStore = inject(IncidentCopilotStore);

  incident: IncidentDto | null = null;
  newComment = '';
  severityOptions = severityOptions;
  statusOptions = statusOptions;
  timelineItems: any[] = [];
  mergedChildren: any[] = [];
  assignUserId = '';
  timelineScale = 1;

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.loadIncident(id);
  }

  ngOnDestroy() {
    this.copilotStore.reset();
  }

  loadIncident(id: string) {
    this.incidentService.get(id).subscribe(data => {
      this.incident = data;
      this.copilotStore.loadAnalysis({ incidentId: id });
      this.copilotStore.loadConversation({ incidentId: id });
      this.incidentService.getRootCauseTimeline(id).subscribe({
        next: t => (this.timelineItems = t || []),
        error: () => (this.timelineItems = []),
      });
      this.incidentService.getMergedChildren(id).subscribe({
        next: m => (this.mergedChildren = m || []),
        error: () => (this.mergedChildren = []),
      });
    });
  }

  resolve() {
    if (!this.incident) return;
    this.incidentService.resolve(this.incident.id).subscribe(data => {
      this.incident = data;
    });
  }

  updateStatus(status: number) {
    if (!this.incident) return;
    this.incidentService.updateStatus(this.incident.id, status as IncidentStatus).subscribe(data => {
      this.incident = data;
    });
  }

  assign() {
    if (!this.incident) return;
    const uid = this.assignUserId?.trim();
    this.incidentService
      .assign(this.incident.id, { userId: uid || null })
      .subscribe(data => {
        this.incident = data;
        this.assignUserId = '';
      });
  }

  addComment() {
    if (!this.incident || !this.newComment.trim()) return;
    this.incidentService.addComment(this.incident.id, { content: this.newComment }).subscribe(() => {
      this.newComment = '';
      this.loadIncident(this.incident!.id);
    });
  }

  refreshCopilot() {
    if (!this.incident) return;
    this.copilotStore.loadAnalysis({ incidentId: this.incident.id });
  }

  submitFollowUp() {
    if (!this.incident) return;
    const text = this.copilotStore.followUpText();
    if (!text?.trim()) return;
    this.copilotStore.sendFollowUp({ incidentId: this.incident.id, text: text.trim() });
  }

  zoomTimeline(factor: number) {
    this.timelineScale = Math.max(0.5, Math.min(3, this.timelineScale * factor));
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
      case 3:
        return 'bg-danger';
      case 2:
        return 'bg-warning text-dark';
      case 1:
        return 'bg-info';
      default:
        return 'bg-secondary';
    }
  }

  getKeyPhrasesList(): string[] {
    return this.incident?.keyPhrases?.split(', ').filter(k => k) || [];
  }

  getEntitiesList(): string[] {
    return this.incident?.entities?.split(', ').filter(e => e) || [];
  }
}
