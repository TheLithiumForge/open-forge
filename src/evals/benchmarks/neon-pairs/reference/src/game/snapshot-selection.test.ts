import assert from 'node:assert/strict';
import { test } from 'vitest';
import { createGame, MISMATCH_MS } from './game.ts';
import { readMatched, readSelection } from './snapshot-selection.ts';
import { SelectionKind } from './selection.ts';
import { EXAMPLE_CODE } from './game.fixture.ts';

// In the example deal, cards 0 and 1 differ and cards 2 and 6 form a pair.
const GAME = createGame(EXAMPLE_CODE);
const MISMATCH = {
  kind: SelectionKind.mismatch,
  first: 0,
  second: 1,
  remainingMs: MISMATCH_MS,
};

test('selections reject an extra key that is otherwise valid', () => {
  const closed = { kind: SelectionKind.closed };
  const one = { kind: SelectionKind.one, index: 0 };
  for (const valid of [closed, one, MISMATCH]) {
    assert.ok(readSelection(valid, GAME, []), 'valid selection baseline');
    assert.equal(readSelection({ ...valid, extra: true }, GAME, []), null, 'extra selection key');
  }
});

test('a pending mismatch accepts both remaining-time endpoints and rejects a fraction', () => {
  for (const remainingMs of [1, MISMATCH_MS])
    assert.ok(readSelection({ ...MISMATCH, remainingMs }, GAME, []), 'remaining endpoint');
  const fraction = { ...MISMATCH, remainingMs: 1.5 };
  assert.equal(readSelection(fraction, GAME, []), null, 'fractional remaining time');
});

test('matched positions need whole pairs without duplicates', () => {
  assert.deepEqual(readMatched([2, 6], GAME), [2, 6], 'valid whole pair baseline');
  assert.equal(readMatched([0], GAME), null, 'D06 half pair is invalid');
  assert.equal(readMatched([2, 2], GAME), null, 'duplicate matched position');
});
