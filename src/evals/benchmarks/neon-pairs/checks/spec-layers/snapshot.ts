import { object } from "./requirements.ts";
import { expectedDeck, parseBoardCode } from "./board-code.ts";
import { Rules, SelectionKind, type Selection, type Snapshot } from "./rules.ts";

export function exactKeys(value: Record<string, unknown>, keys: readonly string[]): boolean {
  return Object.keys(value).length === keys.length && keys.every((key) => Object.hasOwn(value, key));
}
export function nonnegativeInteger(value: unknown): value is number {
  return typeof value === "number" && Number.isSafeInteger(value) && value >= 0;
}

function selection(value: unknown, count: number, matched: readonly number[], deck: readonly number[]): Selection | null {
  if (!object(value)) return null;
  const eligible = (index: unknown): index is number => nonnegativeInteger(index) && index < count && !matched.includes(index);
  if (value["kind"] === SelectionKind.closed && exactKeys(value, ["kind"])) return { kind: SelectionKind.closed };
  if (value["kind"] === SelectionKind.one && exactKeys(value, ["kind", "index"]) && eligible(value["index"])) return { kind: SelectionKind.one, index: value["index"] };
  const first = value["first"], second = value["second"], remainingMs = value["remainingMs"];
  if (value["kind"] !== SelectionKind.mismatch || !exactKeys(value, ["kind", "first", "second", "remainingMs"]) || !eligible(first) || !eligible(second) || first === second || deck[first] === deck[second] || !nonnegativeInteger(remainingMs) || remainingMs < 1 || remainingMs > Rules.mismatchMs) return null;
  return { kind: SelectionKind.mismatch, first, second, remainingMs };
}

export function decodeSnapshot(input: unknown): Snapshot | null {
  if (typeof input !== "string" || Buffer.byteLength(input) > Rules.snapshotBytes) return null;
  let value: unknown;
  try { value = JSON.parse(input); } catch { return null; }
  if (!object(value) || !exactKeys(value, ["version", "boardCode", "matched", "selection", "moves", "elapsedMs", "started"]) || value["version"] !== Rules.formatVersion || typeof value["boardCode"] !== "string" || !parseBoardCode(value["boardCode"]) || !Array.isArray(value["matched"])) return null;
  const deck = expectedDeck(value["boardCode"]);
  const matched = value["matched"];
  if (!matched.every((index: unknown): index is number => nonnegativeInteger(index) && index < deck.length) || new Set(matched).size !== matched.length) return null;
  const faces = new Map<number, number>();
  for (const index of matched) { const face = deck[index]; if (face === undefined) return null; faces.set(face, (faces.get(face) ?? 0) + 1); }
  if ([...faces.values()].some((count) => count !== 2)) return null;
  const chosen = selection(value["selection"], deck.length, matched, deck);
  const moves = value["moves"], elapsedMs = value["elapsedMs"], started = value["started"];
  if (!chosen || !nonnegativeInteger(moves) || moves > Rules.movesMaximum || !nonnegativeInteger(elapsedMs) || elapsedMs > Rules.elapsedMaximumMs || typeof started !== "boolean" || moves < matched.length / 2 + (chosen.kind === SelectionKind.mismatch ? 1 : 0)) return null;
  if (matched.length === deck.length && chosen.kind !== SelectionKind.closed) return null;
  if (!started && (moves !== 0 || elapsedMs !== 0 || matched.length !== 0 || chosen.kind !== SelectionKind.closed)) return null;
  if (started && moves === 0 && chosen.kind !== SelectionKind.one) return null;
  return { version: Rules.formatVersion, boardCode: value["boardCode"], matched, selection: chosen, moves, elapsedMs, started };
}

export function snapshotsEqual(first: Snapshot, second: Snapshot): boolean {
  const a = first.selection, b = second.selection;
  const sameSelection = a.kind === SelectionKind.closed ? b.kind === SelectionKind.closed
    : a.kind === SelectionKind.one ? b.kind === SelectionKind.one && a.index === b.index
      : b.kind === SelectionKind.mismatch && a.first === b.first && a.second === b.second && a.remainingMs === b.remainingMs;
  return first.boardCode === second.boardCode && first.version === second.version && first.moves === second.moves && first.elapsedMs === second.elapsedMs && first.started === second.started
    && JSON.stringify([...first.matched].sort((a, b) => a - b)) === JSON.stringify([...second.matched].sort((a, b) => a - b)) && sameSelection;
}
