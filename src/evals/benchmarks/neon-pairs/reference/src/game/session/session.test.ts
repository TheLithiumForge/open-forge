import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import { test } from 'node:test';
import { activateCard, createGame, isWon, MISMATCH_MS } from '../game.ts';
import { boardCode } from '../deck.ts';
import { activateSession, initialSession, replaceGame, tickSession } from './session.ts';
import { EXAMPLE_CODE, EXAMPLE_PAIRS } from '../game.fixture.ts';

const EMPTY_PREFERENCES = { size: 4, bestMoves: {} };

test('session generation rejects earlier callbacks after replacing a game', () => {
  const first = activateSession(
    activateSession(initialSession(EXAMPLE_CODE, EMPTY_PREFERENCES), 0),
    1,
  );
  const restarted = replaceGame(first, createGame(EXAMPLE_CODE), false);
  assert.equal(tickSession(restarted, first.generation, MISMATCH_MS), restarted);
  assert.equal(restarted.game.moves, 0);
  assert.equal(restarted.game.started, false);
  const active = activateSession(restarted, 2);
  assert.equal(tickSession(active, first.generation, MISMATCH_MS), active);
  const changed = replaceGame(first, createGame(boardCode(6, 1)), false);
  assert.equal(changed.preferences.size, 6);
});

test('an imported session blocks input and active time until Resume', () => {
  const first = activateSession(
    activateSession(initialSession(EXAMPLE_CODE, EMPTY_PREFERENCES), 0),
    1,
  );
  const paused = replaceGame(first, first.game, true);
  assert.equal(activateSession(paused, 2), paused);
  assert.equal(tickSession(paused, paused.generation, MISMATCH_MS), paused);
  assert.equal(paused.celebration, false);
  const pausedOne = replaceGame(paused, activateCard(createGame(EXAMPLE_CODE), 0), true);
  assert.equal(activateSession(pausedOne, 2), pausedOne);
  const resumed = { ...paused, paused: false };
  assert.notEqual(
    tickSession(resumed, resumed.generation, MISMATCH_MS).game.selection.kind,
    first.game.selection.kind,
  );
});

test('turn announcements happen once and repeat identical results as separate events', () => {
  const initial = initialSession(EXAMPLE_CODE, EMPTY_PREFERENCES);
  const first = activateSession(initial, 0);
  assert.equal(first.announcement, initial.announcement);
  const mismatch = activateSession(first, 1);
  assert.equal(mismatch.announcement, initial.announcement + 1);
  const closed = tickSession(mismatch, mismatch.generation, MISMATCH_MS);
  assert.equal(closed.announcement, mismatch.announcement);
  assert.equal(closed.feedback, mismatch.feedback);
  const repeated = activateSession(activateSession(closed, 0), 1);
  assert.equal(repeated.feedback, mismatch.feedback);
  assert.equal(repeated.announcement, mismatch.announcement + 1);
});

test('a new completion records bests but completed imports never do', () => {
  let session = initialSession(EXAMPLE_CODE, EMPTY_PREFERENCES);
  for (const pair of EXAMPLE_PAIRS)
    for (const index of pair) session = activateSession(session, index);
  assert.equal(isWon(session.game), true);
  assert.equal(session.preferences.bestMoves['4'], EXAMPLE_PAIRS.length);
  assert.equal(session.celebration, true);
  assert.equal(session.announcement, EXAMPLE_PAIRS.length);
  const imported = replaceGame(initialSession(EXAMPLE_CODE, EMPTY_PREFERENCES), session.game, true);
  assert.deepEqual(imported.preferences.bestMoves, {});
  assert.deepEqual(activateSession({ ...imported, paused: false }, 0).preferences.bestMoves, {});
  const pending = {
    ...session.game,
    matched: EXAMPLE_PAIRS.slice(0, -1).flat(),
    moves: EXAMPLE_PAIRS.length - 1,
  };
  let restored = { ...replaceGame(imported, pending, true), paused: false };
  for (const index of EXAMPLE_PAIRS[EXAMPLE_PAIRS.length - 1] ?? [])
    restored = activateSession(restored, index);
  assert.equal(restored.preferences.bestMoves['4'], EXAMPLE_PAIRS.length);
});

test('turn feedback matches the reviewed message snapshot', async () => {
  const initial = initialSession(EXAMPLE_CODE, EMPTY_PREFERENCES);
  const mismatch = activateSession(activateSession(initial, 0), 1);
  const matched = activateSession(activateSession(initial, 2), 6);
  let won = initial;
  for (const pair of EXAMPLE_PAIRS) for (const index of pair) won = activateSession(won, index);
  const expected: unknown = JSON.parse(
    await readFile(new URL('./session.feedback.fixture.json', import.meta.url), 'utf8'),
  );
  assert.deepEqual(
    {
      initial: initial.feedback,
      mismatch: mismatch.feedback,
      matched: matched.feedback,
      won: won.feedback,
    },
    expected,
  );
});
