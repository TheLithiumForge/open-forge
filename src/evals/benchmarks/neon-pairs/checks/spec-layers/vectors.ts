import { expectedReplay, freshSnapshot } from "./replay.ts";
import { Rules, SelectionKind, type Snapshot } from "./rules.ts";
import { expectedDeck } from "./board-code.ts";
import { CapClause } from "./cap-policy.ts";

export const KnownCode = `${Rules.boardPrefix}-4-1`;
export const DistinctFacesClause = "D04.1";
export const DistinctFacesVector = "distinct faces";
export interface Vector { readonly name: string; readonly clauses: readonly string[]; readonly text: string }
const snapshot = freshSnapshot(KnownCode);
const replay = { version: Rules.formatVersion, boardCode: KnownCode, actions: [{ atMs: 10, index: 0 }, { atMs: 20, index: 1 }], endMs: 820 };
const vector = (name: string, clauses: string, value: unknown): Vector => ({ name, clauses: clauses.split(" "), text: JSON.stringify(value) });

export interface PairIndices { readonly first: number; readonly second: number }
export function cappedProgress(): { readonly state: Snapshot; readonly first: PairIndices; readonly remaining: readonly PairIndices[] } {
  const deck = expectedDeck(KnownCode), groups = new Map<number, number[]>();
  deck.forEach((face, index) => groups.set(face, [...(groups.get(face) ?? []), index]));
  const pairs = [...groups.values()].slice(0, 3).map((positions): PairIndices => {
    const [first, second] = positions;
    if (first === undefined || second === undefined) throw new Error("The cap vector needs whole pairs.");
    return { first, second };
  });
  const unmatched = pairs.flatMap((pair) => [pair.first, pair.second]), first = pairs.shift();
  if (!first || pairs.length !== 2) throw new Error("The cap vector needs three unmatched pairs.");
  return { first, remaining: pairs, state: { ...snapshot, started: true, moves: Rules.movesMaximum - 1, elapsedMs: Rules.elapsedMaximumMs - 1,
    matched: deck.map((_, index) => index).filter((index) => !unmatched.includes(index)), selection: { kind: SelectionKind.one, index: first.first } } };
}

