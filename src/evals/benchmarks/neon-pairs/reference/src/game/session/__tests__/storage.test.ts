import { describe, expect, test } from 'vitest';
import { DEFAULT_SIZE } from '../../rules.ts';
import { PREFERENCES_KEY, PREFERENCES_VERSION, readPreferences, rememberBest, writePreferences } from '../storage.ts';
import type { PreferenceStore } from '../storage.ts';

function memoryStore(initial = ''): PreferenceStore {
  const entries = new Map([[PREFERENCES_KEY, initial]]);
  return { getItem: (key) => entries.get(key) ?? null, setItem: (key, value) => { entries.set(key, value); } };
}

describe('guarded preferences', () => {
  test('round trips size and independently tracked minimum moves', () => {
    const store = memoryStore();
    const preferences = rememberBest(rememberBest({ size: 6, bestMoves: {} }, 4, 12), 6, 20);
    writePreferences(store, preferences);
    expect(readPreferences(store)).toEqual(preferences);
    expect(rememberBest(preferences, 4, 13)).toBe(preferences);
    expect(rememberBest(preferences, 4, 9).bestMoves['4']).toBe(9);
  });

  test.each(['{', 'null', '{}', JSON.stringify({ schemaVersion: PREFERENCES_VERSION, size: 5, bestMoves: {} }),
    JSON.stringify({ schemaVersion: PREFERENCES_VERSION, size: 6, bestMoves: { 4: 1 } }),
    JSON.stringify({ schemaVersion: PREFERENCES_VERSION, size: 6, bestMoves: { 4: -1 } })])('falls back from invalid stored data %#', (text) => {
    expect(readPreferences(memoryStore(text))).toEqual({ size: DEFAULT_SIZE, bestMoves: {} });
  });

  test('survives denied storage without a real user store', () => {
    const denied: PreferenceStore = { getItem: () => { throw new Error(); }, setItem: () => { throw new Error(); } };
    expect(readPreferences(denied).size).toBe(DEFAULT_SIZE);
    expect(() => writePreferences(denied, { size: DEFAULT_SIZE, bestMoves: {} })).not.toThrow();
    expect(readPreferences(null).size).toBe(DEFAULT_SIZE);
  });
});
