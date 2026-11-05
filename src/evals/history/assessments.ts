import { randomUUID } from "node:crypto";
import { adaptCheck, countChecks, uniqueChecks } from "../grading/counting.ts";
import type { CheckOutcome, CheckSelection } from "../grading/grading.types.ts";
import type { AssessmentRecord, AssessmentVersions, ChecksAssessment, JudgeAssessment, RunRecord, RunView } from "./records.types.ts";
import type { CheckResult, JudgeAnswer } from "./results.types.ts";
import { AssessmentKind, CheckStatus, IntentCheckArea, RecordSchemaVersion } from "./statuses.ts";
import type { JudgeReport } from "../grading/judge/report.ts";

export function defaultSelection(outcomes: readonly CheckOutcome[]): CheckSelection {
  const stated = [...new Set(outcomes.filter((check) => check.area !== IntentCheckArea).map((check) => check.id))];
  return { applicable: stated, working: [], stated };
}

export function assessmentBase(experimentId: string, versions: AssessmentVersions, replacesAssessmentId: string | null = null) {
  return { schemaVersion: RecordSchemaVersion, id: randomUUID(), experimentId, createdAt: new Date().toISOString(), versions, replacesAssessmentId, error: null } as const;
}

export function checksAssessment(run: RunView, checks: readonly (CheckResult | CheckOutcome)[], versions: AssessmentVersions, selection?: CheckSelection): ChecksAssessment {
  const outcomes = uniqueChecks(checks.map(adaptCheck));
  return {
    ...assessmentBase(run.batchId, versions, run.checksAssessmentId), kind: AssessmentKind.checks, runId: run.id,
    outcomes, selection: selection ?? defaultSelection(outcomes), build: run.build, performance: run.performance, shots: run.shots,
  };
}

export function failedChecksAssessment(run: RunView, error: unknown, versions: AssessmentVersions): ChecksAssessment {
  const evidence = error instanceof Error ? error.message : String(error);
  const byId = new Map(run.checkOutcomes.map((check) => [check.id, check]));
  const ids = new Set([...run.checkSelection.applicable, ...byId.keys()]);
  return { ...checksAssessment(run, [], versions, run.checkSelection), error: evidence,
    outcomes: [...ids].map((id) => ({ id, area: byId.get(id)?.area ?? "Grading", status: CheckStatus.error, evidence })) };
}

export function judgeAssessment(run: RunView, answers: readonly JudgeAnswer[], versions: AssessmentVersions, report?: JudgeReport): JudgeAssessment {
  return {
    ...assessmentBase(run.batchId, versions, run.judgeAssessmentId), kind: AssessmentKind.judge, runId: run.id,
    answers, requestedModel: null, reportedModel: null,
    formatRetries: report?.formatRetries ?? 0, ballotFailures: report?.failures ?? [], ...report?.scoring,
  };
}

export interface AssessmentSelection {
  readonly checks?: string;
  readonly judge?: string;
}

export function assessRun(run: RunRecord, assessments: readonly AssessmentRecord[], selected: AssessmentSelection = {}): RunView {
  let checks: ChecksAssessment | undefined;
  let judge: JudgeAssessment | undefined;
  for (const assessment of assessments) {
    if (assessment.kind === AssessmentKind.pick || assessment.runId !== run.id) continue;
    if (assessment.kind === AssessmentKind.checks && (selected.checks === undefined || selected.checks === assessment.id)) checks = assessment;
    if (assessment.kind === AssessmentKind.judge && (selected.judge === undefined || selected.judge === assessment.id)) judge = assessment;
  }
  if ((selected.checks !== undefined && checks === undefined) || (selected.judge !== undefined && judge === undefined)) throw new Error("The named assessment doesn't belong to this run and kind.");
  const outcomes = checks?.outcomes ?? [];
  const selection = checks?.selection ?? defaultSelection(outcomes);
  return {
    ...run, status: run.pipelineStatus, functionalResult: checks === undefined ? run.functionalResult : countChecks(outcomes, selection),
    build: checks?.build ?? run.build, performance: checks?.performance ?? run.performance, shots: checks?.shots ?? run.shots,
    checks: outcomes.map((check) => ({ id: check.id, area: check.area, detail: check.evidence, passed: check.status === CheckStatus.pass ? true : check.status === CheckStatus.fail ? false : null })),
    checkOutcomes: outcomes, checkSelection: selection, judge: judge?.answers ?? [],
    checksAssessmentId: checks?.id ?? null, judgeAssessmentId: judge?.id ?? null, checksError: checks?.error ?? null, judgeError: judge?.error ?? null,
  };
}

export function storedRun(view: RunView): RunRecord {
  const { status, checks, judge, checkOutcomes, checkSelection, checksAssessmentId, judgeAssessmentId, checksError, judgeError, ...run } = view;
  return { ...run, pipelineStatus: status };
}
