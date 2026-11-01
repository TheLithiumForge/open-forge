import assert from 'node:assert/strict';
import { test } from 'node:test';
import { GRID_SIZES, boardCode } from './deck.ts';
import { createGame, activateCard, isWon } from './game.ts';

for (const size of GRID_SIZES) {
  test(`every face pair completes a ${size} by ${size} game in exactly one move per pair`, () => {
    let state = createGame(boardCode(size, 1));
    const pairs = new Map<number, number[]>();
    state.deck.forEach((face, index) => pairs.set(face, [...(pairs.get(face) ?? []), index]));
    for (const pair of pairs.values()) {
      for (const index of pair) state = activateCard(state, index);
    }
    assert.equal(isWon(state), true);
    assert.equal(state.moves, size ** 2 / 2);
    assert.equal(state.matched.length, size ** 2);
  });
}
