import type { SelectionKind } from './rules.ts';

export type Selection =
  | { readonly kind: typeof SelectionKind.closed }
  | { readonly kind: typeof SelectionKind.one; readonly index: number }
  | { readonly kind: typeof SelectionKind.mismatch; readonly first: number; readonly second: number; readonly remainingMs: number };

export interface Board {
  readonly boardCode: string;
  readonly size: number;
  readonly seed: number;
  readonly deck: readonly number[];
}

export interface GameState extends Board {
  readonly matched: readonly number[];
  readonly selection: Selection;
  readonly moves: number;
  readonly elapsedMs: number;
  readonly started: boolean;
}
