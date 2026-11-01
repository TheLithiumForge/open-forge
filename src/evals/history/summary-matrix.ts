import type { EvalConfig, ModelChoice } from "../config/config.types.ts";
import { configuredJudges, modelKey } from "../config/judges.ts";
import { summarizeMeasurements } from "../grading/counting.ts";
import type { AssessmentRecord, AssessmentVersions, JudgeAssessment } from "./records.types.ts";
import type { Preference, RunRecord } from "./results.types.ts";
import { AssessmentKind, RunStatus } from "./statuses.ts";
import { ReferenceImplementer } from "./reference.ts";
import { agentLabel, JudgeAreas, latestJudgeFor, summarize, yesShare, type SetupSummary } from "./summary.ts";

export interface PairSummary {
  readonly layer: number | null;
  readonly implementer: string;
  readonly judge: string;
  readonly grading: string;
  readonly reportedModel: string | null;
  readonly reportedJudgeModel: string | null;
  readonly judgeMissing: number;
  readonly judgeErrors: number;
  readonly runs: readonly RunRecord[];
  readonly summary: readonly SetupSummary[];
}
export interface PairedScore { readonly difference: number | null; readonly samples: number; readonly missing: number }
export interface JudgeDifference {
  readonly layer: number | null; readonly implementer: string; readonly setup: string; readonly grading: string;
  readonly reportedModel: string | null;
  readonly firstJudge: string; readonly secondJudge: string; readonly attempts: number;
  readonly missing: number; readonly errors: number; readonly incompatible: number;
  readonly rules: PairedScore; readonly code: PairedScore; readonly intent: PairedScore;
}
export interface MatrixSummary { readonly layer: number | null; readonly implementationAttempts: number; readonly pairs: readonly PairSummary[]; readonly differences: readonly JudgeDifference[] }

export function summaryGradingKey(run: RunRecord, assessments: readonly AssessmentRecord[], judgeId = run.judgeAssessmentId): string {
  const checks = assessments.find((item) => item.id === run.checksAssessmentId);
  const judge = assessments.find((item) => item.id === judgeId);
  return JSON.stringify([versionKey(checks?.versions), versionKey(judge?.versions)]);
}

function versionKey(versions: AssessmentVersions | undefined) {
  return versions === undefined ? null : [versions.checker, versions.rubric, versions.judge];
}

function uniqueModels(choices: readonly ModelChoice[]): ModelChoice[] {
  return [...new Map(choices.map((choice) => [modelKey(choice), choice])).values()];
}
function attempted(run: RunRecord): boolean {
  return run.startedAt !== null || run.status === RunStatus.done || run.status === RunStatus.failed;
}

export function summarizeMatrix(config: EvalConfig, input: readonly RunRecord[], assessments: readonly AssessmentRecord[], preferences: readonly Preference[]): MatrixSummary {
  const runs = input.filter((run) => run.implementer !== ReferenceImplementer);
  const layer = config.layer ?? null, pairs: PairSummary[] = [];
  const agents = uniqueModels([...config.agents, ...runs.map((run) => run.agent)]);
  const judges = uniqueModels([...configuredJudges(config), ...assessments.flatMap((item) => item.kind === AssessmentKind.judge && item.requestedModel !== null ? [item.requestedModel] : [])]);
  for (const agent of agents) for (const choice of judges) {
    const groups = new Map<string, RunRecord[]>();
    for (const run of runs.filter((run) => modelKey(run.agent) === modelKey(agent))) {
      const selected = latestJudgeFor(run.id, assessments, choice, config.judge);
      const inherited = modelKey(choice) === modelKey(config.judge) && !assessments.some((item) => item.kind === AssessmentKind.judge && item.runId === run.id);
      const answers = selected === undefined ? inherited && run.judgeError === null ? run.judge : [] : selected.error === null ? selected.answers : [];
      const view = { ...run, judge: answers, judgeAssessmentId: selected?.id ?? (inherited ? run.judgeAssessmentId : null), judgeError: selected?.error ?? (inherited ? run.judgeError : null) };
      const key = JSON.stringify([summaryGradingKey(view, assessments), run.reportedModel, selected?.reportedModel ?? null]);
      groups.set(key, [...groups.get(key) ?? [], view]);
    }
    if (!groups.size) groups.set(JSON.stringify([null, null, null]), []);
    for (const members of groups.values()) {
      const first = members[0], selected = assessments.find((item) => item.kind === AssessmentKind.judge && item.id === first?.judgeAssessmentId);
      pairs.push({ layer, implementer: agentLabel(agent), judge: agentLabel(choice), grading: first === undefined ? "ungraded" : summaryGradingKey(first, assessments),
        reportedModel: first?.reportedModel ?? null, reportedJudgeModel: selected?.kind === AssessmentKind.judge ? selected.reportedModel : null,
        judgeMissing: members.filter((run) => attempted(run) && run.judgeAssessmentId === null).length,
        judgeErrors: members.filter((run) => run.judgeError !== null).length, runs: members, summary: summarize(members, preferences) });
    }
  }
  return { layer, implementationAttempts: runs.filter(attempted).length, pairs, differences: judgeDifferences(config, runs, assessments) };
}

