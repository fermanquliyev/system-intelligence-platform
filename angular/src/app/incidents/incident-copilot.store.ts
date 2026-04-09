import { Injectable, inject } from '@angular/core';
import { ComponentStore } from '@ngrx/component-store';
import { exhaustMap, switchMap, tap, catchError, EMPTY } from 'rxjs';
import { AiCopilotService } from '../proxy/ai-copilot/ai-copilot.service';

export interface IncidentCopilotState {
  analysis: Record<string, unknown> | null;
  loading: boolean;
  streaming: boolean;
  error: string | null;
  messages: any[];
  followUpText: string;
}

const initialState: IncidentCopilotState = {
  analysis: null,
  loading: false,
  streaming: false,
  error: null,
  messages: [],
  followUpText: '',
};

@Injectable()
export class IncidentCopilotStore extends ComponentStore<IncidentCopilotState> {
  private copilot = inject(AiCopilotService);

  readonly analysis = this.selectSignal(s => s.analysis);
  readonly loading = this.selectSignal(s => s.loading);
  readonly streaming = this.selectSignal(s => s.streaming);
  readonly error = this.selectSignal(s => s.error);
  readonly messages = this.selectSignal(s => s.messages);
  readonly followUpText = this.selectSignal(s => s.followUpText);

  constructor() {
    super(initialState);
  }

  readonly setFollowUpText = this.updater((s, text: string) => ({ ...s, followUpText: text }));

  readonly loadAnalysis = this.effect<{ incidentId: string }>(origin$ =>
    origin$.pipe(
      tap(() => this.patchState({ loading: true, error: null })),
      switchMap(({ incidentId }) =>
        this.copilot.getAnalysis(incidentId).pipe(
          tap(analysis => this.patchState({ analysis, loading: false })),
          catchError(() => {
            this.patchState({ loading: false, error: 'CopilotError' });
            return EMPTY;
          }),
        ),
      ),
    ),
  );

  readonly loadConversation = this.effect<{ incidentId: string }>(origin$ =>
    origin$.pipe(
      switchMap(({ incidentId }) =>
        this.copilot.getConversation(incidentId).pipe(
          tap(messages => this.patchState({ messages: messages || [] })),
          catchError(() => EMPTY),
        ),
      ),
    ),
  );

  readonly sendFollowUp = this.effect<{ incidentId: string; text: string }>(origin$ =>
    origin$.pipe(
      tap(() => this.patchState({ streaming: true, error: null })),
      exhaustMap(({ incidentId, text }) =>
        this.copilot.postFollowUp(incidentId, { message: text }).pipe(
          tap(() => {
            this.patchState({ streaming: false, followUpText: '' });
            this.loadConversation({ incidentId });
            this.loadAnalysis({ incidentId });
          }),
          catchError(() => {
            this.patchState({ streaming: false, error: 'CopilotError' });
            return EMPTY;
          }),
        ),
      ),
    ),
  );

  reset() {
    this.setState(initialState);
  }
}
