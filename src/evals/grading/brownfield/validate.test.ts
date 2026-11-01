import assert from "node:assert/strict";
import { readFile, writeFile } from "node:fs/promises";
import { join } from "node:path";
import { test } from "node:test";
import { loadBrownfieldSuite } from "../../materialize/brownfield.ts";
import { BrownfieldFiles } from "../../materialize/brownfield-case.ts";
import { BrownfieldFixture } from "../../materialize/brownfield-test-fixture.ts";
import { CheckStatus } from "../../history/statuses.ts";
import { hashes, inventory, writeNew } from "../../materialize/files.ts";
import { ChangeCheckFiles, ChangeCheckVersion } from "./change-checks.ts";
import { validateCase, CaseValidationVersion } from "./validate.ts";
import { readValidationExpectations, ValidationCheck, ValidationPhase } from "./validation-expectations.ts";
import { validationFixture } from "./validation-test-fixture.ts";
import { BrownfieldGuard } from "./scope.ts";

test("case validation builds starting and solution snapshots, saves bound inputs and can run again", async (t) => {
  const fixture = await validationFixture(t), original = hashes(await inventory(fixture.reference));
  const result = await validateCase(fixture.suite, fixture.tools);
  assert.equal(result.schemaVersion, CaseValidationVersion);
  assert.equal(result.passed, true);
  assert.deepEqual(result.stages.map((stage) => stage.phase), Object.values(ValidationPhase));
  assert.deepEqual(fixture.builds, [BrownfieldFixture.broken, BrownfieldFixture.repaired]);
  assert.deepEqual(fixture.phases, fixture.builds);
  assert.ok(result.inputs.case.length && result.inputs.tooling.length && result.inputs.reference.length);
  const path = join(fixture.caseRoot, BrownfieldFiles.checks, BrownfieldFiles.validation);
  assert.deepEqual(JSON.parse(await readFile(path, "utf8")), result);
  const reloaded = await loadBrownfieldSuite(fixture.caseRoot, BrownfieldFixture.id, fixture.reference, fixture.destination);
  assert.equal(reloaded.fingerprint, fixture.suite.fingerprint);
  assert.equal((await validateCase(reloaded, fixture.tools)).passed, true);
  assert.deepEqual(hashes(await inventory(fixture.reference)), original);
  for (const phase of Object.values(ValidationPhase)) for (const check of fixture.checks) {
    const entry = result.stages.find((entry) => entry.phase === phase)?.outcomes.find((entry) => entry.id === `${check.kind}-${check.id}`);
    assert.equal(entry?.expected, phase === ValidationPhase.starting ? CheckStatus.fail : CheckStatus.pass);
    assert.equal(entry?.matches, true);
  }
});

test("validation rejects unexpected starting passes and solution scope or regression failures", async (t) => {
  const fixture = await validationFixture(t), gates = fixture.tools.gates; assert.ok(gates?.regression);
  const regression = gates.regression;
  const result = await validateCase(fixture.suite, { ...fixture.tools, gates: { ...gates,
    changes: async (_item, checks) => checks.map((check) => ({ id: `${check.kind}-${check.id}`, area: check.kind, status: CheckStatus.pass, evidence: "Fixture unexpected pass." })),
    regression: async (input) => {
      const result = await regression(input);
      if ((await readFile(join(input.project, BrownfieldFixture.source), "utf8")) === BrownfieldFixture.repaired) {
        await writeNew(input.project, "outside-scope.txt", "Unrelated change.");
        return { ...result, checks: result.checks.map((check) => ({ ...check, status: CheckStatus.fail })), assertions: result.assertions.map((entry) => ({ ...entry, status: CheckStatus.fail })) };
      }
      return result;
    },
  } });
  assert.equal(result.passed, false);
  assert.ok(result.stages[0]?.outcomes.some((entry) => !entry.matches && entry.expected === CheckStatus.fail && entry.actual === CheckStatus.pass));
  assert.equal(result.stages[1]?.outcomes.find((entry) => entry.id === BrownfieldGuard.scope)?.matches, false);
  assert.ok(result.stages[1]?.outcomes.some((entry) => entry.id.startsWith("regression-") && !entry.matches));
});

