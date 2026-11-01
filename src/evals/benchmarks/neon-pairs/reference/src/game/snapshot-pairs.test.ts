import assert from 'node:assert/strict';
import { test } from 'vitest';
import { activateCard, createGame } from './game.ts';
import { decodeSnapshot, encodeSnapshot } from './snapshot.ts';
import { EXAMPLE_CODE } from './game.fixture.ts';

test('matched indices need whole pairs even when every other field is consistent', () => {
  const valid = activateCard(activateCard(createGame(EXAMPLE_CODE), 0), 11);
  assert.ok(decodeSnapshot(encodeSnapshot(valid)));
  assert.equal(
    decodeSnapshot(encodeSnapshot({ ...valid, matched: [0] })),
    null,
    'D06.3 half pair is invalid',
  );
});
