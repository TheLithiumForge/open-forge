import { describe, expect, test } from 'vitest';
import { boardCode, generateBoard, nextRandomState, parseBoardCode } from '../deck.ts';
import { GRID_SIZES, SEED_MAX } from '../rules.ts';
import { EXAMPLE_CODE, EXAMPLE_DECK, RANDOM_STATES } from './game-fixture.ts';

describe('deterministic board generation', () => {
  test('matches the independent PRNG and deck vectors', () => {
    let state = 1;
    const actual = RANDOM_STATES.map(() => { state = nextRandomState(state); return state; });
    expect(actual).toEqual(RANDOM_STATES);
    expect(generateBoard(EXAMPLE_CODE).deck).toEqual(EXAMPLE_DECK);
  });

  test.each(GRID_SIZES)('deals exactly two cards per face for size %i', (size) => {
    const board = generateBoard(boardCode(size, SEED_MAX));
    const counts = new Map<number, number>();
    for (const face of board.deck) counts.set(face, (counts.get(face) ?? 0) + 1);
    expect(board.deck).toHaveLength(size ** 2);
    expect(counts.size).toBe(size ** 2 / 2);
    expect([...counts.values()].every((count) => count === 2)).toBe(true);
    expect(generateBoard(board.boardCode)).toEqual(board);
  });

  test.each(['np1-4-0', 'np1-4-01', 'np1-04-1', 'np1-5-1', 'np2-4-1', 'np1-4-2147483647', 'np1-4-1\n'])('rejects invalid code %s', (code) => {
    expect(parseBoardCode(code)).toBeNull();
    expect(() => generateBoard(code)).toThrow();
  });
});
