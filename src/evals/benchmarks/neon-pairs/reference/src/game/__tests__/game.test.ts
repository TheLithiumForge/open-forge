import { describe, expect, test } from 'vitest';
import { activateCard, advanceGame, createGame, isWon } from '../game.ts';
import { MISMATCH_MS, SelectionKind } from '../rules.ts';
import { EXAMPLE_CODE, EXAMPLE_PAIRS } from './game-fixture.ts';

describe('card transitions', () => {
  test('reveals a first card and ignores repeated or invalid activation', () => {
    const start = createGame(EXAMPLE_CODE);
    expect(advanceGame(start, 300)).toBe(start);
    const first = activateCard(start, 2);
    expect(first.started).toBe(true);
    expect(first.selection).toEqual({ kind: SelectionKind.one, index: 2 });
    for (const index of [2, -1, 16, 0.5, NaN]) expect(activateCard(first, index)).toBe(first);
    expect(first.moves).toBe(0);
    expect(start.started).toBe(false);
  });

  test('matches permanently and counts a single move', () => {
    const result = activateCard(activateCard(createGame(EXAMPLE_CODE), 2), 6);
    expect(result.matched).toEqual([2, 6]);
    expect(result.selection.kind).toBe(SelectionKind.closed);
    expect(result.moves).toBe(1);
    expect(activateCard(result, 6)).toBe(result);
  });

  test('locks a mismatch until exactly 800 active milliseconds', () => {
    const mismatch = activateCard(activateCard(createGame(EXAMPLE_CODE), 0), 1);
    expect(mismatch.moves).toBe(1);
    expect(activateCard(mismatch, 2)).toBe(mismatch);
    const pending = advanceGame(mismatch, MISMATCH_MS - 1);
    expect(pending.selection).toEqual({ kind: SelectionKind.mismatch, first: 0, second: 1, remainingMs: 1 });
    const closed = advanceGame(pending, 1);
    expect(closed.selection.kind).toBe(SelectionKind.closed);
    expect(closed.elapsedMs).toBe(MISMATCH_MS);
    expect(activateCard(closed, 2).selection.kind).toBe(SelectionKind.one);
  });

  test('derives victory and freezes the clock', () => {
    let state = createGame(EXAMPLE_CODE);
    for (const [first, second] of EXAMPLE_PAIRS) {
      state = activateCard(advanceGame(activateCard(state, first), 10), second);
    }
    expect(isWon(state)).toBe(true);
    expect(state.moves).toBe(EXAMPLE_PAIRS.length);
    expect(state.elapsedMs).toBe(80);
    expect(advanceGame(state, 100)).toBe(state);
    expect(activateCard(state, 0)).toBe(state);
  });

  test('keeps a new game independent from old timing and ignores unsafe advances', () => {
    const old = activateCard(activateCard(createGame(EXAMPLE_CODE), 0), 1);
    const current = createGame(EXAMPLE_CODE);
    advanceGame(old, MISMATCH_MS);
    expect(current).toEqual(createGame(EXAMPLE_CODE));
    for (const delta of [-1, 0.5, Infinity]) expect(advanceGame(old, delta)).toBe(old);
    const nearLimit = { ...old, elapsedMs: Number.MAX_SAFE_INTEGER };
    expect(advanceGame(nearLimit, 1)).toBe(nearLimit);
  });
});
