import assert from 'node:assert/strict';
import { test } from 'vitest';
import {
  DEFAULT_SIZE,
  PREFERENCES_KEY,
  PREFERENCES_VERSION,
  readPreferences,
  rememberBest,
  writePreferences,
} from './storage.ts';
import { SPEC_VECTOR } from '../game.fixture.ts';
import type { PreferenceStore } from './storage.ts';

function memoryStore(initial = ''): PreferenceStore {
  const entries = new Map([[PREFERENCES_KEY, initial]]);
  return {
    getItem: (key) => entries.get(key) ?? null,
    setItem: (key, value) => {
      entries.set(key, value);
    },
  };
}

test('preferences round trip size and separate minimum moves per size', () => {
  const store = memoryStore();
  const preferences = rememberBest(rememberBest({ size: 6, bestMoves: {} }, 4, 12), 6, 20);
  writePreferences(store, preferences);
  assert.deepEqual(readPreferences(store), preferences, 'R10 scores survive storage round trip');
  assert.equal(rememberBest(preferences, 4, 13), preferences, 'R10 worse score keeps best');
  assert.equal(rememberBest(preferences, 4, 12), preferences);
  assert.equal(rememberBest(preferences, 4, 9).bestMoves['4'], 9, 'R10 better score replaces best');
  assert.equal(preferences.bestMoves['6'], 20, 'R10 size six has its separate best');
});

const INVALID_PREFERENCES = [
  '{',
  'null',
  '{}',
  JSON.stringify({ schemaVersion: PREFERENCES_VERSION, size: 5, bestMoves: {} }),
  JSON.stringify({ schemaVersion: PREFERENCES_VERSION, size: 6, bestMoves: { 4: 1 } }),
  JSON.stringify({ schemaVersion: PREFERENCES_VERSION, size: 6, bestMoves: { 4: -1 } }),
  JSON.stringify({ schemaVersion: PREFERENCES_VERSION, size: 6, bestMoves: { 4: 8.5 } }),
  JSON.stringify({ schemaVersion: PREFERENCES_VERSION, size: 6, bestMoves: { '04': 8 } }),
];
for (const [index, text] of INVALID_PREFERENCES.entries()) {
  test(`preferences ignore invalid saved data ${index + 1}`, () => {
    assert.deepEqual(readPreferences(memoryStore(text)), { size: DEFAULT_SIZE, bestMoves: {} });
  });
}

test('storage failures keep play available and never change unrelated keys', () => {
  const denied: PreferenceStore = {
    getItem: () => {
      throw new Error();
    },
    setItem: () => {
      throw new Error();
    },
  };
  assert.equal(DEFAULT_SIZE, SPEC_VECTOR.sizes[0]);
  assert.equal(readPreferences(denied).size, DEFAULT_SIZE);
  assert.doesNotThrow(() => writePreferences(denied, { size: DEFAULT_SIZE, bestMoves: {} }));
  assert.equal(readPreferences(null).size, DEFAULT_SIZE);
  const store = memoryStore();
  const unrelated = 'another-application';
  store.setItem(unrelated, 'kept');
  writePreferences(store, { size: DEFAULT_SIZE, bestMoves: {} });
  assert.equal(store.getItem(unrelated), 'kept');
});
