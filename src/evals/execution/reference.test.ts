import assert from "node:assert/strict";
import { cp, mkdir, mkdtemp, readFile, writeFile } from "node:fs/promises";
import { join } from "node:path";
import { test } from "node:test";
import { referenceOptions } from "../config/reference.ts";
import { PresetName } from "../config/presets.ts";
import { modelKey } from "../config/judges.ts";
import { FixtureConfig, FixtureSuite } from "../config/test-fixtures.ts";
import { FixtureVersions, temporaryHistory, v2Fixture } from "../history/test-fixtures.ts";
import { AssessmentKind, BatchStatus, CheckStatus, RunStatus } from "../history/statuses.ts";
import { fingerprint } from "../history/fingerprints.ts";
import { ReferenceImplementer } from "../history/reference.ts";
import { exportBatch } from "../history/export.ts";
import { HistoryFiles } from "../history/artifacts.ts";
import { batchMatrixSummary } from "../config/summary.ts";
import { ResultFiles } from "../paths.ts";
import { runReference } from "./reference.ts";
import { copyReference, ReferenceSourceCommands } from "./reference-source.ts";

const SourceFile = "game.ts", SourceText = "export const game = 'reference';\n", Commit = "b".repeat(40);
const Documents = ["Fixture full specification."], BuiltPage = "dist/index.html";

test("a reference batch skips agents, records versions, grades both judges and exports as a reference", async (t) => {
  const history = await temporaryHistory(); t.after(history.cleanup);
  const source = join(history.root, "source"); await mkdir(source);
  await writeFile(join(source, SourceFile), SourceText);
  const choices = referenceOptions(undefined, PresetName.both), judges: string[] = [];
  let builds = 0, checks = 0;
  const status = await runReference(choices, () => {}, {
    config: async () => FixtureConfig,
    suite: async (id) => ({ ...FixtureSuite, id, documents: Documents }),
    capture: async (id, config, suite) => ({ ...v2Fixture().batch, id, config, suite: suite.id, runs: [],
      requestedModels: config.agents, versions: FixtureVersions,
      fingerprints: { ...v2Fixture().batch.fingerprints, checks: FixtureVersions.checker, rubric: FixtureVersions.rubric } }),
    copy: (_source, target, spec) => copyReference(source, target, spec, async (args) => args === ReferenceSourceCommands.commit ? Commit
      : args === ReferenceSourceCommands.tracked ? `${SourceFile}\0` : ""),
    store: history.store, prepare: async () => {},
    run: {
      stop: async () => {}, scratch: () => mkdtemp(join(history.root, "judge-")),
      createSpace: async (project, snapshot) => {
        const root = join(history.root, "attempt"), home = join(root, "home"), work = join(root, project);
        await mkdir(home, { recursive: true }); await cp(snapshot, work, { recursive: true });
        return { root, home, work, project };
      },
      conversation: async () => { throw new Error("Reference started an agent."); },
      build: async (_isolation, space) => {
        builds += 1; await mkdir(join(space.work, "dist")); await writeFile(join(space.work, BuiltPage), "<html>Reference</html>");
        return { passed: true, typecheck: true, seconds: 0, packages: 0, html: BuiltPage, bytes: 1, log: "Fixture verifier." };
      },
      checks: async () => {
        checks += 1;
        return { checks: [{ id: "fixture-check", area: "rules", status: CheckStatus.pass, evidence: "Fixture check." }], shots: [], performance: null };
      },
      judge: async (_suite, choice, folder) => {
        judges.push(modelKey(choice)); assert.equal(await readFile(join(folder, SourceFile), "utf8"), SourceText);
        return [{ id: "fixture-question", area: "rules", question: "Fixture rule", yes: 3, no: 0, unsure: 0, evidence: ["Fixture source."] }];
      },
    },
  });
  assert.equal(status, 0); assert.equal(builds, 1); assert.equal(checks, 1);
  assert.deepEqual(judges, choices.judges.map(modelKey));
  const [batch] = await history.store.listBatches(); assert.ok(batch);
  assert.equal(batch.status, BatchStatus.done); assert.equal(batch.runs.length, 1);
  assert.deepEqual(batch.config.agents, []); assert.deepEqual(batch.requestedModels, []);
  assert.equal(batch.reference?.commit, Commit); assert.equal(batch.reference.specHash, fingerprint(Documents));
  assert.equal(batch.reference.contentHash, fingerprint(batch.reference.files));
  const [run] = await history.store.readRuns(batch); assert.ok(run);
  assert.equal(run.implementer, ReferenceImplementer); assert.deepEqual(run.reference, batch.reference);
  assert.equal(run.status, RunStatus.done); assert.equal(run.telemetry, null); assert.equal(run.requestedModel, null); assert.equal(run.agentSeconds, null);
  const assessments = await history.store.readAssessments(batch.id);
  assert.ok(assessments.every((item) => JSON.stringify(item.versions) === JSON.stringify({ ...FixtureVersions, spec: batch.reference?.specHash, specVersion: batch.reference?.specVersion })));
  assert.deepEqual(assessments.filter((item) => item.kind === AssessmentKind.judge).map((item) => item.requestedModel), choices.judges);
  const summary = await batchMatrixSummary(batch.id, history.store);
  assert.deepEqual(summary.pairs, []); assert.equal(summary.implementationAttempts, 0);
  assert.equal(summary.referenceHistory?.length, choices.judges.length + 1);
  assert.equal(await readFile(join(history.store.runFolder(batch.id, run.id), ResultFiles.files, SourceFile), "utf8"), SourceText);
  const exported = await exportBatch(history.store, batch.id, join(history.root, "export"));
  const experiment = JSON.parse(await readFile(join(exported, HistoryFiles.experiment), "utf8"));
  assert.deepEqual(experiment.reference, batch.reference);
  const exportedRuns = (await readFile(join(exported, HistoryFiles.runs), "utf8")).trim().split("\n").map((line) => JSON.parse(line));
  assert.equal(exportedRuns[0].implementer, ReferenceImplementer); assert.deepEqual(exportedRuns[0].reference, batch.reference);
});
