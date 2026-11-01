import { GRID_SIZES, isValidSize } from '../deck.ts';
import { MOVES_MAX } from '../game.ts';
import { isJsonObject, isNonnegativeSafeInteger, parseJson } from '../saved-format.ts';

export const DEFAULT_SIZE = GRID_SIZES[0];
export const PREFERENCES_KEY = 'neon-pairs.preferences.v1';
export const PREFERENCES_VERSION = 1;
export interface Preferences {
  readonly size: number;
  readonly bestMoves: Readonly<Record<string, number>>;
}
export interface PreferenceStore {
  getItem(key: string): string | null;
  setItem(key: string, value: string): void;
}

export function readPreferences(store: PreferenceStore | null): Preferences {
  const fallback: Preferences = { size: DEFAULT_SIZE, bestMoves: {} };
  try {
    const text = store?.getItem(PREFERENCES_KEY);
    if (!text) return fallback;
    const value = parseJson(text);
    if (
      !isJsonObject(value) ||
      value.schemaVersion !== PREFERENCES_VERSION ||
      typeof value.size !== 'number'
    )
      return fallback;
    if (!isValidSize(value.size) || !isJsonObject(value.bestMoves)) return fallback;
    const bestMoves: Record<string, number> = {};
    for (const [size, moves] of Object.entries(value.bestMoves)) {
      const numberSize = Number(size);
      if (
        String(numberSize) !== size ||
        !isValidSize(numberSize) ||
        !isNonnegativeSafeInteger(moves) ||
        moves > MOVES_MAX ||
        moves < numberSize ** 2 / 2
      )
        return fallback;
      bestMoves[size] = moves;
    }
    return { size: value.size, bestMoves };
  } catch {
    return fallback;
  }
}

// Another tab may have saved a better score since this one loaded. Keeps this tab's size and
// the better best for each size.
function mergeSavedBests(store: PreferenceStore | null, preferences: Preferences): Preferences {
  let merged = preferences;
  for (const [size, moves] of Object.entries(readPreferences(store).bestMoves))
    merged = rememberBest(merged, Number(size), moves);
  return merged;
}

export function writePreferences(store: PreferenceStore | null, preferences: Preferences): void {
  const merged = mergeSavedBests(store, preferences);
  try {
    store?.setItem(
      PREFERENCES_KEY,
      JSON.stringify({ schemaVersion: PREFERENCES_VERSION, ...merged }),
    );
  } catch {
    // A blocked browser store doesn't stop the current game.
  }
}

export function rememberBest(preferences: Preferences, size: number, moves: number): Preferences {
  const previous = preferences.bestMoves[String(size)];
  if (previous !== undefined && previous <= moves) return preferences;
  return { ...preferences, bestMoves: { ...preferences.bestMoves, [size]: moves } };
}
