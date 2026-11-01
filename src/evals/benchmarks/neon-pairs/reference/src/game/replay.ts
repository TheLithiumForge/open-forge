import { parseBoardCode } from './deck.ts';
import { activateCard, advanceGame, createGame } from './game.ts';
import {
  hasExactKeys,
  isJsonObject,
  isNonnegativeSafeInteger,
  isWithinBytes,
  parseJson,
} from './saved-format.ts';
import type { GameState } from './game.ts';

export const REPLAY_VERSION = 1;
export const REPLAY_MAX_BYTES = 1_048_576;
export const REPLAY_MAX_ACTIONS = 10_000;
const REPLAY_KEYS = ['version', 'boardCode', 'actions', 'endMs'];
const ACTION_KEYS = ['atMs', 'index'];

export const ReplayProblem = {
  tooLarge: 'tooLarge',
  malformedJson: 'malformedJson',
  format: 'format',
  boardCode: 'boardCode',
  actions: 'actions',
  endTime: 'endTime',
  action: 'action',
  order: 'order',
  index: 'index',
} as const;
export type ReplayProblemKind = (typeof ReplayProblem)[keyof typeof ReplayProblem];
type ReplayResult =
  { readonly state: GameState } | { readonly problem: ReplayProblemKind; readonly action?: number };

export function readReplay(text: string): ReplayResult {
  if (!isWithinBytes(text, REPLAY_MAX_BYTES)) return { problem: ReplayProblem.tooLarge };
  const value = parseJson(text);
  if (value === undefined) return { problem: ReplayProblem.malformedJson };
  if (!isJsonObject(value) || !hasExactKeys(value, REPLAY_KEYS) || value.version !== REPLAY_VERSION)
    return { problem: ReplayProblem.format };
  if (typeof value.boardCode !== 'string' || !parseBoardCode(value.boardCode))
    return { problem: ReplayProblem.boardCode };
  if (!Array.isArray(value.actions) || value.actions.length > REPLAY_MAX_ACTIONS)
    return { problem: ReplayProblem.actions };
  if (!isNonnegativeSafeInteger(value.endMs)) return { problem: ReplayProblem.endTime };
  let state = createGame(value.boardCode);
  let previousTime = 0;
  for (const [position, action] of value.actions.entries()) {
    if (
      !isJsonObject(action) ||
      !hasExactKeys(action, ACTION_KEYS) ||
      !isNonnegativeSafeInteger(action.atMs) ||
      !isNonnegativeSafeInteger(action.index)
    )
      return { problem: ReplayProblem.action, action: position };
    if (action.atMs < previousTime || action.atMs > value.endMs)
      return { problem: ReplayProblem.order, action: position };
    if (action.index >= state.deck.length)
      return { problem: ReplayProblem.index, action: position };
    state = activateCard(advanceGame(state, action.atMs - previousTime), action.index);
    previousTime = action.atMs;
  }
  return { state: advanceGame(state, value.endMs - previousTime) };
}

export function replayGame(text: string): GameState | null {
  const result = readReplay(text);
  return 'state' in result ? result.state : null;
}
