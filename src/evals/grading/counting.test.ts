import assert from "node:assert/strict";
import { test } from "node:test";
import { CheckStatus } from "../history/statuses.ts";
import type { CheckOutcome } from "./grading.types.ts";
import { adaptCheck, countChecks, coverage, passesGate, summarizeMeasurements, tokenTotal } from "./counting.ts";

const ids = { opens: "opens", plays: "plays", wins: "wins", missing: "missing" } as const;
const check = (id: string, status: CheckStatus): CheckOutcome => ({ id, area: "play", status, evidence: id });

test("legacy booleans become pass, fail and blocked with their evidence", () => {
  for (const [passed, status] of [[true, CheckStatus.pass], [false, CheckStatus.fail], [null, CheckStatus.blocked]] as const) {
    assert.deepEqual(adaptCheck({ id: ids.opens, area: "play", passed, detail: ids.opens }), check(ids.opens, status));
  }
});

test("coverage keeps blocked and unresolved errors and counts each ID once", () => {
  const outcomes = [check(ids.opens, CheckStatus.fail), check(ids.opens, CheckStatus.pass), check(ids.plays, CheckStatus.blocked), check(ids.wins, CheckStatus.error)];
  assert.deepEqual(coverage(outcomes, [ids.opens, ids.opens, ids.plays, ids.wins, ids.missing]), {
    passed: 0, applicable: 4, blocked: 2, errors: 1, share: 0,
  });
});

test("working and stated gates use their declared IDs independently", () => {
  const outcomes = [check(ids.opens, CheckStatus.pass), check(ids.plays, CheckStatus.pass), check(ids.wins, CheckStatus.error)];
  const result = countChecks(outcomes, { applicable: Object.values(ids), working: [ids.opens, ids.plays], stated: [ids.opens, ids.plays, ids.wins] });
  assert.equal(result.coverage.share, 0.5);
  assert.equal(result.workingGate, true);
  assert.equal(result.statedSuccess, false);
  assert.equal(passesGate(outcomes, [ids.missing]), false);
  assert.equal(passesGate(outcomes, []), null);
  assert.equal(coverage([]).share, null);
});

test("token results keep every missing value and count zero as reported", () => {
  assert.equal(tokenTotal(null), null);
  assert.equal(tokenTotal({ input: 1, cachedInput: 2, cacheWrite: 3, output: 4, reasoning: 4 }), 10);
  const values = [null, 0, 12, null];
  assert.deepEqual(summarizeMeasurements(values), { values, samples: 2, missing: 2, mean: 6 });
  assert.deepEqual(summarizeMeasurements([null]), { values: [null], samples: 0, missing: 1, mean: null });
});
