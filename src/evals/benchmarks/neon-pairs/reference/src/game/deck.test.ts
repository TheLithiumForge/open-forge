import assert from 'node:assert/strict';
import { test } from 'vitest';
import {
  boardCode,
  generateBoard,
  GRID_SIZES,
  nextRandomState,
  parseBoardCode,
  SEED_MAX,
} from './deck.ts';
import { EXAMPLE_CODE, EXAMPLE_DECK, RANDOM_STATES, SPEC_VECTOR } from './game.fixture.ts';

test('generation matches the independent specification vectors', () => {
  let state = 1;
  assert.deepEqual(
    RANDOM_STATES.map(() => {
      state = nextRandomState(state);
      return state;
    }),
    RANDOM_STATES,
    'D03.2 generator follows independent states',
  );
  assert.deepEqual(generateBoard(EXAMPLE_CODE).deck, EXAMPLE_DECK);
  assert.deepEqual(GRID_SIZES, SPEC_VECTOR.sizes);
  assert.equal(SEED_MAX, SPEC_VECTOR.seedMax);
});

test('D01.2 accepts the inclusive maximum seed', () => {
  assert.deepEqual(
    parseBoardCode('np1-4-2147483646'),
    { size: 4, seed: 2147483646 },
    'D01.2 inclusive maximum seed',
  );
});

for (const size of GRID_SIZES) {
  test(`generation deals complete immutable pairs at size ${size}`, () => {
    const board = generateBoard(boardCode(size, SEED_MAX));
    const counts = new Map<number, number>();
    for (const face of board.deck) counts.set(face, (counts.get(face) ?? 0) + 1);
    assert.equal(board.deck.length, size ** 2);
    assert.equal(counts.size, size ** 2 / 2);
    assert.ok([...counts.values()].every((count) => count === 2));
    assert.deepEqual(generateBoard(board.boardCode), board);
    assert.ok(Object.isFrozen(board));
    assert.ok(Object.isFrozen(board.deck));
  });
}

const INVALID_CODES = [
  'np1-4-0',
  'np1-4-01',
  'np1-04-1',
  'np1-5-1',
  'np2-4-1',
  'np1-4-2147483647',
  'np1-4-1\n',
  'np1-4-1\r',
  ' np1-4-1',
  'np1-4-1 ',
  'np1-4-1e2',
  'np1-4-+1',
  'np1-4-1-extra',
  'np1-4-9007199254740992',
];
for (const [index, code] of INVALID_CODES.entries()) {
  test(`board-code validation rejects invalid form ${index + 1}`, () => {
    assert.equal(parseBoardCode(code), null);
    assert.throws(() => generateBoard(code));
  });
}
