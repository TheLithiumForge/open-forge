import assert from "node:assert/strict";
import { test } from "node:test";
import { MatrixAgents, MatrixJudges } from "../config/matrix.ts";
import { modelKey } from "../config/judges.ts";
import { assessRun, checksAssessment, judgeAssessment } from "./assessments.ts";
import { FixtureVersions, v2Fixture } from "./test-fixtures.ts";
import { agentLabel, JudgeAreas } from "./summary.ts";
import { summarizeMatrix } from "./summary-matrix.ts";
import { AssessmentKind, FailureKind } from "./statuses.ts";
import type { AssessmentRecord, JudgeAssessment } from "./records.types.ts";

function fixture() {
  const { batch, run } = v2Fixture();
  const config = { ...batch.config, layer: 3, agents: MatrixAgents, judge: MatrixJudges[0], judges: MatrixJudges };
  const runs = MatrixAgents.map((agent, index) => ({ ...run, id: `${run.id}-${index}`, agent, reportedModel: agent.model }));
  const assessments: AssessmentRecord[] = [];
  for (const attempt of runs) {
    assessments.push(checksAssessment(attempt, attempt.checks, FixtureVersions));
    for (const choice of MatrixJudges) {
      const ownFamily = choice.tool === attempt.agent.tool;
      const answers = Object.values(JudgeAreas).map((area) => ({ id: area, area, question: "Fixture question", yes: ownFamily ? 3 : 0, no: ownFamily ? 0 : 3, unsure: 0, evidence: ["Fixture evidence."] }));
      assessments.push({ ...judgeAssessment(attempt, answers, FixtureVersions), requestedModel: choice });
    }
  }
  const views = (records: readonly AssessmentRecord[] = assessments) => runs.map((run) => assessRun(run, records));
  return { batch, config, runs, assessments, views };
}

test("the four pairs keep judge scores separate and compare the same two implementation attempts", () => {
  const data = fixture(), report = summarizeMatrix(data.config, data.views(), data.assessments, []);
  assert.equal(report.pairs.length, MatrixAgents.length * MatrixJudges.length);
  assert.equal(report.implementationAttempts, data.runs.length);
  assert.ok(report.pairs.every((pair) => pair.layer === data.config.layer && pair.runs.length === 1));
  for (const agent of MatrixAgents) for (const choice of MatrixJudges) {
    const pair = report.pairs.find((pair) => pair.implementer === agentLabel(agent) && pair.judge === agentLabel(choice));
    assert.equal(pair?.summary[0]?.rules, agent.tool === choice.tool ? 1 : 0);
    assert.equal(pair?.summary[0]?.coverage, 1);
    assert.equal(pair?.summary[0]?.tokenMissing, 1);
    assert.equal(pair?.summary[0]?.attempts, 1);
  }
  for (const agent of MatrixAgents) {
    const row = report.differences.find((row) => row.implementer === agentLabel(agent));
    assert.equal(row?.rules.difference, agent.tool === MatrixJudges[1].tool ? 1 : -1);
    assert.equal(row?.rules.samples, 1);
    assert.equal(row?.attempts, 1);
  }
});

