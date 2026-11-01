import assert from 'node:assert/strict';
import { test } from 'vitest';
import {
  activateCard,
  advanceGame,
  createGame,
  isWon,
  MISMATCH_MS,
  ELAPSED_MAX_MS,
} from './game.ts';
import { SelectionKind } from './selection.ts';
import { EXAMPLE_CODE, EXAMPLE_PAIRS, SPEC_VECTOR } from './game.fixture.ts';
import { formatElapsed } from './elapsed-time.ts';

test('first reveal starts play without a move and ignores invalid activation', () => {
  const start = createGame(EXAMPLE_CODE);
  assert.equal(advanceGame(start, 300), start);
  const first = activateCard(start, 2);
  assert.equal(first.started, true, 'R07.1 first reveal starts time');
  assert.deepEqual(first.selection, { kind: SelectionKind.one, index: 2 });
  for (const index of [2, -1, 16, 0.5, NaN, Infinity])
    assert.equal(activateCard(first, index), first);
  assert.equal(first.moves, 0);
  assert.equal(start.started, false);
});

test('a matching pair stays up and costs exactly one move', () => {
  const result = activateCard(activateCard(createGame(EXAMPLE_CODE), 2), 6);
  assert.deepEqual(result.matched, [2, 6], 'R03.2 matching pair stays matched');
  assert.equal(result.selection.kind, SelectionKind.closed);
  assert.equal(result.moves, 1, 'R05.1 second distinct reveal adds one move');
  assert.equal(activateCard(result, 6), result);
});

test('a mismatch blocks all activation and closes together at the exact interval', () => {
  assert.equal(MISMATCH_MS, SPEC_VECTOR.mismatchMs);
  const mismatch = activateCard(activateCard(createGame(EXAMPLE_CODE), 0), 1);
  assert.equal(mismatch.moves, 1);
  for (const index of [0, 1, 2, 6])
    assert.equal(activateCard(mismatch, index), mismatch, 'R04.3 mismatch blocks activation');
  const pending = advanceGame(mismatch, SPEC_VECTOR.mismatchMs - 1);
  assert.deepEqual(pending.selection, {
    kind: SelectionKind.mismatch,
    first: 0,
    second: 1,
    remainingMs: 1,
  });
  const closed = advanceGame(pending, 1);
  assert.equal(closed.selection.kind, SelectionKind.closed, 'R04.1 mismatch closes at 800ms');
  assert.equal(closed.elapsedMs, SPEC_VECTOR.mismatchMs);
  assert.equal(activateCard(closed, 2).selection.kind, SelectionKind.one);
});

test('completion counts only matched pairs and stops the clock and later activation', () => {
  let state = createGame(EXAMPLE_CODE);
  for (const [first, second] of EXAMPLE_PAIRS) {
    state = activateCard(advanceGame(activateCard(state, first), 10), second);
  }
  assert.equal(isWon(state), true);
  assert.equal(state.matched.length / 2, EXAMPLE_PAIRS.length);
  assert.equal(state.moves, EXAMPLE_PAIRS.length);
  assert.equal(state.elapsedMs, 80);
  assert.equal(advanceGame(state, 100), state);
  assert.equal(activateCard(state, 0), state);
});

test('new states keep earlier games unchanged and reject invalid elapsed input', () => {
  const old = activateCard(activateCard(createGame(EXAMPLE_CODE), 0), 1);
  const current = createGame(EXAMPLE_CODE);
  advanceGame(old, MISMATCH_MS);
  assert.deepEqual(current, createGame(EXAMPLE_CODE));
  for (const delta of [-1, 0.5, Infinity, NaN]) assert.equal(advanceGame(old, delta), old);
  const capped = advanceGame({ ...old, elapsedMs: ELAPSED_MAX_MS - 1 }, Number.MAX_SAFE_INTEGER);
  assert.equal(capped.elapsedMs, ELAPSED_MAX_MS);
  assert.equal(capped.selection.kind, SelectionKind.closed);
  assert.equal(activateCard(capped, 2).selection.kind, SelectionKind.one);
  assert.equal(formatElapsed(capped.elapsedMs), '5999:59');
});

test('one active-time step advances elapsed and mismatch together', () => {
  const old = activateCard(activateCard(createGame(EXAMPLE_CODE), 0), 1);
  const before = advanceGame(old, 799);
  assert.equal(before.elapsedMs, 799);
  assert.deepEqual(before.selection, {
    kind: SelectionKind.mismatch,
    first: 0,
    second: 1,
    remainingMs: 1,
  });
  assert.equal(advanceGame(before, 1).selection.kind, SelectionKind.closed);
});
