import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormGroup, FormBuilder, Validators } from '@angular/forms';
import { NgxDatatableModule } from '@swimlane/ngx-datatable';
import {
  ListService,
  PagedResultDto,
  LocalizationPipe,
  PermissionDirective,
  AutofocusDirective,
} from '@abp/ng.core';
import {
  ConfirmationService,
  Confirmation,
  NgxDatatableDefaultDirective,
  NgxDatatableListDirective,
  ModalCloseDirective,
  ModalComponent,
} from '@abp/ng.theme.shared';
import {
  ApplicationService,
  MonitoredApplicationDto,
  ApiKeyResultDto,
} from '../proxy/applications/application.service';

@Component({
  selector: 'app-applications',
  templateUrl: './applications.component.html',
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    NgxDatatableModule,
    ModalComponent,
    AutofocusDirective,
    NgxDatatableListDirective,
    NgxDatatableDefaultDirective,
    PermissionDirective,
    ModalCloseDirective,
    LocalizationPipe,
  ],
  providers: [ListService],
})
export class ApplicationsComponent implements OnInit {
  readonly list = inject(ListService);
  private appService = inject(ApplicationService);
  private fb = inject(FormBuilder);
  private confirmation = inject(ConfirmationService);

  data = { items: [], totalCount: 0 } as PagedResultDto<MonitoredApplicationDto>;
  form: FormGroup;
  isModalOpen = false;
  selectedApp: MonitoredApplicationDto | null = null;

  apiKeyResult: ApiKeyResultDto | null = null;
  isApiKeyModalOpen = false;

  ngOnInit() {
    this.list.hookToQuery(query => this.appService.getList(query)).subscribe((response: any) => {
      this.data = response;
    });
  }

  createApp() {
    this.selectedApp = null;
    this.buildForm();
    this.isModalOpen = true;
  }

  editApp(id: string) {
    this.appService.get(id).subscribe(app => {
      this.selectedApp = app;
      this.buildForm();
      this.isModalOpen = true;
    });
  }

  delete(id: string) {
    this.confirmation.warn('::AreYouSureToDelete', '::AreYouSure').subscribe(status => {
      if (status === Confirmation.Status.confirm) {
        this.appService.delete(id).subscribe(() => this.list.get());
      }
    });
  }

  regenerateApiKey(id: string) {
    this.confirmation
      .warn('Are you sure you want to regenerate the API key? The old key will stop working.', '::AreYouSure')
      .subscribe(status => {
        if (status === Confirmation.Status.confirm) {
          this.appService.regenerateApiKey(id).subscribe(result => {
            this.apiKeyResult = result;
            this.isApiKeyModalOpen = true;
          });
        }
      });
  }

  buildForm() {
    this.form = this.fb.group({
      name: [this.selectedApp?.name || '', Validators.required],
      description: [this.selectedApp?.description || ''],
      environment: [this.selectedApp?.environment || ''],
      isActive: [this.selectedApp?.isActive ?? true],
    });
  }

  save() {
    if (this.form.invalid) return;

    if (this.selectedApp?.id) {
      this.appService.update(this.selectedApp.id, this.form.value).subscribe(() => {
        this.isModalOpen = false;
        this.list.get();
      });
    } else {
      this.appService.create(this.form.value).subscribe(result => {
        this.isModalOpen = false;
        this.apiKeyResult = result;
        this.isApiKeyModalOpen = true;
        this.list.get();
      });
    }
  }

  copyApiKey() {
    if (this.apiKeyResult?.apiKey) {
      navigator.clipboard.writeText(this.apiKeyResult.apiKey);
    }
  }
}
