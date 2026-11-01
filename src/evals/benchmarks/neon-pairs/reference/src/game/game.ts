import { generateBoard } from './deck.ts';
import { MISMATCH_MS, safeNonnegative, SelectionKind } from './rules.ts';
import type { GameState } from './game.types.ts';

export function createGame(boardCode: string): GameState {
  return { ...generateBoard(boardCode), matched: [], selection: { kind: SelectionKind.closed }, moves: 0, elapsedMs: 0, started: false };
}

export function isWon(state: GameState): boolean {
  return state.matched.length === state.deck.length;
}

export function activateCard(state: GameState, index: number): GameState {
  if (!safeNonnegative(index) || index >= state.deck.length || state.matched.includes(index) || isWon(state)) return state;
  switch (state.selection.kind) {
    case SelectionKind.closed:
      return { ...state, started: true, selection: { kind: SelectionKind.one, index } };
    case SelectionKind.one: {
      const first = state.selection.index;
      if (first === index || !Number.isSafeInteger(state.moves + 1)) return state;
      const moves = state.moves + 1;
      if (state.deck[first] === state.deck[index]) {
        return { ...state, moves, matched: [...state.matched, first, index], selection: { kind: SelectionKind.closed } };
      }
      return { ...state, moves, selection: { kind: SelectionKind.mismatch, first, second: index, remainingMs: MISMATCH_MS } };
    }
    case SelectionKind.mismatch:
      return state;
    default:
      return unreachable(state.selection);
  }
}

export function advanceGame(state: GameState, milliseconds: number): GameState {
  if (!safeNonnegative(milliseconds) || milliseconds === 0 || !state.started || isWon(state)) return state;
  const elapsedMs = state.elapsedMs + milliseconds;
  if (!Number.isSafeInteger(elapsedMs)) return state;
  if (state.selection.kind !== SelectionKind.mismatch) return { ...state, elapsedMs };
  const remainingMs = state.selection.remainingMs - milliseconds;
  const selection = remainingMs > 0
    ? { ...state.selection, remainingMs }
    : { kind: SelectionKind.closed };
  return { ...state, elapsedMs, selection };
}

function unreachable(value: never): never {
  throw new Error(`Unknown selection: ${String(value)}`);
}
