import type { Selection } from './selection.ts';

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
