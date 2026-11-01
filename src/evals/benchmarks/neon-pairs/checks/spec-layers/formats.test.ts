import assert from "node:assert/strict";
import { test } from "node:test";
import { boundedDraw, expectedDeck, generator, parseBoardCode } from "./board-code.ts";
import { activate, advance, expectedReplay, freshSnapshot, parseReplay } from "./replay.ts";
import { decodeSnapshot, exactKeys, nonnegativeInteger, snapshotsEqual } from "./snapshot.ts";
import { Rules, SelectionKind, faceName } from "./rules.ts";
import { cardName, elapsedIn, faceIn, fullyVisible, near, overlaps, pixels, rgb, transitionMs } from "./view-values.ts";

const Code = `${Rules.boardPrefix}-4-1`;
const LiteralDeck = [6, 7, 0, 5, 2, 2, 0, 5, 3, 7, 1, 6, 4, 4, 1, 3];
test("board codes use exact decimal syntax and fixed independent vectors", () => {
  assert.deepEqual(parseBoardCode(Code), { size: 4, seed: 1 });
  for (const code of [null, "np2-4-1", "np1-04-1", "np1-4-01", "np1-5-1", "np1-4-0", `np1-4-${Rules.seedMaximum + 1}`, `${Code}\n`]) assert.equal(parseBoardCode(code), null);
  assert.deepEqual(expectedDeck(Code), LiteralDeck);
  const next = generator(1);
  assert.deepEqual(Array.from({ length: 5 }, next), [16807, 282475249, 1622650073, 984943658, 1144108930]);
  for (const size of Rules.sizes) {
    const deck = expectedDeck(`np1-${size}-${Rules.seedMaximum}`);
    assert.equal(deck.length, size * size);
    assert.equal(new Set(deck).size, size * size / 2);
    for (const face of new Set(deck)) assert.equal(deck.filter((item) => item === face).length, 2);
  }
  const draws = [Rules.modulus - 1, 1];
  assert.equal(boundedDraw(() => draws.shift() ?? 1, 5), 0);
  assert.throws(() => generator(0));
  assert.throws(() => boundedDraw(() => 0, 2));
});

test("snapshot decoder validates full state without reconstructing history", () => {
  const fresh = freshSnapshot(Code);
  assert.deepEqual(decodeSnapshot(JSON.stringify(fresh)), fresh);
  const one = activate(fresh, 0);
  const miss = activate(one, 1);
  assert.deepEqual(decodeSnapshot(JSON.stringify(miss)), miss);
  const impossibleHistory = { ...one, elapsedMs: 1_000_000, moves: 100 };
  assert.deepEqual(decodeSnapshot(JSON.stringify(impossibleHistory)), impossibleHistory);
  for (const patch of [{ version: 2 }, { extra: 1 }, { matched: [0] }, { matched: [0, 0] }, { moves: -1 }, { elapsedMs: Number.MAX_SAFE_INTEGER + 1 }, { started: 1 }, { selection: { kind: "closed", index: 1 } }, { selection: { kind: "one", index: 16 } }]) assert.equal(decodeSnapshot(JSON.stringify({ ...fresh, ...patch })), null);
  for (const remainingMs of [0, 801, 1.5]) assert.equal(decodeSnapshot(JSON.stringify({ ...miss, selection: { ...miss.selection, remainingMs } })), null);
  assert.equal(decodeSnapshot(" ".repeat(Rules.snapshotBytes + 1)), null);
  assert.equal(decodeSnapshot("{"), null);
  assert.equal(snapshotsEqual({ ...fresh, matched: [0, 11] }, { ...fresh, matched: [11, 0] }), true);
  assert.equal(snapshotsEqual(one, fresh), false);
  assert.equal(exactKeys({ a: 1 }, ["a"]), true);
  assert.equal(nonnegativeInteger(NaN), false);
});

test("replay advances before activation, preserves equal-time order and ignores locked cards", () => {
  const replay = { version: Rules.formatVersion, boardCode: Code, actions: [{ atMs: 200, index: 0 }, { atMs: 200, index: 1 }, { atMs: 999, index: 2 }, { atMs: 1000, index: 2 }], endMs: 1100 };
  const actual = expectedReplay(JSON.stringify(replay));
  assert.equal(actual?.moves, 1);
  assert.equal(actual?.elapsedMs, 900);
  assert.deepEqual(actual?.selection, { kind: SelectionKind.one, index: 2 });
  for (const patch of [{ version: 2 }, { extra: 1 }, { endMs: 100 }, { actions: [{ atMs: 2, index: 0 }, { atMs: 1, index: 1 }] }, { actions: [{ atMs: 0, index: 16 }] }, { actions: [{ atMs: 0.5, index: 1 }] }]) assert.equal(parseReplay(JSON.stringify({ ...replay, ...patch })), null);
  assert.equal(parseReplay("x".repeat(Rules.replayBytes + 1)), null);
  assert.equal(parseReplay(JSON.stringify({ ...replay, actions: Array.from({ length: Rules.actionsMaximum + 1 }, () => ({ atMs: 0, index: 0 })) })), null);
  assert.equal(advance({ ...freshSnapshot(Code), started: true, elapsedMs: Number.MAX_SAFE_INTEGER }, 1), null);
});

test("view parsers compare units, names and geometry without hidden game data", () => {
  assert.equal(faceName(127), "ice outline chevron, marker top-left");
  assert.throws(() => faceName(128));
  assert.equal(cardName('- button "Row 1, column 1, closed"'), "Row 1, column 1, closed");
  assert.equal(faceIn(`Row 1, column 1, ${faceName(127)}, revealed`), faceName(127));
  assert.equal(faceIn("Row 1, column 1, closed"), null);
  assert.equal(pixels("44px"), Rules.cardMinimum);
  assert.equal(pixels("auto"), null);
  assert.equal(rgb("rgb(99, 231, 255)"), Rules.palette[0]);
  assert.equal(rgb("rgba(99, 231, 255, 0.5)"), null);
  assert.equal(rgb("#63e7ff"), Rules.palette[0]);
  assert.equal(near(44.1, Rules.cardMinimum), true);
  assert.equal(near(null, Rules.cardMinimum), false);
  assert.deepEqual(transitionMs("120ms, 0.12s"), [120, 120]);
  assert.equal(elapsedIn("Time: 01:23"), 83_000);
  assert.equal(elapsedIn("Elapsed: 1:02:03"), 3_723_000);
  const box = { x: 10, y: 10, width: 44, height: 44 };
  assert.equal(fullyVisible(box, { x: 0, y: 0, width: 100, height: 100 }), true);
  assert.equal(fullyVisible(box, { ...box }), false);
  assert.equal(overlaps(box, { ...box, x: 100 }), false);
});