test("each judge keeps its latest assessment while missing and failed judgments stay counted", () => {
  const data = fixture(), run = data.runs[0]; assert.ok(run);
  const judgments = data.assessments.filter((item): item is JudgeAssessment => item.kind === AssessmentKind.judge && item.runId === run.id);
  const first = judgments.find((item) => item.requestedModel !== null && modelKey(item.requestedModel) === modelKey(MatrixJudges[0])); assert.ok(first);
  const second = judgments.find((item) => item.requestedModel !== null && modelKey(item.requestedModel) === modelKey(MatrixJudges[1])); assert.ok(second);
  const repeated = { ...first, id: `${first.id}-next`, answers: first.answers.map((answer) => ({ ...answer, yes: 3, no: 0 })) };
  const updated = [...data.assessments, repeated];
  const selected = summarizeMatrix(data.config, data.views(updated), updated, []).pairs.find((pair) => pair.implementer === agentLabel(run.agent) && pair.judge === agentLabel(MatrixJudges[0]));
  assert.equal(selected?.summary[0]?.rules, 1);
  assert.equal(selected?.runs[0]?.judgeAssessmentId, repeated.id);
  const missing = data.assessments.filter((item) => item.id !== second.id);
  const absent = summarizeMatrix(data.config, data.views(missing), missing, []);
  assert.equal(absent.pairs.find((pair) => pair.implementer === agentLabel(run.agent) && pair.judge === agentLabel(MatrixJudges[1]))?.judgeMissing, 1);
  assert.equal(absent.differences.find((row) => row.implementer === agentLabel(run.agent))?.rules.samples, 0);
  const failed = [...data.assessments, { ...second, id: `${second.id}-failed`, answers: [], error: "Fixture judge fault." }];
  const errored = summarizeMatrix(data.config, data.views(failed), failed, []);
  assert.equal(errored.pairs.find((pair) => pair.implementer === agentLabel(run.agent) && pair.judge === agentLabel(MatrixJudges[1]))?.judgeErrors, 1);
  assert.equal(errored.differences.find((row) => row.implementer === agentLabel(run.agent))?.errors, 1);
});

test("different grading versions cannot contribute a paired score and a legacy judge stays readable", () => {
  const data = fixture(), last = data.assessments.at(-1); assert.ok(last?.kind === AssessmentKind.judge);
  const changed = [...data.assessments, { ...last, id: `${last.id}-grading`, versions: { ...last.versions, rubric: `${FixtureVersions.rubric}-next` } }];
  const report = summarizeMatrix(data.config, data.views(changed), changed, []);
  const incompatible = report.differences.find((row) => row.implementer === agentLabel(data.runs[1]?.agent ?? MatrixAgents[1]));
  assert.equal(incompatible?.incompatible, 1);
  assert.equal(incompatible?.rules.samples, 0);
  const { batch, run } = v2Fixture();
  const legacy = summarizeMatrix(batch.config, [run], [], []);
  assert.equal(legacy.pairs.length, 1);
  assert.equal(legacy.differences.length, 0);
  assert.equal(legacy.pairs[0]?.judge, agentLabel(batch.config.judge));
});

test("attempts with different check versions stay in separate pair and difference groups", () => {
  const data = fixture(), original = data.runs[0]; assert.ok(original);
  const other = { ...original, id: `${original.id}-different-checks` };
  const checking = checksAssessment(other, other.checks, { ...FixtureVersions, checker: `${FixtureVersions.checker}-next` });
  const judgments = data.assessments.filter((item): item is JudgeAssessment => item.kind === AssessmentKind.judge && item.runId === original.id)
    .map((item) => ({ ...item, id: `${item.id}-different-checks`, runId: other.id }));
  const records = [...data.assessments, checking, ...judgments], views = [...data.runs, other].map((run) => assessRun(run, records));
  const report = summarizeMatrix(data.config, views, records, []);
  const pairs = report.pairs.filter((pair) => pair.implementer === agentLabel(original.agent));
  assert.equal(pairs.length, MatrixJudges.length * 2);
  assert.ok(pairs.every((pair) => pair.runs.length === 1));
  const differences = report.differences.filter((row) => row.implementer === agentLabel(original.agent));
  assert.equal(differences.length, 2);
  assert.equal(new Set(differences.map((row) => row.grading)).size, differences.length);
  assert.ok(differences.every((row) => row.rules.samples === 1));
});

test("an attempt replaced after an infrastructure failure leaves the pairs to its replacement", () => {
  const data = fixture(), [first] = data.runs;
  assert.ok(first);
  const stopped = { ...first, id: `${first.id}-stopped`, failureKind: FailureKind.infrastructure };
  const replacement = { ...first, replacesRunId: stopped.id };
  const runs = [assessRun(stopped, data.assessments), ...data.views().map((run) => run.id === first.id ? assessRun(replacement, data.assessments) : run)];
  const report = summarizeMatrix(data.config, runs, data.assessments, []);
  assert.equal(report.implementationAttempts, data.runs.length);
  assert.ok(report.pairs.every((pair) => pair.runs.every((run) => run.id !== stopped.id)));
  assert.equal(report.pairs.length, MatrixAgents.length * MatrixJudges.length);
});
