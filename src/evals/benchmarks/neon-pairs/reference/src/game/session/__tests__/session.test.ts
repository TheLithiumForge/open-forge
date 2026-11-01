import { describe, expect, test } from 'vitest';
import { createGame, isWon } from '../../game.ts';
import { MISMATCH_MS } from '../../rules.ts';
import { activateSession, initialSession, replaceGame, tickSession } from '../session.ts';
import { elapsedStep } from '../browser-clock.ts';
import { EXAMPLE_CODE, EXAMPLE_PAIRS } from '../../__tests__/game-fixture.ts';

const EMPTY_PREFERENCES = { size: 4, bestMoves: {} };

describe('session lifecycle', () => {
  test('rejects old-generation clock callbacks after restart', () => {
    const first = activateSession(activateSession(initialSession(EXAMPLE_CODE, EMPTY_PREFERENCES), 0), 1);
    const restarted = replaceGame(first, createGame(EXAMPLE_CODE), false);
    expect(tickSession(restarted, first.generation, MISMATCH_MS)).toBe(restarted);
    expect(restarted.game.moves).toBe(0);
    expect(restarted.game.started).toBe(false);
  });

  test('keeps imported state paused until resume and clears celebration', () => {
    const first = activateSession(activateSession(initialSession(EXAMPLE_CODE, EMPTY_PREFERENCES), 0), 1);
    const paused = replaceGame(first, first.game, true);
    expect(activateSession(paused, 2)).toBe(paused);
    expect(tickSession(paused, paused.generation, MISMATCH_MS)).toBe(paused);
    expect(paused.celebration).toBe(false);
    const resumed = { ...paused, paused: false };
    expect(tickSession(resumed, resumed.generation, MISMATCH_MS).game.selection.kind).not.toBe(first.game.selection.kind);
  });

  test('records best moves only on a new completed transition', () => {
    let session = initialSession(EXAMPLE_CODE, EMPTY_PREFERENCES);
    for (const pair of EXAMPLE_PAIRS) for (const index of pair) session = activateSession(session, index);
    expect(isWon(session.game)).toBe(true);
    expect(session.preferences.bestMoves['4']).toBe(8);
    expect(session.celebration).toBe(true);
    const imported = replaceGame(initialSession(EXAMPLE_CODE, EMPTY_PREFERENCES), session.game, true);
    expect(imported.preferences.bestMoves).toEqual({});
    expect(activateSession({ ...imported, paused: false }, 0).preferences.bestMoves).toEqual({});
  });

  test('preserves sub-millisecond remainder and ignores backwards clocks', () => {
    expect(elapsedStep(10.5, 15.9)).toEqual({ milliseconds: 5, next: 15.5 });
    expect(elapsedStep(15.5, 15)).toEqual({ milliseconds: 0, next: 15 });
  });
});
