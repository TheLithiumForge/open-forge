import { createGame, isWon } from './game.ts';
import { parseBoardCode } from './deck.ts';
import { exactKeys, readJson, record } from './saved-format.ts';
import { readMatched, readSelection } from './snapshot-selection.ts';
import { safeNonnegative, SelectionKind, SNAPSHOT_MAX_BYTES, SNAPSHOT_VERSION } from './rules.ts';
import type { GameState } from './game.types.ts';

const SNAPSHOT_KEYS = ['version', 'boardCode', 'matched', 'selection', 'moves', 'elapsedMs', 'started'];

export function encodeSnapshot(state: GameState): string {
  return JSON.stringify({
    version: SNAPSHOT_VERSION,
    boardCode: state.boardCode,
    matched: [...state.matched].sort((left, right) => left - right),
    selection: state.selection,
    moves: state.moves,
    elapsedMs: state.elapsedMs,
    started: state.started,
  }, null, 2);
}

export function decodeSnapshot(text: string): GameState | null {
  const value = readJson(text, SNAPSHOT_MAX_BYTES);
  if (!record(value) || !exactKeys(value, SNAPSHOT_KEYS) || value.version !== SNAPSHOT_VERSION) return null;
  if (typeof value.boardCode !== 'string' || !parseBoardCode(value.boardCode)) return null;
  if (!safeNonnegative(value.moves) || !safeNonnegative(value.elapsedMs) || typeof value.started !== 'boolean') return null;
  const initial = createGame(value.boardCode);
  const matched = readMatched(value.matched, initial);
  if (!matched) return null;
  const selection = readSelection(value.selection, initial, matched);
  if (!selection) return null;
  const state: GameState = { ...initial, matched, selection, moves: value.moves, elapsedMs: value.elapsedMs, started: value.started };
  if (isWon(state) && selection.kind !== SelectionKind.closed) return null;
  const minimumMoves = matched.length / 2 + (selection.kind === SelectionKind.mismatch ? 1 : 0);
  if (state.moves < minimumMoves) return null;
  if (!state.started && (state.moves !== 0 || state.elapsedMs !== 0 || matched.length !== 0 || selection.kind !== SelectionKind.closed)) return null;
  if (state.started && state.moves === 0 && selection.kind === SelectionKind.closed) return null;
  return state;
}
