import assert from "node:assert/strict";
import { readFile, writeFile } from "node:fs/promises";
import { join } from "node:path";
import { test } from "node:test";
import { brownfieldFixture, BrownfieldFixture } from "../../materialize/brownfield-test-fixture.ts";
import { brownfieldInputs } from "../../materialize/brownfield-inputs.ts";
import { writeNew, hashes } from "../../materialize/files.ts";
import { loadDefinition } from "../../benchmarks/neon-pairs/checks/spec-layers/load.ts";
import { LayerId } from "../../benchmarks/neon-pairs/checks/spec-layers/requirements.ts";
import { CheckStatus } from "../../history/statuses.ts";
import type { LayerOutput } from "../../benchmarks/neon-pairs/checks/spec-layers/container.ts";
import { runBrownfieldChecks } from "./gates.ts";
import { regressionDefinition, regressionOutcomes } from "./regression.ts";
import { BrownfieldGuard, changedFiles } from "./scope.ts";
import { ChangeKind, readChangeChecks } from "./change-checks.ts";

const Build = { passed: true, typecheck: true, seconds: 0, packages: null, html: "dist/index.html", bytes: null, log: "" };

async function passingRegression(): Promise<LayerOutput> {
  const definition = await loadDefinition(LayerId.three);
  return { checks: definition.checks.map((check) => ({ id: check.id, area: check.area, status: CheckStatus.pass, evidence: "Fixture regression evidence." })),
    assertions: definition.checks.flatMap((check) => check.assertions.map((assertion) => ({ id: assertion.id, clause: assertion.clause, area: "regression", status: CheckStatus.pass, evidence: "Fixture clause evidence." }))),
    shots: [], review: definition.review, performance: null };
}

test("regressions remove only superseded clauses inside a grouped check", async () => {
  const frozen = await loadDefinition(LayerId.three), removed = "R05.1";
  const selected = regressionDefinition(frozen, [removed]);
  assert.equal(selected.checks.flatMap((check) => check.assertions).some((assertion) => assertion.clause === removed), false);
  assert.equal(selected.checks.flatMap((check) => check.assertions).length, frozen.checks.flatMap((check) => check.assertions).length - 1);
  const evidence = await passingRegression();
  const outcomes = regressionOutcomes(selected, evidence.assertions.map((assertion) => assertion.clause === removed ? { ...assertion, status: CheckStatus.fail } : assertion), evidence.checks);
  assert.ok(outcomes.every((check) => check.status === CheckStatus.pass));
  assert.throws(() => regressionDefinition(frozen, ["R99.1"]));
});

test("combined cases report feature failures apart from repairs and frozen regressions", async (t) => {
  const fixture = await brownfieldFixture(t), inputs = await brownfieldInputs(fixture.caseRoot, fixture.reference, fixture.destination);
  const project = join(fixture.root, "candidate");
  for (const file of inputs.seed) await writeNew(project, file.path, file.bytes);
  await writeFile(join(project, BrownfieldFixture.source), BrownfieldFixture.repaired);
  const declared = await readChangeChecks(inputs.case);
  const output = await runBrownfieldChecks(inputs, hashes(inputs.seed), { project, html: null, build: Build, shotsFolder: join(fixture.root, "shots") }, {
    protectedRoots: [fixture.caseRoot], regression: passingRegression,
    changes: async () => declared.map((check) => ({ id: `${check.kind}-${check.id}`, area: check.kind, status: check.kind === ChangeKind.feature ? CheckStatus.fail : CheckStatus.pass, evidence: "Independent fixture result." })),
  });
  assert.ok(output.checks.filter((check) => check.area === ChangeKind.feature).every((check) => check.status === CheckStatus.fail));
  assert.ok(output.checks.filter((check) => check.area === ChangeKind.repair || check.area === "regression").every((check) => check.status === CheckStatus.pass));
  assert.equal(output.checks.find((check) => check.id === BrownfieldGuard.scope)?.status, CheckStatus.pass);
  assert.equal(output.checks.find((check) => check.id === BrownfieldGuard.protected)?.status, CheckStatus.pass);
  assert.equal(output.groups.feature.passed, 0);
  assert.equal(output.groups.repair.passed, output.groups.repair.applicable);
  assert.equal(output.groups.regression.passed, output.groups.regression.applicable);
  const changes = changedFiles(hashes(inputs.seed), hashes(inputs.seed).filter((file) => file.path !== BrownfieldFixture.source), ["src/**"]);
  assert.equal(changes[0]?.change, "deleted");
  assert.equal(changes[0]?.allowed, true);
});

test("scope and protected inputs fail independently and a regression error doesn't block repair checks", async (t) => {
  const fixture = await brownfieldFixture(t), inputs = await brownfieldInputs(fixture.caseRoot, fixture.reference, fixture.destination);
  const project = join(fixture.root, "candidate");
  for (const file of inputs.seed) await writeNew(project, file.path, file.bytes);
  await writeNew(project, "unrelated.txt", "An unrelated change.\n");
  await writeNew(project, "seed.patch", await readFile(join(fixture.caseRoot, "seed.patch")));
  const declared = await readChangeChecks(inputs.case);
  let changesRan = false;
  const output = await runBrownfieldChecks(inputs, hashes(inputs.seed), { project, html: null, build: Build, shotsFolder: join(fixture.root, "shots") }, {
    protectedRoots: [fixture.caseRoot], regression: async () => { throw new Error("Fixture regression fault."); }, changes: async () => {
      changesRan = true;
      await writeNew(fixture.caseRoot, "changed-grader.txt", "Changed while checking.\n");
      return declared.map((check) => ({ id: `${check.kind}-${check.id}`, area: check.kind, status: CheckStatus.pass, evidence: "Independent fixture result." }));
    },
  });
  assert.equal(changesRan, true);
  assert.equal(output.checks.find((check) => check.id === BrownfieldGuard.scope)?.status, CheckStatus.fail);
  assert.equal(output.checks.find((check) => check.id === BrownfieldGuard.protected)?.status, CheckStatus.fail);
  assert.ok(output.checks.filter((check) => check.area === ChangeKind.repair).every((check) => check.status === CheckStatus.pass));
  assert.ok(output.checks.filter((check) => check.area === "regression").every((check) => check.status === CheckStatus.error));
});