export function snapshotVectors(): Vector[] {
  const one = { ...snapshot, started: true, selection: { kind: SelectionKind.one, index: 0 } };
  const mismatch = { ...one, moves: 1, selection: { kind: SelectionKind.mismatch, first: 0, second: 1, remainingMs: Rules.mismatchMs } };
  const matched = { ...snapshot, started: true, moves: 1, matched: [0, 11] };
  const all = { ...snapshot, started: true, moves: 8, matched: Array.from({ length: 16 }, (_, index) => index) };
  const vectors = [
    vector("closed", "D05.1 D05.2 D05.4 D05.5 D06.1 D07.1 D07.2 D07.4 D10.5", snapshot),
    vector("one", "D05.6 D06.4 D07.5", one), vector("mismatch", "D05.7 D06.5 D06.6 D07.3", mismatch),
    vector("paired", "D05.3 D05.4 D06.3", matched), vector("completed", "D06.7", all),
    vector("version", "D05.2 D10.4", { ...snapshot, version: 2 }), vector("unknown field", "D05.1 D06.2", { ...snapshot, extra: true }),
    vector("selection field", "D05.5 D06.2", { ...snapshot, selection: { kind: "closed", index: 0 } }),
    vector("one field", "D05.6 D06.2", { ...one, selection: { ...one.selection, extra: 0 } }),
    vector("mismatch field", "D05.7 D06.2", { ...mismatch, selection: { ...mismatch.selection, extra: 0 } }),
    vector("unpaired matched", "D06.3", { ...matched, matched: [0, 1] }), vector("duplicate matched", "D06.3", { ...matched, matched: [0, 0] }),
    vector("matched out of range", "D06.3", { ...matched, matched: [0, 16] }),
    vector("selected matched", "D06.4", { ...matched, selection: { kind: "one", index: 0 } }),
    vector("selected out of range", "D06.4", { ...one, selection: { kind: "one", index: 16 } }),
    vector("same selected", "D06.5", { ...mismatch, selection: { ...mismatch.selection, second: 0 } }),
    vector("mismatched same face", "D06.5", { ...mismatch, selection: { ...mismatch.selection, second: 11 } }),
    vector("completed selection", "D06.7", { ...all, selection: { kind: "one", index: 0 } }),
    vector("started type", "D07.1", { ...snapshot, started: 1 }), vector("not started progress", "D07.4", { ...one, started: false }),
    vector("moves too small", "D07.3", { ...matched, moves: 0 }), vector("miss too small", "D07.3", { ...mismatch, moves: 0 }),
    vector("started empty", "D07.5", { ...snapshot, started: true }),
    vector("history not reconstructed", "D10.8", { ...one, moves: 999, elapsedMs: 1 }),
    vector("elapsed cap accepted", CapClause.snapshotElapsed, { ...one, elapsedMs: Rules.elapsedMaximumMs }),
    vector("elapsed cap rejected", CapClause.snapshotElapsed, { ...one, elapsedMs: Rules.elapsedMaximumMs + 1 }),
    vector("move cap accepted", CapClause.snapshotMoves, { ...one, moves: Rules.movesMaximum }),
    vector("move cap rejected", CapClause.snapshotMoves, { ...one, moves: Rules.movesMaximum + 1 }),
    vector("both caps accepted", `${CapClause.snapshotElapsed} ${CapClause.snapshotMoves}`, { ...one, elapsedMs: Rules.elapsedMaximumMs, moves: Rules.movesMaximum }),
  ];
  for (const remainingMs of [0, 801, 1.5]) vectors.push(vector(`remaining ${remainingMs}`, "D06.6", { ...mismatch, selection: { ...mismatch.selection, remainingMs } }));
  for (const field of ["moves", "elapsedMs"]) for (const value of [-1, 0.5, Number.MAX_SAFE_INTEGER + 1, "0"]) vectors.push(vector(`${field} ${value}`, "D07.2", { ...one, [field]: value }));
  for (const code of ["np2-4-1", "np1-04-1", "np1-4-0", "np1-5-1"]) vectors.push(vector(code, "D10.7", { ...snapshot, boardCode: code }));
  vectors.push({ name: "malformed", clauses: ["D10.4"], text: "{" });
  const text = JSON.stringify(snapshot);
  vectors.push({ name: "snapshot byte limit accepted", clauses: ["D10.1"], text: text + " ".repeat(Rules.snapshotBytes - Buffer.byteLength(text)) });
  vectors.push({ name: "snapshot byte limit rejected", clauses: ["D10.1"], text: text + " ".repeat(Rules.snapshotBytes + 1 - Buffer.byteLength(text)) });
  vectors.push({ name: "snapshot multibyte limit", clauses: ["D10.1"], text: JSON.stringify({ ...snapshot, extra: "é".repeat(Rules.snapshotBytes / 2) }) });
  return vectors;
}

