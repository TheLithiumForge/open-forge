import assert from 'node:assert/strict';
import { test } from 'vitest';
import { activateCard, advanceGame, createGame, MISMATCH_MS } from './game.ts';
import {
  decodeSnapshot,
  encodeSnapshot,
  SNAPSHOT_KEYS,
  SNAPSHOT_MAX_BYTES,
  SNAPSHOT_VERSION,
} from './snapshot.ts';
import { SelectionKind } from './selection.ts';
import { EXAMPLE_CODE, EXAMPLE_PAIRS, SPEC_VECTOR } from './game.fixture.ts';

function snapshot(overrides: Record<string, unknown> = {}): string {
  return JSON.stringify({
    version: SNAPSHOT_VERSION,
    boardCode: EXAMPLE_CODE,
    matched: [],
    selection: { kind: SelectionKind.closed },
    moves: 0,
    elapsedMs: 0,
    started: false,
    ...overrides,
  });
}

test('snapshot round trips preserve pending time and serialize only the declared fields', () => {
  const state = advanceGame(activateCard(activateCard(createGame(EXAMPLE_CODE), 0), 1), 175);
  assert.deepEqual(decodeSnapshot(encodeSnapshot(state)), state);
  assert.deepEqual(state.selection, {
    kind: SelectionKind.mismatch,
    first: 0,
    second: 1,
    remainingMs: MISMATCH_MS - 175,
  });
  assert.equal(SNAPSHOT_VERSION, SPEC_VECTOR.snapshotVersion);
  assert.equal(SNAPSHOT_MAX_BYTES, SPEC_VECTOR.snapshotBytes);
  const exported: unknown = JSON.parse(encodeSnapshot(state));
  assert.ok(exported && typeof exported === 'object');
  assert.deepEqual(Object.keys(exported).sort(), [...SNAPSHOT_KEYS].sort());
});

test('snapshots accept whole pairs and completed states without changing the source order', () => {
  const state = activateCard(activateCard(createGame(EXAMPLE_CODE), 6), 2);
  assert.deepEqual(decodeSnapshot(encodeSnapshot(state))?.matched, [2, 6]);
  assert.deepEqual(state.matched, [6, 2]);
  assert.equal(
    decodeSnapshot(
      snapshot({ matched: EXAMPLE_PAIRS.flat(), moves: EXAMPLE_PAIRS.length, started: true }),
    )?.matched.length,
    16,
  );
  assert.ok(decodeSnapshot(snapshot({ moves: 17, started: true })));
});

const INVALID_SNAPSHOTS: readonly Record<string, unknown>[] = [
  { extra: true },
  { version: 2 },
  { boardCode: 'np1-4-0' },
  { elapsedMs: -1 },
  { elapsedMs: 0.5 },
  { moves: Number.MAX_SAFE_INTEGER + 1 },
  { moves: -1 },
  { started: 'true' },
  { matched: [0] },
  { matched: [0, 1] },
  { matched: [2, 6, 2, 6] },
  { matched: [2, 16] },
  { matched: [2, 6], started: true, moves: 0 },
  { selection: { kind: SelectionKind.one, index: 16 }, started: true },
  { selection: { kind: SelectionKind.one, index: 2, extra: true }, started: true },
  { selection: { kind: SelectionKind.one, index: 2 }, started: false },
  { started: true },
  { moves: 1 },
  { elapsedMs: 1 },
  { selection: { kind: 'unknown' } },
  {
    selection: { kind: SelectionKind.mismatch, first: 2, second: 6, remainingMs: 1 },
    started: true,
    moves: 1,
  },
  {
    selection: { kind: SelectionKind.mismatch, first: 0, second: 0, remainingMs: 1 },
    started: true,
    moves: 1,
  },
  {
    selection: { kind: SelectionKind.mismatch, first: 0, second: 1, remainingMs: 0 },
    started: true,
    moves: 1,
  },
  {
    selection: { kind: SelectionKind.mismatch, first: 0, second: 1, remainingMs: MISMATCH_MS + 1 },
    started: true,
    moves: 1,
  },
  { matched: [2, 6], selection: { kind: SelectionKind.one, index: 2 }, started: true, moves: 1 },
];
for (const [index, overrides] of INVALID_SNAPSHOTS.entries()) {
  test(`snapshot validation rejects inconsistent state ${index + 1}`, () => {
    assert.equal(
      decodeSnapshot(snapshot(overrides)),
      null,
      `snapshot rejects isolated field case ${index + 1}`,
    );
  });
}

test('snapshot parsing enforces malformed, missing and exact byte limits', () => {
  for (const text of ['null', '[]', '{}', '{']) assert.equal(decodeSnapshot(text), null);
  const text = snapshot();
  const padded = text + ' '.repeat(SNAPSHOT_MAX_BYTES - Buffer.byteLength(text));
  assert.ok(decodeSnapshot(padded));
  assert.equal(decodeSnapshot(padded + ' '), null);
});
