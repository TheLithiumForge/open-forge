import { exactKeys, record, safeNonnegative } from './saved-format.ts';
import { MISMATCH_MS } from './game.ts';
import { SelectionKind, type Selection } from './selection.ts';
import type { GameState } from './game.types.ts';

export function readSelection(
  value: unknown,
  state: GameState,
  matched: readonly number[],
): Selection | null {
  if (!record(value)) return null;
  const available = (index: unknown): index is number =>
    safeNonnegative(index) && index < state.deck.length && !matched.includes(index);
  switch (value.kind) {
    case SelectionKind.closed:
      return exactKeys(value, ['kind']) ? { kind: SelectionKind.closed } : null;
    case SelectionKind.one:
      return exactKeys(value, ['kind', 'index']) && available(value.index)
        ? { kind: SelectionKind.one, index: value.index }
        : null;
    case SelectionKind.mismatch:
      if (!exactKeys(value, ['kind', 'first', 'second', 'remainingMs'])) return null;
      if (!available(value.first) || !available(value.second) || value.first === value.second)
        return null;
      if (state.deck[value.first] === state.deck[value.second]) return null;
      if (
        !safeNonnegative(value.remainingMs) ||
        value.remainingMs < 1 ||
        value.remainingMs > MISMATCH_MS
      )
        return null;
      return {
        kind: SelectionKind.mismatch,
        first: value.first,
        second: value.second,
        remainingMs: value.remainingMs,
      };
    default:
      return null;
  }
}

export function readMatched(value: unknown, state: GameState): readonly number[] | null {
  if (!Array.isArray(value)) return null;
  const matched: number[] = [];
  const counts = new Map<number, number>();
  for (const index of value) {
    if (!safeNonnegative(index) || index >= state.deck.length || matched.includes(index))
      return null;
    const face = state.deck[index];
    if (face === undefined) return null;
    matched.push(index);
    counts.set(face, (counts.get(face) ?? 0) + 1);
  }
  return [...counts.values()].every((count) => count === 2) ? matched : null;
}
