import { object } from "./requirements.ts";
import { exactKeys, nonnegativeInteger } from "./snapshot.ts";
import { expectedDeck, parseBoardCode } from "./board-code.ts";
import { Rules, SelectionKind, type Replay, type Snapshot } from "./rules.ts";

export function parseReplay(input: unknown): Replay | null {
  if (typeof input !== "string" || Buffer.byteLength(input) > Rules.replayBytes) return null;
  let value: unknown;
  try { value = JSON.parse(input); } catch { return null; }
  if (!object(value) || !exactKeys(value, ["version", "boardCode", "actions", "endMs"]) || value["version"] !== Rules.formatVersion || typeof value["boardCode"] !== "string" || !parseBoardCode(value["boardCode"]) || !Array.isArray(value["actions"]) || value["actions"].length > Rules.actionsMaximum || !nonnegativeInteger(value["endMs"])) return null;
  const count = expectedDeck(value["boardCode"]).length;
  const actions: { atMs: number; index: number }[] = [];
  let previous = 0;
  for (const action of value["actions"]) {
    if (!object(action) || !exactKeys(action, ["atMs", "index"]) || !nonnegativeInteger(action["atMs"]) || action["atMs"] < previous || !nonnegativeInteger(action["index"]) || action["index"] >= count) return null;
    previous = action["atMs"];
    actions.push({ atMs: action["atMs"], index: action["index"] });
  }
  return value["endMs"] < previous ? null : { version: Rules.formatVersion, boardCode: value["boardCode"], actions, endMs: value["endMs"] };
}

export function freshSnapshot(boardCode: string): Snapshot {
  if (!parseBoardCode(boardCode)) throw new Error("Invalid oracle board.");
  return { version: Rules.formatVersion, boardCode, matched: [], selection: { kind: SelectionKind.closed }, moves: 0, elapsedMs: 0, started: false };
}

export function advance(state: Snapshot, milliseconds: number): Snapshot | null {
  if (!nonnegativeInteger(milliseconds) || !nonnegativeInteger(state.elapsedMs) || state.elapsedMs > Rules.elapsedMaximumMs) return null;
  const won = state.matched.length === expectedDeck(state.boardCode).length;
  const active = state.started && !won ? Math.min(milliseconds, Rules.elapsedMaximumMs - state.elapsedMs) : 0;
  const elapsedMs = state.elapsedMs + active;
  const chosen = state.selection;
  if (chosen.kind !== SelectionKind.mismatch) return { ...state, elapsedMs };
  return { ...state, elapsedMs, selection: milliseconds >= chosen.remainingMs ? { kind: SelectionKind.closed } : { ...chosen, remainingMs: chosen.remainingMs - milliseconds } };
}

export function activate(state: Snapshot, index: number): Snapshot {
  const deck = expectedDeck(state.boardCode);
  const chosen = state.selection;
  if (!nonnegativeInteger(index) || index >= deck.length || state.matched.includes(index) || chosen.kind === SelectionKind.mismatch || (chosen.kind === SelectionKind.one && chosen.index === index)) return state;
  if (chosen.kind === SelectionKind.closed) return { ...state, started: true, selection: { kind: SelectionKind.one, index } };
  const moves = Math.min(Rules.movesMaximum, state.moves + 1);
  return deck[chosen.index] === deck[index]
    ? { ...state, moves, matched: [...state.matched, chosen.index, index], selection: { kind: SelectionKind.closed } }
    : { ...state, moves, selection: { kind: SelectionKind.mismatch, first: chosen.index, second: index, remainingMs: Rules.mismatchMs } };
}

export function expectedReplay(input: unknown): Snapshot | null {
  const replay = parseReplay(input);
  if (!replay) return null;
  let state = freshSnapshot(replay.boardCode);
  let previous = 0;
  for (const action of replay.actions) {
    const advanced = advance(state, action.atMs - previous);
    if (!advanced) return null;
    state = activate(advanced, action.index);
    previous = action.atMs;
  }
  return advance(state, replay.endMs - previous);
}
