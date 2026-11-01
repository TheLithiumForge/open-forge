import { DEFAULT_SIZE, safeNonnegative, validSize } from '../rules.ts';
import { record } from '../saved-format.ts';

export const PREFERENCES_KEY = 'neon-pairs.preferences.v1';
export const PREFERENCES_VERSION = 1;
export interface Preferences { readonly size: number; readonly bestMoves: Readonly<Record<string, number>> }
export interface PreferenceStore { getItem(key: string): string | null; setItem(key: string, value: string): void }

export function readPreferences(store: PreferenceStore | null): Preferences {
  const fallback: Preferences = { size: DEFAULT_SIZE, bestMoves: {} };
  try {
    const text = store?.getItem(PREFERENCES_KEY);
    if (!text) return fallback;
    const value: unknown = JSON.parse(text);
    if (!record(value) || value.schemaVersion !== PREFERENCES_VERSION || typeof value.size !== 'number') return fallback;
    if (!validSize(value.size) || !record(value.bestMoves)) return fallback;
    const bestMoves: Record<string, number> = {};
    for (const [size, moves] of Object.entries(value.bestMoves)) {
      const numberSize = Number(size);
      if (String(numberSize) !== size || !validSize(numberSize) || !safeNonnegative(moves) || moves < numberSize ** 2 / 2) return fallback;
      bestMoves[size] = moves;
    }
    return { size: value.size, bestMoves };
  } catch {
    return fallback;
  }
}

export function writePreferences(store: PreferenceStore | null, preferences: Preferences): void {
  try {
    store?.setItem(PREFERENCES_KEY, JSON.stringify({ schemaVersion: PREFERENCES_VERSION, ...preferences }));
  } catch {
    // A blocked browser store doesn't stop the current game.
  }
}

export function rememberBest(preferences: Preferences, size: number, moves: number): Preferences {
  const previous = preferences.bestMoves[String(size)];
  if (previous !== undefined && previous <= moves) return preferences;
  return { ...preferences, bestMoves: { ...preferences.bestMoves, [size]: moves } };
}
