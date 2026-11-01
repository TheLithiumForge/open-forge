import assert from 'node:assert/strict';
import { test } from 'vitest';
import { activateCard, advanceGame, createGame, isWon, MISMATCH_MS, MOVES_MAX } from './game.ts';
import { decodeSnapshot, encodeSnapshot } from './snapshot.ts';
import { EXAMPLE_CODE, EXAMPLE_PAIRS } from './game.fixture.ts';
import { SelectionKind } from './selection.ts';

test('moves reach the cap once and matching, completion and snapshot export continue', () => {
  assert.equal(MOVES_MAX, 999_999);
  let game = { ...activateCard(createGame(EXAMPLE_CODE), 0), moves: MOVES_MAX - 1 };
  game = activateCard(game, 11);
  assert.equal(game.moves, MOVES_MAX);
  assert.equal(game.matched.length, 2);
  for (const pair of EXAMPLE_PAIRS) for (const index of pair) game = activateCard(game, index);
  assert.equal(isWon(game), true);
  assert.equal(game.moves, MOVES_MAX);
  const restored = decodeSnapshot(encodeSnapshot(game));
  assert.ok(restored && isWon(restored));
  assert.equal(restored.moves, MOVES_MAX);
});

test('a capped mismatch keeps its move count and still closes and exports', () => {
  const game = { ...activateCard(createGame(EXAMPLE_CODE), 0), moves: MOVES_MAX };
  const mismatch = activateCard(game, 1);
  assert.equal(mismatch.selection.kind, SelectionKind.mismatch);
  assert.equal(mismatch.moves, MOVES_MAX, 'R05 move count saturates');
  const closed = advanceGame(mismatch, MISMATCH_MS);
  assert.equal(closed.selection.kind, SelectionKind.closed);
  assert.ok(decodeSnapshot(encodeSnapshot(closed)));
});
