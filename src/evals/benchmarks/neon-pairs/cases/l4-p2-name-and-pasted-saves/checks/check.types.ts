import type { Page } from 'playwright';
import type { ChangeType, Kind, Status } from './check-state.ts';

export interface ChangeCheck {
  readonly id: string;
  readonly description: string;
  readonly starting: 'pass' | 'fail';
  readonly action: (page: Page) => Promise<void>;
}
export interface DeclaredCheck { readonly id: string; readonly kind: Kind }
export interface WorkerInput {
  readonly project: string;
  readonly html: string | null;
  readonly report: string;
  readonly checks: readonly DeclaredCheck[];
  readonly changeType: ChangeType;
}
export interface Outcome { readonly id: string; readonly status: Status; readonly evidence: string }
