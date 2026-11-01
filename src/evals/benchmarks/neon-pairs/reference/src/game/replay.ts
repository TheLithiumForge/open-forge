import { parseBoardCode } from './deck.ts';
import { activateCard, advanceGame, createGame } from './game.ts';
import { exactKeys, readJson, record, safeNonnegative } from './saved-format.ts';
import type { GameState } from './game.types.ts';

export const REPLAY_VERSION = 1;
export const REPLAY_MAX_BYTES = 1_048_576;
export const REPLAY_MAX_ACTIONS = 10_000;
const REPLAY_KEYS = ['version', 'boardCode', 'actions', 'endMs'];
const ACTION_KEYS = ['atMs', 'index'];

export function replayGame(text: string): GameState | null {
  const value = readJson(text, REPLAY_MAX_BYTES);
  if (!record(value) || !exactKeys(value, REPLAY_KEYS) || value.version !== REPLAY_VERSION)
    return null;
  if (typeof value.boardCode !== 'string' || !parseBoardCode(value.boardCode)) return null;
  if (
    !Array.isArray(value.actions) ||
    value.actions.length > REPLAY_MAX_ACTIONS ||
    !safeNonnegative(value.endMs)
  )
    return null;
  let state = createGame(value.boardCode);
  let previousTime = 0;
  for (const action of value.actions) {
    if (!record(action) || !exactKeys(action, ACTION_KEYS)) return null;
    if (!safeNonnegative(action.atMs) || !safeNonnegative(action.index)) return null;
    if (
      action.atMs < previousTime ||
      action.atMs > value.endMs ||
      action.index >= state.deck.length
    )
      return null;
    state = activateCard(advanceGame(state, action.atMs - previousTime), action.index);
    previousTime = action.atMs;
  }
  return advanceGame(state, value.endMs - previousTime);
}
