import type { Page } from 'playwright';
import type { ChangeType, Kind, Status } from './check-state.ts';
import type { SessionOptions } from './browser.ts';

interface CheckBase {
  readonly id: string;
  readonly description: string;
}
export interface BrowserCheck extends CheckBase {
  readonly session?: SessionOptions;
  readonly action: (page: Page) => Promise<void>;
}
export interface SourceCheck extends CheckBase {
  readonly source: (project: string) => Promise<void>;
}
export type ChangeCheck = BrowserCheck | SourceCheck;
export interface DeclaredCheck { readonly id: string; readonly kind: Kind }
export interface WorkerInput {
  readonly project: string;
  readonly html: string | null;
  readonly report: string;
  readonly checks: readonly DeclaredCheck[];
  readonly changeType: ChangeType;
}
export interface Outcome { readonly id: string; readonly status: Status; readonly evidence: string }
