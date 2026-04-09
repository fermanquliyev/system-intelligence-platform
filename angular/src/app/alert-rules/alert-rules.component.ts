import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LocalizationPipe } from '@abp/ng.core';
import { RestService } from '@abp/ng.core';

@Component({
  selector: 'app-alert-rules',
  templateUrl: './alert-rules.component.html',
  imports: [CommonModule, FormsModule, LocalizationPipe],
})
export class AlertRulesComponent implements OnInit {
  private rest = inject(RestService);

  rules: any[] = [];
  history: any[] = [];
  name = '';
  definitionJson = '{"minErrorsLastHour":50}';
  skip = 0;
  take = 20;

  ngOnInit() {
    this.loadRules();
    this.loadHistory();
  }

  loadRules() {
    this.rest
      .request<any, any>({
        method: 'GET',
        url: '/api/app/alert-rule',
        params: { skipCount: this.skip, maxResultCount: this.take },
      }, { apiName: 'Default' })
      .subscribe(r => (this.rules = r.items || []));
  }

  loadHistory() {
    this.rest
      .request<any, any>({
        method: 'GET',
        url: '/api/app/alert-rule/history',
        params: { skipCount: 0, maxResultCount: 50 },
      }, { apiName: 'Default' })
      .subscribe(r => (this.history = r.items || []));
  }

  create() {
    this.rest
      .request<any, any>({
        method: 'POST',
        url: '/api/app/alert-rule',
        body: { name: this.name, definitionJson: this.definitionJson, isEnabled: true },
      }, { apiName: 'Default' })
      .subscribe(() => {
        this.name = '';
        this.loadRules();
      });
  }

  toggle(r: any) {
    this.rest
      .request<any, any>({
        method: 'PUT',
        url: `/api/app/alert-rule/${r.id}/set-enabled`,
        params: { isEnabled: !r.isEnabled },
      }, { apiName: 'Default' })
      .subscribe(() => this.loadRules());
  }

  evaluate() {
    this.rest
      .request<any, any>({
        method: 'POST',
        url: '/api/app/alert-rule/evaluate-now',
        params: { applicationId: undefined },
      }, { apiName: 'Default' })
      .subscribe(() => this.loadHistory());
  }
}
