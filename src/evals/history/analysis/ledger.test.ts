import assert from "node:assert/strict";
import { test } from "node:test";
import { ContextClass } from "./classes.ts";
import { partKey, runLedger, type LedgerStep } from "./ledger.ts";
import { apportion, contextParts } from "./parts.ts";

const spec = (tokens: number) => ({ class: ContextClass.spec, fileType: "md", tokens });
const code = (tokens: number) => ({ class: ContextClass.code, fileType: "ts", tokens });
const step = (input: number, fresh: number, added: LedgerStep["added"] = [], compacted = false): LedgerStep => ({ input, fresh, added, compacted });
const sum = (tokens: ReadonlyMap<string, number>) => [...tokens.values()].reduce((all, value) => all + value, 0);

test("whole numbers keep their proportions and always add up to the total", () => {
  assert.deepEqual(apportion([1, 1, 1], 10), [4, 3, 3]);
  assert.deepEqual(apportion([0, 0], 5), [3, 2]);
  assert.equal(apportion([0.2, 7.9, 3.3], 1001).reduce((all, value) => all + value, 0), 1001);
});

test("the first request is up front and each later growth splits across what joined by estimated size", () => {
  const ledger = runLedger([step(1000, 1000), step(1300, 300, [spec(100), code(100)]), step(1400, 100, [code(50)])]);
  assert.equal(sum(ledger.input), 1000 + 1300 + 1400);
  assert.equal(ledger.input.get(partKey(ContextClass.upfront, null)), 3000);
  assert.equal(ledger.input.get(partKey(ContextClass.spec, "md")), 150 * 2);
  assert.equal(ledger.input.get(partKey(ContextClass.code, "ts")), 150 * 2 + 100);
  assert.equal(sum(ledger.fresh), 1400);
  assert.equal(ledger.fresh.get(partKey(ContextClass.code, "ts")), 150 + 100);
  assert.equal(sum(ledger.peak), 1400);
});

test("a shrinking context scales every part alike, and a compaction keeps the up-front text and counts the rest as other", () => {
  const shrunk = runLedger([step(1000, 1000), step(900, 0, [spec(100)])]);
  assert.equal(sum(shrunk.input), 1900);
  const compacted = runLedger([step(1000, 1000), step(3000, 2000, [code(2000)]), step(1200, 200, [], true)]);
  assert.equal(sum(compacted.input), 5200);
  assert.equal(compacted.peak.get(partKey(ContextClass.code, "ts")), 2000);
  assert.equal(compacted.input.get(partKey(ContextClass.other, null)), 200);
});

test("context parts sum exactly to the run's totals, so the split always checks against them", () => {
  const ledger = runLedger([step(1001, 1001), step(1337, 340, [spec(33), code(77)])]);
  const parts = contextParts(ledger, { input: 2338, fresh: 1341, peak: 1337 });
  assert.equal(parts.reduce((all, part) => all + part.input, 0), 2338);
  assert.equal(parts.reduce((all, part) => all + part.fresh, 0), 1341);
  assert.equal(parts.reduce((all, part) => all + part.peak, 0), 1337);
  assert.deepEqual(parts.map((part) => part.class), [ContextClass.upfront, ContextClass.code, ContextClass.spec]);
});
