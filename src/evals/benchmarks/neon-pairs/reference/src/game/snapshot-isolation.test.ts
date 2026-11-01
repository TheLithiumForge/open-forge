import assert from 'node:assert/strict';
import { test } from 'vitest';
import { decodeSnapshot } from './snapshot.ts';

const ONE = {
  version: 1,
  boardCode: 'np1-4-1',
  matched: [],
  moves: 0,
  elapsedMs: 0,
  started: true,
  selection: { kind: 'one', index: 0 },
};
const MISMATCH = {
  ...ONE,
  moves: 1,
  selection: { kind: 'mismatch', first: 0, second: 1, remainingMs: 800 },
};
function read(value: unknown) {
  return decodeSnapshot(JSON.stringify(value));
}

test('D05 D05 D06 reject extra selection keys in otherwise valid states', () => {
  const closed = { ...ONE, moves: 1, selection: { kind: 'closed' } };
  for (const valid of [closed, ONE, MISMATCH]) {
    assert.ok(read(valid), 'valid selection baseline');
    assert.equal(
      read({ ...valid, selection: { ...valid.selection, extra: true } }),
      null,
      'extra selection key',
    );
  }
});

test('D06 rejects duplicate indices without a second rejection reason', () => {
  const valid = { ...ONE, matched: [2, 6], moves: 1 };
  assert.ok(read(valid), 'valid whole pair baseline');
  assert.equal(read({ ...valid, matched: [2, 2] }), null, 'duplicate matched position');
});

test('D06 accepts both remaining-time endpoints and rejects an in-range fraction', () => {
  for (const remainingMs of [1, 800])
    assert.ok(
      read({ ...MISMATCH, selection: { ...MISMATCH.selection, remainingMs } }),
      'remaining endpoint',
    );
  assert.equal(
    read({ ...MISMATCH, selection: { ...MISMATCH.selection, remainingMs: 1.5 } }),
    null,
    'fractional remaining time',
  );
});

test('D07 rejects a nonboolean started flag with valid started progress', () => {
  assert.ok(read(ONE), 'valid started baseline');
  assert.equal(read({ ...ONE, started: 'true' }), null, 'nonboolean started');
});

test('D07 rejects negative and fractional counters independently of started consistency', () => {
  for (const field of ['moves', 'elapsedMs'])
    for (const value of [-1, 0.5]) {
      assert.equal(
        read({ ...ONE, [field]: value }),
        null,
        `${field} must be a nonnegative integer`,
      );
    }
});

test('D07 rejects insufficient moves independently of the closed zero-move rule', () => {
  assert.ok(read(MISMATCH), 'valid mismatch baseline');
  assert.equal(read({ ...MISMATCH, moves: 0 }), null, 'pending mismatch needs one move');
  const valid = { ...ONE, matched: [2, 6, 0, 11], moves: 2, selection: { kind: 'one', index: 3 } };
  assert.ok(read(valid), 'valid two-pair baseline');
  assert.equal(read({ ...valid, moves: 1 }), null, 'matched pairs need two moves');
});
