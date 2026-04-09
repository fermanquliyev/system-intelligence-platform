import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LocalizationPipe, PermissionDirective, RestService } from '@abp/ng.core';

@Component({
  selector: 'app-playbooks-page',
  templateUrl: './playbooks-page.component.html',
  imports: [CommonModule, FormsModule, LocalizationPipe, PermissionDirective],
})
export class PlaybooksPageComponent implements OnInit {
  private rest = inject(RestService);

  playbooks: any[] = [];
  name = '';
  triggerJson = '{}';
  stepTitle = 'Check dashboards';
  stepBody = 'Open service health view.';
  incidentId = '';
  selectedPlaybookId = '';
  activeRun: any = null;
  error = '';

  ngOnInit() {
    this.loadPlaybooks();
  }

  loadPlaybooks() {
    this.rest
      .request<any, any>(
        { method: 'GET', url: '/api/app/playbook', params: { skipCount: 0, maxResultCount: 100 } },
        { apiName: 'Default' },
      )
      .subscribe({
        next: r => (this.playbooks = r.items || []),
        error: e => (this.error = e?.error?.error?.message || e.message || 'Request failed'),
      });
  }

  create() {
    this.error = '';
    this.rest
      .request<any, any>(
        {
          method: 'POST',
          url: '/api/app/playbook',
          body: {
            name: this.name,
            description: null,
            triggerDefinitionJson: this.triggerJson,
            steps: [{ sortOrder: 0, title: this.stepTitle, body: this.stepBody }],
          },
        },
        { apiName: 'Default' },
      )
      .subscribe({
        next: () => {
          this.name = '';
          this.loadPlaybooks();
        },
        error: e => (this.error = e?.error?.error?.message || e.message || 'Request failed'),
      });
  }

  delete(id: string) {
    if (!confirm('Delete?')) return;
    this.rest
      .request<any, void>({ method: 'DELETE', url: `/api/app/playbook/${id}` }, { apiName: 'Default' })
      .subscribe(() => this.loadPlaybooks());
  }

  run() {
    this.error = '';
    if (!this.selectedPlaybookId || !this.incidentId) return;
    this.rest
      .request<any, any>(
        {
          method: 'POST',
          url: '/api/app/playbook/run-for-incident',
          params: { playbookId: this.selectedPlaybookId, incidentId: this.incidentId },
        },
        { apiName: 'Default' },
      )
      .subscribe({
        next: run => (this.activeRun = run),
        error: e => (this.error = e?.error?.error?.message || e.message || 'Request failed'),
      });
  }

  loadActiveRun() {
    this.error = '';
    if (!this.incidentId) return;
    this.rest
      .request<any, any>(
        {
          method: 'GET',
          url: '/api/app/playbook/active-run',
          params: { incidentId: this.incidentId },
        },
        { apiName: 'Default' },
      )
      .subscribe({
        next: run => (this.activeRun = run),
        error: () => (this.activeRun = null),
      });
  }

  completeStep(stepOrder: number) {
    if (!this.activeRun?.id) return;
    this.rest
      .request<any, any>(
        {
          method: 'POST',
          url: '/api/app/playbook/complete-run-step',
          params: { runId: this.activeRun.id, stepOrder },
        },
        { apiName: 'Default' },
      )
      .subscribe(run => (this.activeRun = run));
  }
}
