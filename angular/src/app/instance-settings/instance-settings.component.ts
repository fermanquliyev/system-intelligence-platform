import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LocalizationPipe, LocalizationService, PermissionDirective } from '@abp/ng.core';
import { Confirmation, ConfirmationService } from '@abp/ng.theme.shared';
import { InstanceConfigurationService } from '../proxy/instance-configuration/instance-configuration.service';

@Component({
  selector: 'app-instance-settings',
  templateUrl: './instance-settings.component.html',
  styleUrl: './instance-settings.component.scss',
  imports: [CommonModule, FormsModule, PermissionDirective, LocalizationPipe],
})
export class InstanceSettingsComponent implements OnInit {
  private service = inject(InstanceConfigurationService);
  private confirmation = inject(ConfirmationService);
  private localization = inject(LocalizationService);

  snapshot: InstanceConfigurationSnapshotDto | null = null;
  /** Precomputed in load(); do not call a method from the template (causes change-detection thrash / freeze). */
  groupedSettings: { category: string; items: InstanceSettingStateDto[] }[] = [];
  featureStates: Record<string, boolean> = {};
  settingEdits: Record<string, string> = {};
  saving = false;
  migrateMessage: string | null = null;
  migrateBusy = false;
  loadError: string | null = null;
  loading = true;

  ngOnInit() {
    this.load();
  }

  load() {
    this.loading = true;
    this.loadError = null;
    this.service.getSnapshot().subscribe({
      next: data => {
        this.loading = false;
        if (!data || !Array.isArray(data.features) || !Array.isArray(data.settings)) {
          this.loadError = this.localization.instant('::InstanceSettings.InvalidResponseFromServer');
          this.snapshot = null;
          this.groupedSettings = [];
          return;
        }
        this.snapshot = data;
        this.featureStates = {};
        for (const f of data.features) {
          this.featureStates[f.name] = f.isEnabled;
        }
        this.settingEdits = {};
        for (const s of data.settings) {
          this.settingEdits[s.key] = s.isSecret ? '' : (s.effectiveDisplayValue ?? '');
        }
        this.groupedSettings = this.buildGroupedSettings(data.settings);
      },
      error: err => {
        this.loading = false;
        this.snapshot = null;
        this.groupedSettings = [];
        this.loadError =
          err?.error?.error?.message ?? err?.message ?? this.localization.instant('::InstanceSettings.FailedToLoadInstanceConfiguration');
      },
    });
  }

  private buildGroupedSettings(settings: InstanceSettingStateDto[]) {
    const map = new Map<string, InstanceSettingStateDto[]>();
    for (const s of settings) {
      const c = s.category || this.localization.instant('::InstanceSettings.Other');
      if (!map.has(c)) map.set(c, []);
      map.get(c)!.push(s);
    }
    return [...map.entries()]
      .sort(([a], [b]) => a.localeCompare(b))
      .map(([category, items]) => ({ category, items }));
  }

  saveFeatures() {
    this.saving = true;
    this.service.updateFeatures({ features: { ...this.featureStates } }).subscribe({
      next: () => this.load(),
      error: () => (this.saving = false),
      complete: () => (this.saving = false),
    });
  }

  saveSettings() {
    const values: Record<string, string | null> = {};
    for (const s of this.snapshot?.settings ?? []) {
      const v = this.settingEdits[s.key];
      if (s.isSecret) {
        if (v != null && String(v).trim() !== '') values[s.key] = String(v).trim();
      } else {
        values[s.key] = v == null ? '' : String(v).trim();
      }
    }
    this.saving = true;
    this.service.updateSettings({ values }).subscribe({
      next: () => this.load(),
      error: () => (this.saving = false),
      complete: () => (this.saving = false),
    });
  }

  applyMigrations() {
    this.confirmation.warn('::InstanceSettings.ApplyMigrationsConfirmationMessage', '::AreYouSure').subscribe(status => {
      if (status !== Confirmation.Status.confirm) return;

      this.migrateBusy = true;
      this.migrateMessage = null;
      this.service.applyMigrations().subscribe({
        next: r => {
          this.migrateMessage = r.success
            ? (r.message ?? this.localization.instant('::InstanceSettings.MigrationsCompleted'))
            : (r.message ?? this.localization.instant('::InstanceSettings.MigrationsFailed'));
          this.migrateBusy = false;
        },
        error: err => {
          this.migrateMessage =
            err?.error?.error?.message ?? err?.message ?? this.localization.instant('::InstanceSettings.RequestFailed');
          this.migrateBusy = false;
        },
      });
    });
  }

  trackFeature(_i: number, f: InstanceFeatureStateDto) {
    return f.id;
  }

  categoryLabel(category: string): string {
    if (category === 'MinIO') {
      return this.localization.instant('::InstanceSettings.CategoryMinIO');
    }
    return category;
  }

  isMinioUseSslKey(key: string): boolean {
    return key === 'Minio:UseSsl';
  }

  minioUseSslChecked(s: InstanceSettingStateDto): boolean {
    const raw = this.settingEdits[s.key];
    if (raw != null && String(raw).trim() !== '') {
      return this.parseTruthyString(String(raw));
    }
    return this.parseTruthyString(s.effectiveDisplayValue ?? '');
  }

  setMinioUseSsl(key: string, checked: boolean) {
    this.settingEdits[key] = checked ? 'true' : 'false';
  }

  private parseTruthyString(v: string): boolean {
    const t = String(v).trim().toLowerCase();
    return t === 'true' || t === '1' || t === 'yes';
  }
}