interface PairedAttempt { readonly run: RunRecord; readonly first: JudgeAssessment | undefined; readonly second: JudgeAssessment | undefined }
function comparable(pair: PairedAttempt): boolean {
  return pair.first !== undefined && pair.second !== undefined && pair.first.error === null && pair.second.error === null
    && JSON.stringify(versionKey(pair.first.versions)) === JSON.stringify(versionKey(pair.second.versions));
}
function pairedScore(pairs: readonly PairedAttempt[], area: string): PairedScore {
  const values = pairs.map((pair) => {
    if (!comparable(pair)) return null;
    const first = yesShare(pair.first?.answers ?? [], area), second = yesShare(pair.second?.answers ?? [], area);
    return first === null || second === null ? null : second - first;
  });
  const result = summarizeMeasurements(values);
  return { difference: result.mean, samples: result.samples, missing: result.missing };
}
function judgeDifferences(config: EvalConfig, runs: readonly RunRecord[], assessments: readonly AssessmentRecord[]): JudgeDifference[] {
  const judges = configuredJudges(config), rows: JudgeDifference[] = [];
  for (const [index, first] of judges.entries()) for (const second of judges.slice(index + 1)) {
    const groups = new Map<string, PairedAttempt[]>();
    for (const run of runs.filter(attempted)) {
      const pair = { run, first: latestJudgeFor(run.id, assessments, first, config.judge), second: latestJudgeFor(run.id, assessments, second, config.judge) };
      const key = JSON.stringify([modelKey(run.agent), run.setup, run.reportedModel, summaryGradingKey(run, assessments, null), versionKey(pair.first?.versions), versionKey(pair.second?.versions)]);
      groups.set(key, [...groups.get(key) ?? [], pair]);
    }
    for (const pairs of groups.values()) {
      const pair = pairs[0]; if (!pair) continue;
      rows.push({ layer: config.layer ?? null, implementer: agentLabel(pair.run.agent), reportedModel: pair.run.reportedModel, setup: pair.run.setup,
        grading: JSON.stringify([summaryGradingKey(pair.run, assessments, null), versionKey(pair.first?.versions), versionKey(pair.second?.versions)]), firstJudge: agentLabel(first), secondJudge: agentLabel(second), attempts: pairs.length,
        missing: pairs.filter((pair) => pair.first === undefined || pair.second === undefined).length,
        errors: pairs.filter((pair) => pair.first?.error != null || pair.second?.error != null).length,
        incompatible: pairs.filter((pair) => pair.first !== undefined && pair.second !== undefined && JSON.stringify(versionKey(pair.first.versions)) !== JSON.stringify(versionKey(pair.second.versions))).length,
        rules: pairedScore(pairs, JudgeAreas.rules), code: pairedScore(pairs, JudgeAreas.code), intent: pairedScore(pairs, JudgeAreas.intent) });
    }
  }
  return rows;
}