test("build and protected-input failures leave a failed report and still check the solution", async (t) => {
  const fixture = await validationFixture(t), build = fixture.tools.build; assert.ok(build);
  let calls = 0;
  const result = await validateCase(fixture.suite, { ...fixture.tools, build: async (space) => {
    const result = await build(space); calls += 1;
    if (calls === 1) {
      await writeNew(fixture.caseRoot, "changed-input.txt", "Changed outside the project.");
      return { ...result, passed: false };
    }
    return result;
  } });
  assert.equal(result.passed, false);
  assert.equal(calls, 2);
  assert.equal(result.stages[0]?.outcomes.find((entry) => entry.id === ValidationCheck.build)?.matches, false);
  assert.ok(result.stages.every((stage) => stage.outcomes.find((entry) => entry.id === ValidationCheck.inputs)?.matches === false));
  assert.equal(JSON.parse(await readFile(join(fixture.caseRoot, BrownfieldFiles.checks, BrownfieldFiles.validation), "utf8")).passed, false);
});

test("validation requires declared starting results and rejects unknown regression expectations", async (t) => {
  const fixture = await validationFixture(t), item = fixture.suite.brownfield?.case; assert.ok(item);
  const path = join(item.root, item.checks, ChangeCheckFiles.manifest);
  const manifest = { schemaVersion: ChangeCheckVersion, checks: fixture.checks };
  await writeFile(path, JSON.stringify({ ...manifest, checks: fixture.checks.map(({ expected: _expected, ...check }) => check) }));
  await assert.rejects(readValidationExpectations(item));
  await writeFile(path, JSON.stringify({ ...manifest, regressionExpectations: { "unknown-regression": CheckStatus.fail } }));
  await assert.rejects(readValidationExpectations(item));
  await writeFile(path, JSON.stringify({ ...manifest, checks: fixture.checks.map((check) => ({ ...check, expected: { starting: CheckStatus.error, solution: CheckStatus.pass } })) }));
  await assert.rejects(readValidationExpectations(item));
});

test("stale solution patches and verifier faults produce failed reports with paths removed", async (t) => {
  const fixture = await validationFixture(t), build = fixture.tools.build; assert.ok(build);
  let calls = 0;
  const failed = await validateCase(fixture.suite, { ...fixture.tools, build: async (space) => {
    calls += 1;
    if (calls === 1) throw new Error(`Fixture verifier fault at ${fixture.reference}`);
    return build(space);
  } });
  assert.equal(failed.passed, false);
  assert.equal(calls, 2);
  const saved = await readFile(join(fixture.caseRoot, BrownfieldFiles.checks, BrownfieldFiles.validation), "utf8");
  assert.equal(saved.includes(fixture.reference), false);
  const patchPath = join(fixture.caseRoot, BrownfieldFiles.solution);
  await writeFile(patchPath, (await readFile(patchPath, "utf8")).replace(BrownfieldFixture.broken.trimEnd(), "stale context"));
  const reloaded = await loadBrownfieldSuite(fixture.caseRoot, BrownfieldFixture.id, fixture.reference, fixture.destination);
  const stale = await validateCase(reloaded, fixture.tools);
  assert.equal(stale.passed, false);
  assert.equal(stale.stages[0]?.passed, true);
  assert.equal(stale.stages[1]?.outcomes.find((entry) => entry.id === ValidationCheck.stage)?.matches, false);
});

test("a reference changed during the build fails the input guard", async (t) => {
  const fixture = await validationFixture(t), build = fixture.tools.build; assert.ok(build);
  const result = await validateCase(fixture.suite, { ...fixture.tools, build: async (space) => {
    await writeFile(join(fixture.reference, BrownfieldFixture.source), BrownfieldFixture.repaired);
    return build(space);
  } });
  assert.equal(result.passed, false);
  assert.ok(result.stages.every((stage) => stage.outcomes.find((entry) => entry.id === ValidationCheck.inputs)?.matches === false));
});