export function replayVectors(): Vector[] {
  const vectors = [vector("timeline", "C14.1 C14.3 T07.7 D08.1 D08.2 D08.3 D08.4 D08.5 D08.6 D09.1 D09.2 D09.6 D09.7 D10.6 D10.10", replay),
    vector("exact expiry", "D09.3", { ...replay, actions: [...replay.actions, { atMs: 820, index: 2 }], endMs: 1000 }),
    vector("locked", "D09.4", { ...replay, actions: [...replay.actions, { atMs: 819, index: 2 }], endMs: 819 }),
    vector("equal time", "D08.7", { ...replay, actions: [{ atMs: 10, index: 0 }, { atMs: 10, index: 11 }], endMs: 10 }),
    vector("pair activation", "D09.2", { ...replay, actions: [{ atMs: 10, index: 0 }, { atMs: 10, index: 11 }], endMs: 10 }),
    vector("version", "D08.1 D10.4 D10.10", { ...replay, version: 2 }), vector("unknown field", "D08.1", { ...replay, extra: 0 }),
    vector("action field", "D08.2", { ...replay, actions: [{ atMs: 0, index: 0, extra: 0 }] }),
    vector("negative index", "D08.3", { ...replay, actions: [{ atMs: 0, index: -1 }] }),
    vector("outside index", "D08.3", { ...replay, actions: [{ atMs: 0, index: 16 }] }),
    vector("descending time", "D08.5", { ...replay, actions: [{ atMs: 20, index: 0 }, { atMs: 10, index: 1 }] }),
    vector("early end", "D08.6", { ...replay, endMs: 19 }),
    vector("safe elapsed boundary", "D09.5", { ...replay, actions: [{ atMs: 0, index: 0 }], endMs: Number.MAX_SAFE_INTEGER }),
    vector("elapsed reaches cap", "D09.5", { ...replay, actions: [{ atMs: 0, index: 0 }], endMs: Rules.elapsedMaximumMs }),
    vector("elapsed passes cap", "D09.5", { ...replay, actions: [{ atMs: 0, index: 0 }], endMs: Rules.elapsedMaximumMs + 1 }),
    vector("play after elapsed cap", "D09.3 D09.5", { ...replay, actions: [{ atMs: 0, index: 0 }, { atMs: Rules.elapsedMaximumMs, index: 1 }, { atMs: Rules.elapsedMaximumMs + Rules.mismatchMs, index: 2 }], endMs: Rules.elapsedMaximumMs + Rules.mismatchMs }),
  ];
  for (const value of [-1, 0.1, Number.MAX_SAFE_INTEGER + 1]) {
    vectors.push(vector(`action time ${value}`, "D08.4", { ...replay, actions: [{ atMs: value, index: 0 }] }));
    vectors.push(vector(`end time ${value}`, "D08.4 D09.5", { ...replay, endMs: value }));
  }
  for (const code of ["np1-4-0", "np1-4-01", "np1-04-1", "np1-5-1", `np1-4-${Rules.seedMaximum + 1}`, "np2-4-1", "np1-4-1\n"]) vectors.push(vector(code, "D01.1 D01.2 D01.3 D02.3 D10.7 D10.10", { ...replay, boardCode: code }));
  for (const code of [KnownCode, `np1-4-${Rules.seedMaximum}`, "np1-16-1"]) vectors.push(vector(`valid ${code}`, "D01.1 D01.2 D01.3 D09.1", { ...replay, boardCode: code, actions: [], endMs: 1000 }));
  for (const boardCode of [` ${KnownCode}`, `${KnownCode} `, `${KnownCode}\n`]) vectors.push(vector("exact parser keeps whitespace invalid", "D01.4", { ...replay, boardCode }));
  const accepted = Array.from({ length: Rules.actionsMaximum }, () => ({ atMs: 0, index: 0 }));
  vectors.push(vector("action limit accepted", "D10.3", { ...replay, actions: accepted }));
  vectors.push(vector("action limit rejected", "D10.3", { ...replay, actions: [...accepted, { atMs: 0, index: 0 }] }));
  const text = JSON.stringify(replay);
  for (const extra of [0, 1]) vectors.push({ name: `replay byte limit ${extra}`, clauses: ["D10.2"], text: text + " ".repeat(Rules.replayBytes + extra - Buffer.byteLength(text)) });
  vectors.push({ name: "malformed", clauses: ["D10.4", "D10.10"], text: "{" });
  for (const size of Rules.sizes) for (const seed of [1, 2, 739806647, Rules.seedMaximum]) {
    const code = `np1-${size}-${seed}`, deck = expectedDeck(code), pairs = new Map<number, number[]>();
    deck.forEach((face, index) => pairs.set(face, [...(pairs.get(face) ?? []), index]));
    const actions = [...pairs.values()].flatMap((positions) => positions.map((index) => ({ atMs: 0, index })));
    vectors.push(vector(`known deal ${code}`, `D02.1 D02.2 D02.4 D03.1 D03.2 D04.1 D04.2 D04.3 D04.4 D04.5${code === KnownCode ? " D03.3 D04.6" : ""}`, { ...replay, boardCode: code, actions, endMs: 1000 }));
  }
  for (const size of Rules.sizes) {
    const boardCode = `np1-${size}-1`, deck = expectedDeck(boardCode), anchor = deck.indexOf(0);
    // Solving expected pairs can't detect additional copies of the same identity.
    const actions = Array.from({ length: size * size / 2 - 1 }, (_, index) => [
      { atMs: index * Rules.mismatchMs, index: anchor },
      { atMs: index * Rules.mismatchMs, index: deck.indexOf(index + 1) },
    ]).flat();
    vectors.push(vector(`${DistinctFacesVector} ${boardCode}`, DistinctFacesClause, { ...replay, boardCode, actions, endMs: (size * size / 2 - 1) * Rules.mismatchMs }));
  }
  return vectors.map((entry) => ({ ...entry, clauses: [...new Set([...entry.clauses, expectedReplayStatus(entry.text) ? "C14.4" : "C14.5"])] }));
}

function expectedReplayStatus(text: string): boolean {
  // CLI output rules apply to every vector, including size and byte limits.
  return expectedReplay(text) !== null;
}
