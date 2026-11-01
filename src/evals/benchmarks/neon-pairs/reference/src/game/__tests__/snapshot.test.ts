import { describe, expect, test } from 'vitest';
import { activateCard, advanceGame, createGame } from '../game.ts';
import { decodeSnapshot, encodeSnapshot } from '../snapshot.ts';
import { MISMATCH_MS, SelectionKind, SNAPSHOT_MAX_BYTES, SNAPSHOT_VERSION } from '../rules.ts';
import { EXAMPLE_CODE, EXAMPLE_PAIRS } from './game-fixture.ts';

function snapshot(overrides: Record<string, unknown> = {}): string {
  return JSON.stringify({ version: SNAPSHOT_VERSION, boardCode: EXAMPLE_CODE, matched: [],
    selection: { kind: SelectionKind.closed }, moves: 0, elapsedMs: 0, started: false, ...overrides });
}

describe('snapshot validation', () => {
  test('round trips a pending mismatch with active time', () => {
    const state = advanceGame(activateCard(activateCard(createGame(EXAMPLE_CODE), 0), 1), 175);
    expect(decodeSnapshot(encodeSnapshot(state))).toEqual(state);
    expect(state.selection).toEqual({ kind: SelectionKind.mismatch, first: 0, second: 1, remainingMs: MISMATCH_MS - 175 });
  });

  test('accepts complete face pairs and a winning closed board', () => {
    const state = activateCard(activateCard(createGame(EXAMPLE_CODE), 6), 2);
    const restored = decodeSnapshot(encodeSnapshot(state));
    expect(restored?.matched).toEqual([2, 6]);
    expect(decodeSnapshot(snapshot({ matched: EXAMPLE_PAIRS.flat(), moves: EXAMPLE_PAIRS.length, started: true }))?.matched).toHaveLength(16);
  });

  test.each([
    { extra: true }, { version: 2 }, { boardCode: 'np1-4-0' }, { elapsedMs: -1 }, { elapsedMs: 0.5 },
    { moves: Number.MAX_SAFE_INTEGER + 1 }, { started: 'true' }, { matched: [0] }, { matched: [0, 1] },
    { matched: [2, 6, 2, 6] }, { matched: [2, 16] }, { matched: [2, 6], started: true, moves: 0 },
    { selection: { kind: SelectionKind.one, index: 16 }, started: true },
    { selection: { kind: SelectionKind.one, index: 2, extra: true }, started: true },
    { selection: { kind: SelectionKind.one, index: 2 }, started: false },
    { started: true }, { moves: 1 }, { elapsedMs: 1 }, { selection: { kind: 'unknown' } },
    { selection: { kind: SelectionKind.mismatch, first: 2, second: 6, remainingMs: 1 }, started: true, moves: 1 },
    { selection: { kind: SelectionKind.mismatch, first: 0, second: 0, remainingMs: 1 }, started: true, moves: 1 },
    { selection: { kind: SelectionKind.mismatch, first: 0, second: 1, remainingMs: 0 }, started: true, moves: 1 },
    { selection: { kind: SelectionKind.mismatch, first: 0, second: 1, remainingMs: MISMATCH_MS + 1 }, started: true, moves: 1 },
    { matched: [2, 6], selection: { kind: SelectionKind.one, index: 2 }, started: true, moves: 1 },
  ])('rejects invalid snapshot %#', (overrides) => {
    expect(decodeSnapshot(snapshot(overrides))).toBeNull();
  });

  test('rejects malformed, missing, oversized and multibyte JSON', () => {
    for (const text of ['null', '[]', '{}', '{', ' '.repeat(SNAPSHOT_MAX_BYTES + 1), `"${'é'.repeat(SNAPSHOT_MAX_BYTES / 2)}"`]) {
      expect(decodeSnapshot(text)).toBeNull();
    }
  });
});
