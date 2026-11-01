import assert from "node:assert/strict";
import { test } from "node:test";
import { CapClause, NewCapClauses } from "./cap-policy.ts";
import { activate, advance, expectedReplay, freshSnapshot } from "./replay.ts";
import { decodeSnapshot, snapshotsEqual } from "./snapshot.ts";
import { Rules, SelectionKind } from "./rules.ts";
import { replaceSavedScore } from "./saved-score.ts";
import { cappedProgress, KnownCode, replayVectors, snapshotVectors } from "./vectors.ts";
import { fillTrace } from "./trace.ts";

test("snapshot caps accept both exact limits and reject either next integer", () => {
  const state = { ...freshSnapshot(KnownCode), started: true, moves: Rules.movesMaximum, elapsedMs: Rules.elapsedMaximumMs, selection: { kind: SelectionKind.one, index: 0 } };
  assert.deepEqual(decodeSnapshot(JSON.stringify(state)), state);
  assert.equal(decodeSnapshot(JSON.stringify({ ...state, moves: Rules.movesMaximum + 1 })), null);
  assert.equal(decodeSnapshot(JSON.stringify({ ...state, elapsedMs: Rules.elapsedMaximumMs + 1 })), null);
  for (const clause of [CapClause.snapshotElapsed, CapClause.snapshotMoves]) {
    const cases = snapshotVectors().filter((vector) => vector.clauses.includes(clause));
    assert.ok(cases.some((vector) => decodeSnapshot(vector.text) !== null));
    assert.ok(cases.some((vector) => decodeSnapshot(vector.text) === null));
  }
});

test("capped counters still allow mismatches, matching, victory and valid version 1 export", () => {
  const vector = cappedProgress();
  assert.deepEqual(decodeSnapshot(JSON.stringify(vector.state)), vector.state);
  const reached = activate(vector.state, vector.first.second);
  assert.equal(reached.moves, Rules.movesMaximum);
  const [a, b] = vector.remaining; assert.ok(a && b);
  const miss = activate(activate(reached, a.first), b.first);
  assert.equal(miss.moves, Rules.movesMaximum);
  assert.equal(miss.selection.kind, SelectionKind.mismatch);
  const pending = advance(miss, Rules.mismatchMs - 1); assert.ok(pending);
  assert.deepEqual(pending.selection, { kind: SelectionKind.mismatch, first: a.first, second: b.first, remainingMs: 1 });
  let state = advance(pending, 1); assert.ok(state);
  assert.deepEqual(state.selection, { kind: SelectionKind.closed });
  assert.equal(state.elapsedMs, Rules.elapsedMaximumMs);
  for (const pair of vector.remaining) state = activate(activate(state, pair.first), pair.second);
  assert.equal(state.moves, Rules.movesMaximum);
  assert.equal(state.matched.length, vector.state.matched.length + 2 * (vector.remaining.length + 1));
  const exported = decodeSnapshot(JSON.stringify(state)); assert.ok(exported);
  assert.equal(snapshotsEqual(state, exported), true);
  assert.deepEqual(advance(state, Number.MAX_SAFE_INTEGER), state);
});

test("logical time saturates safely while replay timestamps keep their safe-integer range", () => {
  const one = { ...activate(freshSnapshot(KnownCode), 0), elapsedMs: Rules.elapsedMaximumMs - 1 };
  assert.equal(advance(one, 1)?.elapsedMs, Rules.elapsedMaximumMs);
  assert.equal(advance(one, Number.MAX_SAFE_INTEGER)?.elapsedMs, Rules.elapsedMaximumMs);
  assert.equal(advance(one, -1), null);
  assert.equal(advance(one, Number.MAX_SAFE_INTEGER + 1), null);
  const replay = { version: Rules.formatVersion, boardCode: KnownCode, actions: [{ atMs: 0, index: 0 }], endMs: Number.MAX_SAFE_INTEGER };
  assert.equal(expectedReplay(JSON.stringify(replay))?.elapsedMs, Rules.elapsedMaximumMs);
  assert.equal(expectedReplay(JSON.stringify({ ...replay, endMs: Number.MAX_SAFE_INTEGER + 1 })), null);
  const afterCap = replayVectors().find((vector) => vector.name === "play after elapsed cap"); assert.ok(afterCap);
  const actual = expectedReplay(afterCap.text); assert.ok(actual);
  assert.equal(actual.elapsedMs, Rules.elapsedMaximumMs);
  assert.equal(actual.moves, 1);
  assert.deepEqual(actual.selection, { kind: SelectionKind.one, index: 2 });
});

test("saved-score mutation changes only discovered capped values and keeps unrelated fields", () => {
  const saved = { version: Rules.formatVersion, size: Rules.sizes[0], best: { smallest: Rules.movesMaximum }, other: [null, true, "unrelated", 8] };
  const replacement = replaceSavedScore(saved, Rules.movesMaximum, Rules.movesMaximum + 1);
  assert.equal(replacement.replacements, 1);
  assert.deepEqual(replacement.value, { ...saved, best: { smallest: Rules.movesMaximum + 1 } });
  assert.equal(saved.best.smallest, Rules.movesMaximum);
  assert.deepEqual(replaceSavedScore(null, Rules.movesMaximum, 0), { value: null, replacements: 0 });
  assert.equal(replaceSavedScore([Rules.movesMaximum, { best: Rules.movesMaximum }], Rules.movesMaximum, 0).replacements, 2);
  assert.deepEqual(replaceSavedScore(String(Rules.movesMaximum), Rules.movesMaximum, 0), { value: "0", replacements: 1 });
});

test("cap-only trace updates keep every existing entry and reference slot byte identical", () => {
  const entries = [{ clauseId: "R05.1", checks: ["existing check"], reference: ["keep existing source"] },
    ...NewCapClauses.map((clauseId) => ({ clauseId, checks: [], reference: ["keep rebuilt source"] }))];
  const text = JSON.stringify({ entries }, null, 2);
  const slots = new Map(NewCapClauses.map((clause) => [clause, [`cap-check#${clause}`]]));
  const result = fillTrace(text, slots);
  const updated: unknown = JSON.parse(result);
  assert.deepEqual(updated, { entries: entries.map((entry) => ({ ...entry, checks: slots.get(entry.clauseId) ?? entry.checks })) });
  assert.equal(fillTrace(result, new Map(NewCapClauses.map((clause) => [clause, []]))), text);
});
