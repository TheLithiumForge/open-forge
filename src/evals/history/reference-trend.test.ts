import assert from "node:assert/strict";
import { test } from "node:test";
import { SpecVersion } from "../benchmarks/neon-pairs/checks/spec-layers/requirements.ts";
import { MatrixJudges } from "../config/matrix.ts";
import { checksAssessment, judgeAssessment } from "./assessments.ts";
import { FixtureVersions, temporaryHistory, v2Fixture } from "./test-fixtures.ts";
import { ReferenceImplementer } from "./reference.ts";
import { referenceHistory, referenceTrends, type ReferenceHistory } from "./reference-trend.ts";
import { summarize } from "./summary.ts";
import { summarizeMatrix } from "./summary-matrix.ts";
import type { RunRecord } from "./results.types.ts";
import { AssessmentKind } from "./statuses.ts";

const Reference = { commit: "c".repeat(40), contentHash: "source-version", specHash: "spec-version", specVersion: SpecVersion, files: [] };
const Scores = [{ id: "fixture-question", area: "visual", question: "Fixture visual", yes: 3, no: 0, unsure: 0, evidence: [] }];

function referenceFixture(): ReferenceHistory {
  const { batch, run } = v2Fixture();
  const referenceRun: RunRecord = { ...run, reference: Reference, implementer: ReferenceImplementer };
  const checked = checksAssessment(referenceRun, run.checks, FixtureVersions);
  const judged = { ...judgeAssessment(referenceRun, Scores, FixtureVersions), requestedModel: MatrixJudges[0], ballots: MatrixJudges[0].repeats };
  return { batch: { ...batch, reference: Reference, config: { ...batch.config, layer: 3, agents: [] } }, runs: [referenceRun], assessments: [checked, judged] };
}

test("reference trends keep all assessments and split every changed grading input", () => {
  const original = referenceFixture(), repeated = referenceFixture();
  const initial = referenceTrends([original, repeated]);
  assert.equal(initial.length, 2); assert.ok(initial.every((group) => group.points.length === 2));
  assert.equal(initial.find((group) => group.grading.model !== null)?.points[0]?.scores[Scores[0]?.area ?? ""], 1);
  const judge = original.assessments[1]; assert.ok(judge?.kind === AssessmentKind.judge);
  const changes = [
    { ...judge, id: "checker-change", versions: { ...judge.versions, checker: "changed" } },
    { ...judge, id: "rubric-change", versions: { ...judge.versions, rubric: "changed" } },
    { ...judge, id: "prompt-change", versions: { ...judge.versions, judge: "changed" } },
    { ...judge, id: "spec-change", versions: { ...judge.versions, spec: "changed" } },
    { ...judge, id: "model-change", requestedModel: MatrixJudges[1] },
    { ...judge, id: "reported-model-change", reportedModel: "reported-judge" },
    { ...judge, id: "ballot-change", ballots: MatrixJudges[0].repeats + 1 },
  ];
  const changed = referenceTrends([{ ...original, assessments: [...original.assessments, ...changes] }]);
  assert.equal(changed.length, initial.length + changes.length);
  assert.equal(changed.find((group) => group.grading.checker === "changed")?.points[0]?.coverage, null);
  const newSource = { ...Reference, contentHash: "changed-source" };
  const sourceHistory = { ...original, batch: { ...original.batch, reference: newSource }, runs: original.runs.map((run) => ({ ...run, reference: newSource })) };
  assert.equal(referenceTrends([original, sourceHistory]).length, initial.length * 2);
  const appended = { ...judge, id: "appended", replacesAssessmentId: judge.id };
  const kept = referenceTrends([{ ...original, assessments: [...original.assessments, appended] }]);
  assert.equal(kept.find((group) => group.grading.model !== null)?.points.length, 2);
});

test("reference grades stay outside setup summaries, paired games and the model matrix", async (t) => {
  const history = await temporaryHistory(); t.after(history.cleanup);
  const reference = referenceFixture(), { run } = v2Fixture();
  const referenceRun = reference.runs[0]; assert.ok(referenceRun);
  assert.deepEqual(summarize([run, referenceRun], []), summarize([run], []));
  assert.deepEqual(summarizeMatrix(FixtureConfigFor(run), [run, referenceRun], reference.assessments, []),
    summarizeMatrix(FixtureConfigFor(run), [run], reference.assessments, []));
  const { reference: _reference, ...modelBatch } = reference.batch;
  assert.deepEqual(referenceTrends([{ ...reference, batch: modelBatch }]), []);
});

function FixtureConfigFor(run: ReturnType<typeof v2Fixture>["run"]) {
  return { ...v2Fixture().batch.config, agents: [run.agent] };
}

test("saved reference history keeps the version groups of its records", async (t) => {
  const history = await temporaryHistory(); t.after(history.cleanup);
  const reference = referenceFixture();
  await history.store.saveBatch(reference.batch);
  for (const run of reference.runs) await history.store.saveRun(run);
  for (const assessment of reference.assessments) await history.store.appendAssessment(assessment);
  const groups = await referenceHistory(history.store);
  assert.deepEqual(groups, referenceTrends([reference]));
});
