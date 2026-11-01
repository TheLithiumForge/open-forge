import assert from 'node:assert/strict';
import { test } from 'vitest';
import { activateCard, advanceGame, createGame, MISMATCH_MS, MOVES_MAX } from './game.ts';
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
  { moves: MOVES_MAX + 1, started: true },
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

// Started play with one revealed card is valid at zero moves, so each variant breaks one rule.
const ONE_REVEALED = { started: true, selection: { kind: SelectionKind.one, index: 0 } };
const PENDING = {
  started: true,
  moves: 1,
  selection: { kind: SelectionKind.mismatch, first: 0, second: 1, remainingMs: MISMATCH_MS },
};

test('started progress rejects a nonboolean flag and negative or fractional counters', () => {
  assert.ok(decodeSnapshot(snapshot(ONE_REVEALED)), 'valid started baseline');
  const nonboolean = snapshot({ ...ONE_REVEALED, started: 'true' });
  assert.equal(decodeSnapshot(nonboolean), null, 'nonboolean started');
  for (const field of ['moves', 'elapsedMs'])
    for (const value of [-1, 0.5])
      assert.equal(
        decodeSnapshot(snapshot({ ...ONE_REVEALED, [field]: value })),
        null,
        `${field} must be a nonnegative integer`,
      );
});

test('progress needs one move per matched pair and one for a pending mismatch', () => {
  assert.ok(decodeSnapshot(snapshot(PENDING)), 'valid mismatch baseline');
  const unpaid = snapshot({ ...PENDING, moves: 0 });
  assert.equal(decodeSnapshot(unpaid), null, 'pending mismatch needs one move');
  const twoPairs = {
    ...ONE_REVEALED,
    matched: [2, 6, 0, 11],
    moves: 2,
    selection: { kind: SelectionKind.one, index: 3 },
  };
  assert.ok(decodeSnapshot(snapshot(twoPairs)), 'valid two-pair baseline');
  const short = snapshot({ ...twoPairs, moves: 1 });
  assert.equal(decodeSnapshot(short), null, 'matched pairs need two moves');
});

// A natural clock reveals the second card 10.9ms after the first and exports 0.2ms later.
const BEFORE_MISMATCH_MS = 10.9;
const INTO_MISMATCH_MS = 0.2;

test('export rounds fractional time down and a mismatch from its own activation up', () => {
  const first = activateCard(createGame(EXAMPLE_CODE), 0);
  const mismatch = activateCard(advanceGame(first, BEFORE_MISMATCH_MS), 1);
  const exported: unknown = JSON.parse(encodeSnapshot(advanceGame(mismatch, INTO_MISMATCH_MS)));
  assert.deepEqual(exported, {
    version: SNAPSHOT_VERSION,
    boardCode: EXAMPLE_CODE,
    matched: [],
    selection: { kind: SelectionKind.mismatch, first: 0, second: 1, remainingMs: MISMATCH_MS },
    moves: 1,
    elapsedMs: Math.floor(BEFORE_MISMATCH_MS + INTO_MISMATCH_MS),
    started: true,
  });
  const almost = advanceGame(mismatch, MISMATCH_MS - INTO_MISMATCH_MS);
  assert.equal(
    almost.selection.kind,
    SelectionKind.mismatch,
    'C08 the mismatch runs its full interval',
  );
  assert.equal(advanceGame(mismatch, MISMATCH_MS).selection.kind, SelectionKind.closed);
});

test('snapshot parsing enforces malformed, missing and exact byte limits', () => {
  for (const text of ['null', '[]', '{}', '{']) assert.equal(decodeSnapshot(text), null);
  const text = snapshot();
  const padded = text + ' '.repeat(SNAPSHOT_MAX_BYTES - Buffer.byteLength(text));
  assert.ok(decodeSnapshot(padded));
  assert.equal(decodeSnapshot(padded + ' '), null);
});
