import assert from 'node:assert/strict';
import { test } from 'node:test';
import { activateCard, advanceGame, createGame, isWon, MISMATCH_MS, MOVES_MAX } from './game.ts';
import { decodeSnapshot, encodeSnapshot } from './snapshot.ts';
import { EXAMPLE_CODE, EXAMPLE_PAIRS } from './game.fixture.ts';
import { SelectionKind } from './selection.ts';
import { activateSession, initialSession, replaceGame } from './session/session.ts';
import {
  PREFERENCES_KEY,
  PREFERENCES_VERSION,
  readPreferences,
  rememberBest,
  writePreferences,
} from './session/storage.ts';

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

test('a capped mismatch closes and larger imported move counts are rejected', () => {
  const game = { ...activateCard(createGame(EXAMPLE_CODE), 0), moves: MOVES_MAX };
  const mismatch = activateCard(game, 1);
  assert.equal(mismatch.selection.kind, SelectionKind.mismatch);
  assert.equal(mismatch.moves, MOVES_MAX);
  const closed = advanceGame(mismatch, MISMATCH_MS);
  assert.equal(closed.selection.kind, SelectionKind.closed);
  assert.ok(decodeSnapshot(encodeSnapshot(closed)));
  for (const moves of [MOVES_MAX + 1, Number.MAX_SAFE_INTEGER])
    assert.equal(decodeSnapshot(encodeSnapshot({ ...game, moves })), null);
});

test('capped wins record bounded bests and invalid saved bests are ignored', () => {
  const preferences = { size: 4, bestMoves: {} };
  const game = { ...createGame(EXAMPLE_CODE), started: true, moves: MOVES_MAX };
  let session = {
    ...replaceGame(initialSession(EXAMPLE_CODE, preferences), game, true),
    paused: false,
  };
  for (const pair of EXAMPLE_PAIRS)
    for (const index of pair) session = activateSession(session, index);
  assert.equal(session.preferences.bestMoves['4'], MOVES_MAX);
  const entries = new Map<string, string>();
  const store = {
    getItem: (key: string) => entries.get(key) ?? null,
    setItem: (key: string, value: string) => {
      entries.set(key, value);
    },
  };
  writePreferences(store, session.preferences);
  assert.deepEqual(readPreferences(store), session.preferences);
  assert.equal(rememberBest(session.preferences, 4, MOVES_MAX), session.preferences);
  store.setItem(
    PREFERENCES_KEY,
    JSON.stringify({
      schemaVersion: PREFERENCES_VERSION,
      size: 4,
      bestMoves: { 4: MOVES_MAX + 1 },
    }),
  );
  assert.deepEqual(readPreferences(store), preferences);
});
