import { ProgressLimit } from './game.ts';

// Reuse the version 1 state rules with either snapshot envelope.
export const invalidProgress: readonly Record<string, unknown>[] = [
  { boardCode: 'np1-4-0' }, { elapsedMs: -1, started: true, moves: 1 }, { elapsedMs: 0.5, started: true, moves: 1 },
  { elapsedMs: ProgressLimit.elapsedMs + 1, started: true, moves: 1 },
  { moves: Number.MAX_SAFE_INTEGER + 1 }, { moves: -1 },
  { moves: ProgressLimit.moves + 1, started: true }, { started: 'true', selection: { kind: 'one', index: 0 } },
  { matched: [0], started: true, moves: 1 }, { matched: [0, 1], started: true, moves: 1 },
  { matched: [2, 6, 2, 6], started: true, moves: 2 }, { matched: [2, 16], started: true, moves: 1 },
  { matched: [2, 2], started: true, moves: 1 },
  { matched: [2, 6], started: true, moves: 0 },
  { selection: { kind: 'one', index: 16 }, started: true },
  { selection: { kind: 'one', index: 2, extra: true }, started: true },
  { selection: { kind: 'closed', extra: true }, started: true, moves: 1 },
  { selection: { kind: 'mismatch', first: 0, second: 1, remainingMs: 1, extra: true }, started: true, moves: 1 },
  { selection: { kind: 'one', index: 2 }, started: false },
  { started: true }, { moves: 1 }, { elapsedMs: 1 }, { selection: { kind: 'unknown' } },
  { selection: { kind: 'mismatch', first: 2, second: 6, remainingMs: 1 }, started: true, moves: 1 },
  { selection: { kind: 'mismatch', first: 0, second: 0, remainingMs: 1 }, started: true, moves: 1 },
  { selection: { kind: 'mismatch', first: 0, second: 1, remainingMs: 0 }, started: true, moves: 1 },
  { selection: { kind: 'mismatch', first: 0, second: 1, remainingMs: 1.5 }, started: true, moves: 1 },
  { selection: { kind: 'mismatch', first: 0, second: 1, remainingMs: ProgressLimit.mismatchMs + 1 }, started: true, moves: 1 },
  { matched: [2, 6], selection: { kind: 'one', index: 2 }, started: true, moves: 1 },
];
