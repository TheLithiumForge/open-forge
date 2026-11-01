import assert from "node:assert/strict";
import { randomUUID } from "node:crypto";
import { mkdir, mkdtemp, writeFile } from "node:fs/promises";
import { join } from "node:path";
import { test } from "node:test";
import { brownfieldFixture, BrownfieldFixture } from "../../materialize/brownfield-test-fixture.ts";
import { loadBrownfieldSuite } from "../../materialize/brownfield.ts";
import { hashes, writeNew } from "../../materialize/files.ts";
import { CodeOnlySetup } from "../../materialize/brownfield-case.ts";
import { HistoryStore } from "../../history/store.ts";
import { v2Fixture } from "../../history/test-fixtures.ts";
import { plannedRun } from "../../history/run-records.ts";
import { AssessmentKind, CheckStatus, RunStatus } from "../../history/statuses.ts";
import { executeRun } from "../../execution/run.ts";
import { suiteForSetup } from "../../execution/workspace.ts";
import { TurnOutcome } from "../../execution/providers/agent-session.ts";
import { readChangeChecks } from "./change-checks.ts";
import { runBrownfieldChecks } from "./gates.ts";

const BuiltPage = "dist/index.html";
const Telemetry = { tokens: null, requests: null, peakContext: null, contextWindow: null, compactions: null, toolCalls: 0, failedToolCalls: 0,
  openForgeReads: 0, openForgeBytes: null, reportedCostUsd: null, model: null, sessionId: null } as const;

test("the product run saves brownfield gates as a checks assessment without Docker or models", async (t) => {
  const fixture = await brownfieldFixture(t);
  const suite = await loadBrownfieldSuite(fixture.caseRoot, BrownfieldFixture.id, fixture.reference, fixture.destination);
  const inputs = suite.brownfield; assert.ok(inputs);
  const original = v2Fixture(), store = new HistoryStore(join(fixture.root, "records"));
  const config = { ...original.batch.config, suite: suite.id, setups: [CodeOnlySetup] };
  const agent = config.agents[0]; assert.ok(agent);
  const run = plannedRun(randomUUID(), original.batch.id, CodeOnlySetup, agent, 1);
  const batch = { ...original.batch, suite: suite.id, config, runs: [run.id] };
  await store.saveBatch(batch); await store.saveRun(run);
  const declared = await readChangeChecks(inputs.case);
  const output = await executeRun({ run, suite: suiteForSetup(suite, CodeOnlySetup), config, snapshot: "", position: "fixture", services: {
    store, stop: async () => {}, scratch: () => mkdtemp(join(fixture.root, "judge-")), judge: async () => [],
    createSpace: async (project) => {
      const root = join(fixture.root, "attempt"), work = join(root, project), home = join(root, "home");
      await mkdir(home, { recursive: true });
      for (const file of inputs.seed) await writeNew(work, file.path, file.bytes);
      return { root, work, home, project };
    },
    conversation: async (context) => {
      await writeFile(join(context.space.work, BrownfieldFixture.source), BrownfieldFixture.repaired);
      return { outcome: TurnOutcome.answered, finalMessage: "Fixture change complete.", questions: [], telemetry: Telemetry, error: null, seconds: 0 };
    },
    build: async (_isolation, space) => {
      await writeNew(space.work, BuiltPage, "<html>Fixture build</html>");
      return { passed: true, typecheck: true, seconds: 0, packages: 0, html: BuiltPage, bytes: 1, log: "Fixture verifier." };
    },
    checks: (input) => runBrownfieldChecks(inputs, hashes(inputs.seed), input, {
      protectedRoots: [fixture.caseRoot], regression: async () => { throw new Error("Fixture regression checker fault."); },
      changes: async () => declared.map((check) => ({ id: `${check.kind}-${check.id}`, area: check.kind, status: CheckStatus.pass, evidence: "Fixture change evidence." })),
    }),
  } });
  assert.equal(output.status, RunStatus.done);
  const saved = await store.readRun(batch.id, run.id);
  assert.ok(saved.functionalResult);
  assert.ok(saved.functionalResult.coverage.errors > 0);
  const assessment = (await store.readAssessments(batch.id)).find((entry) => entry.kind === AssessmentKind.checks);
  assert.ok(assessment?.kind === AssessmentKind.checks);
  for (const area of ["feature", "repair", "regression", "scope", "protected"]) assert.ok(assessment.outcomes.some((check) => check.area === area));
});
