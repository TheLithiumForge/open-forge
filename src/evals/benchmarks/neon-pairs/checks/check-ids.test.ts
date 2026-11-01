import assert from "node:assert/strict";
import { test } from "node:test";
import { CheckStatus } from "../../../history/statuses.ts";
import { countChecks } from "../../../grading/counting.ts";
import { CheckArea, CheckId, GameFailure, guarded, result, withPageErrors } from "./check-ids.ts";
import { completeChecks, loadLayer, selection } from "./layer.ts";

test("game assertions, checker exceptions and app errors have distinct outcomes", async () => {
  assert.equal(result(CheckId.matching, CheckArea.matching, true, "Played").status, CheckStatus.pass);
  assert.equal((await guarded(CheckId.matching, CheckArea.matching, async () => { throw new GameFailure("Wrong game behavior"); }))[0]?.status, CheckStatus.fail);
  assert.equal((await guarded(CheckId.matching, CheckArea.matching, async () => { throw new Error("Checker timeout"); }))[0]?.status, CheckStatus.error);
  assert.equal(withPageErrors([result(CheckId.matching, CheckArea.matching, true, "Played")], ["Uncaught app error"])[0]?.status, CheckStatus.fail);
});

test("declared sets keep wishes apart and preserve the first outcome", async () => {
  const layer = await loadLayer();
  const passed = layer.checks.map((check) => ({ ...check, status: CheckStatus.pass, evidence: "Checked" }));
  const first = result(CheckId.matching, CheckArea.matching, false, "First attempt failed");
  const completed = completeChecks(layer, [first, ...passed]);
  assert.equal(completed.find((check) => check.id === first.id)?.status, CheckStatus.fail);
  const functional = countChecks(completeChecks(layer, passed.map((check) => layer.wishes.includes(check.id) ? { ...check, status: CheckStatus.fail } : check)), selection(layer));
  assert.equal(functional.workingGate, true);
  assert.equal(functional.statedSuccess, true);
  assert.equal(functional.coverage.passed, layer.stated.length);
});

test("a stopped group blocks dependent checks and missing independent results are errors", async () => {
  const layer = await loadLayer();
  const stopped = { id: CheckId.best, area: CheckArea.best, status: CheckStatus.error, evidence: "Browser crashed" };
  const completed = completeChecks(layer, [stopped]);
  assert.equal(completed.find((check) => check.id === CheckId.movesCompared)?.status, CheckStatus.blocked);
  assert.equal(completed.find((check) => check.id === CheckId.keyboard)?.status, CheckStatus.error);
});
