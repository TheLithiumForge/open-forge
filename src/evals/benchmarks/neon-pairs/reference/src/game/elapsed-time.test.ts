import assert from 'node:assert/strict';
import { test } from 'vitest';
import { formatElapsed } from './elapsed-time.ts';
import { ELAPSED_MAX_MS } from './game.ts';
import { createGame, activateCard, advanceGame } from './game.ts';
import { encodeSnapshot, decodeSnapshot } from './snapshot.ts';
import { EXAMPLE_CODE } from './game.fixture.ts';

test('elapsed cap accepts its endpoint, rejects larger imports and keeps play exportable', () => {
  assert.equal(ELAPSED_MAX_MS, 359_999_999);
  let state = { ...activateCard(createGame(EXAMPLE_CODE), 0), elapsedMs: ELAPSED_MAX_MS };
  assert.deepEqual(decodeSnapshot(encodeSnapshot(state)), state);
  assert.equal(decodeSnapshot(encodeSnapshot({ ...state, elapsedMs: ELAPSED_MAX_MS + 1 })), null);
  state = advanceGame(activateCard(state, 1), Number.MAX_SAFE_INTEGER);
  assert.equal(state.elapsedMs, ELAPSED_MAX_MS, 'R07.9 elapsed time saturates');
  assert.equal(state.moves, 1);
  assert.ok(decodeSnapshot(encodeSnapshot(state)));
  assert.equal(formatElapsed(state.elapsedMs), '5999:59');
});
