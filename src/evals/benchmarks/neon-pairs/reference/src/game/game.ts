import { generateBoard } from './deck.ts';
import { SelectionKind } from './selection.ts';
import type { Board } from './deck.ts';
import type { Selection } from './selection.ts';

export const ELAPSED_MAX_MS = 359_999_999;
export const MISMATCH_MS = 800;
export const MOVES_MAX = 999_999;

export interface GameState extends Board {
  readonly matched: readonly number[];
  readonly selection: Selection;
  readonly moves: number;
  readonly elapsedMs: number;
  readonly started: boolean;
}

export function createGame(boardCode: string): GameState {
  return {
    ...generateBoard(boardCode),
    matched: [],
    selection: { kind: SelectionKind.closed },
    moves: 0,
    elapsedMs: 0,
    started: false,
  };
}

export function isWon(state: GameState): boolean {
  return state.matched.length === state.deck.length;
}

// The board reads one set per game state instead of searching the matched list for every card.
export function faceUpCards(state: Pick<GameState, 'matched' | 'selection'>): ReadonlySet<number> {
  const cards = new Set(state.matched);
  const selection = state.selection;
  if (selection.kind === SelectionKind.one) cards.add(selection.index);
  if (selection.kind === SelectionKind.mismatch) {
    cards.add(selection.first);
    cards.add(selection.second);
  }
  return cards;
}

export function activateCard(state: GameState, index: number): GameState {
  if (!Number.isInteger(index) || index < 0 || index >= state.deck.length) return state;
  if (state.matched.includes(index)) return state;
  switch (state.selection.kind) {
    case SelectionKind.closed:
      return { ...state, started: true, selection: { kind: SelectionKind.one, index } };
    case SelectionKind.one: {
      const first = state.selection.index;
      if (first === index) return state;
      const moves = Math.min(MOVES_MAX, state.moves + 1);
      if (state.deck[first] === state.deck[index]) {
        return {
          ...state,
          moves,
          matched: [...state.matched, first, index],
          selection: { kind: SelectionKind.closed },
        };
      }
      return {
        ...state,
        moves,
        selection: { kind: SelectionKind.mismatch, first, second: index, remainingMs: MISMATCH_MS },
      };
    }
    case SelectionKind.mismatch:
      return state;
    default:
      return unreachable(state.selection);
  }
}

export function advanceGame(state: GameState, milliseconds: number): GameState {
  if (!Number.isFinite(milliseconds) || milliseconds <= 0) return state;
  if (!state.started || isWon(state)) return state;
  const elapsedMs = Math.min(ELAPSED_MAX_MS, state.elapsedMs + milliseconds);
  if (state.selection.kind !== SelectionKind.mismatch) return { ...state, elapsedMs };
  const remainingMs = state.selection.remainingMs - milliseconds;
  const selection =
    remainingMs > 0 ? { ...state.selection, remainingMs } : { kind: SelectionKind.closed };
  return { ...state, elapsedMs, selection };
}

function unreachable(value: never): never {
  throw new Error(`Unknown selection: ${String(value)}`);
}
