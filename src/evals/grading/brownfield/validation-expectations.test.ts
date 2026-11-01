import assert from "node:assert/strict";
import { readFile, writeFile } from "node:fs/promises";
import { join } from "node:path";
import { test } from "node:test";
import { CheckStatus } from "../../history/statuses.ts";
import { BrownfieldFiles, ChangeType } from "../../materialize/brownfield-case.ts";
import { ChangeCheckFiles, ChangeKind } from "./change-checks.ts";
import { compareValidation, readValidationExpectations, ValidationPhase, ValidationStatus } from "./validation-expectations.ts";
import { validationFixture } from "./validation-test-fixture.ts";

test("starting regression exceptions are registered by ID and feature cases cannot waive regressions", async (t) => {
  const fixture = await validationFixture(t), item = fixture.suite.brownfield?.case; assert.ok(item);
  const original = await readValidationExpectations(item);
  const regression = original.find((check) => check.id.startsWith("regression-")); assert.ok(regression);
  const path = join(item.root, item.checks, ChangeCheckFiles.manifest);
  const raw = JSON.parse(await readFile(path, "utf8"));
  const overrides = { [regression.id]: CheckStatus.fail };
  await writeFile(path, JSON.stringify({ ...raw, regressionExpectations: overrides }));
  const updated = await readValidationExpectations(item);
  assert.equal(updated.find((check) => check.id === regression.id)?.starting, CheckStatus.fail);
  assert.equal(updated.find((check) => check.id === regression.id)?.solution, CheckStatus.pass);
  await writeFile(path, JSON.stringify({ ...raw, checks: fixture.checks.map((check) => ({ ...check, kind: ChangeKind.feature })), regressionExpectations: overrides }));
  await assert.rejects(readValidationExpectations({ ...item, changeType: ChangeType.feature }));
  assert.ok(item.inputs.some((file) => file.path === BrownfieldFiles.solution));
});

test("missing, duplicate, invented and errored outcomes cannot validate a case", () => {
  const id = "fixture-check", expected = [{ id, starting: CheckStatus.fail, solution: CheckStatus.pass }];
  const check = { id, area: ChangeKind.repair, status: CheckStatus.fail, evidence: "Fixture evidence." };
  assert.equal(compareValidation(expected, [check], ValidationPhase.starting)[0]?.matches, true);
  assert.equal(compareValidation(expected, [check], ValidationPhase.solution)[0]?.matches, false);
  assert.equal(compareValidation(expected, [], ValidationPhase.starting)[0]?.actual, ValidationStatus.missing);
  for (const outcomes of [[check, check], [{ ...check, id: "invented-check" }], [{ ...check, status: CheckStatus.error }]]) {
    assert.ok(compareValidation(expected, outcomes, ValidationPhase.starting).some((outcome) => !outcome.matches));
  }
});
