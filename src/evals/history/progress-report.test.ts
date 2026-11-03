import assert from "node:assert/strict";
import { test } from "node:test";
import { judgeAssessment } from "./assessments.ts";
import { batchProgress, checkedWithoutErrors, progressLines } from "./progress-report.ts";
import { FailureKind, RunStatus } from "./statuses.ts";
import { FixtureVersions, v2Fixture } from "./test-fixtures.ts";

const Judge = { tool: "codex", model: "judge-fixture", effort: "xhigh", repeats: 3 } as const;
const Coverage = { passed: 3, applicable: 4, blocked: 0, errors: 0, share: 0.75 } as const;

test("progress counts runs whose agent finished, checked and judged runs, leaving out replaced failures and runs waiting to be redone", () => {
  const { batch: fixture, run } = v2Fixture();
  const setup = run.setup;
  const batch = { ...fixture, config: { ...fixture.config, agents: [run.agent], setups: [setup], runsPerSetup: 3 } };
  const finished = { ...run, id: "finished", status: RunStatus.done, agentSeconds: 60, finishedAt: run.startedAt, replacesRunId: null, failureKind: null,
    functionalResult: { coverage: Coverage, workingGate: true, statedSuccess: false } };
  const unchecked = { ...finished, id: "unchecked", functionalResult: { coverage: { ...Coverage, errors: 1 }, workingGate: null, statedSuccess: null } };
  const failed = { ...finished, id: "failed", status: RunStatus.failed, failureKind: FailureKind.infrastructure };
  const replacement = { ...finished, id: "replacement", status: RunStatus.agent, agentSeconds: null, finishedAt: null, replacesRunId: "failed" };
  const limited = { ...finished, id: "limited", status: RunStatus.limited, failureKind: FailureKind.infrastructure };
  const grading = { ...finished, id: "grading", status: RunStatus.checking, finishedAt: null, functionalResult: null, build: finished.build === null ? null : { ...finished.build, passed: false } };
  const answers = [{ id: "rule", area: "rules", question: "Rule", yes: 2, no: 0, unsure: 1, evidence: [] }];
  const judged = { ...judgeAssessment(finished, answers, FixtureVersions), requestedModel: Judge };
  const errored = { ...judgeAssessment(unchecked, answers, FixtureVersions), requestedModel: Judge, error: "Fixture judge failure." };
  const progress = batchProgress(batch, [finished, unchecked, failed, replacement, limited, grading], [judged, errored]);
  assert.deepEqual([progress.planned, progress.implemented, progress.checked], [3, 3, 1]);
  assert.equal(progress.built, 2);
  assert.deepEqual(progress.judges.map((judge) => judge.judged), [1]);
  assert.deepEqual(progress.setups.map((row) => [row.implemented, row.checked, row.coverage, row.judges[0]?.judged, row.judges[0]?.yesShare]), [[3, 1, 0.75, 1, 2 / 3]]);
  assert.equal(progressLines(progress).length, 4);
});

test("a run counts as checked, and check --missing leaves it, only when its latest check graded a clause without errors", () => {
  const functional = { coverage: Coverage, workingGate: true, statedSuccess: false };
  assert.equal(checkedWithoutErrors({ functionalResult: functional }), true);
  assert.equal(checkedWithoutErrors({ functionalResult: { ...functional, coverage: { ...Coverage, errors: 1 } } }), false);
  assert.equal(checkedWithoutErrors({ functionalResult: { ...functional, coverage: { ...Coverage, applicable: 0 } } }), false);
  assert.equal(checkedWithoutErrors({ functionalResult: null }), false);
});
