import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LocalizationPipe } from '@abp/ng.core';
import { RouterModule } from '@angular/router';
import { IncidentService } from '../proxy/incidents/incident.service';

@Component({
  selector: 'app-global-timeline',
  templateUrl: './global-timeline.component.html',
  imports: [CommonModule, FormsModule, LocalizationPipe, RouterModule],
})
export class GlobalTimelineComponent implements OnInit {
  private incidentService = inject(IncidentService);

  items: any[] = [];
  totalCount = 0;
  skipCount = 0;
  maxResultCount = 20;
  applicationId = '';
  severity: number | null = null;
  timeScale = 1;

  ngOnInit() {
    this.load();
  }

  load() {
    this.incidentService
      .getGlobalTimeline({
        skipCount: this.skipCount,
        maxResultCount: this.maxResultCount,
        applicationId: this.applicationId || undefined,
        severity: this.severity ?? undefined,
      })
      .subscribe(res => {
        this.items = res.items || [];
        this.totalCount = res.totalCount || 0;
      });
  }

  zoom(f: number) {
    this.timeScale = Math.max(0.5, Math.min(3, this.timeScale * f));
  }
}
