import { mkdtemp, rm } from "node:fs/promises";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { judgeRun } from "./judge/judge.ts";
import type { JudgeOptions } from "./judge/judge.ts";
import type { JudgeReport } from "./judge/report.ts";
import { prepareJudgeFolder } from "./judge/judge-folder.ts";
import { judgeImagePaths } from "./judge/shot-paths.ts";
import { ResultFiles } from "../paths.ts";
import { judgeAssessment } from "../history/assessments.ts";
import { gradingVersions } from "../history/capture.ts";
import type { AssessmentVersions, JudgeAssessment, RunView } from "../history/records.types.ts";
import type { JudgeAnswer } from "../history/results.types.ts";
import { appendAssessment, readAssessments, readBatch, readRuns, runFolder, type HistoryStore } from "../history/store.ts";
import { loadSuite } from "../benchmarks/suite.ts";
import type { Suite } from "../benchmarks/suite.types.ts";
import type { EvalConfig, JudgeChoice } from "../config/config.types.ts";
import { configuredJudges } from "../config/judges.ts";
import { Flag } from "../config/commands.ts";
import { loadPlan } from "../config/plans.ts";
import { latestJudgeFor } from "../history/summary.ts";
import { FailureKind } from "../history/statuses.ts";
import { runInParallel } from "../execution/run-queue.ts";

const JudgeTimeoutMs = 15 * 60_000;

export interface RejudgeDependencies {
  readonly evaluate: (run: RunView, onReport: (report: JudgeReport) => void) => Promise<readonly JudgeAnswer[]>;
  readonly append: (assessment: JudgeAssessment) => Promise<void>;
  readonly choice?: JudgeChoice;
  readonly previous?: JudgeAssessment;
}

export async function rejudgeRun(run: RunView, versions: AssessmentVersions, dependencies: RejudgeDependencies): Promise<JudgeAssessment> {
  let assessment: JudgeAssessment;
  let report: JudgeReport | undefined;
  try {
    const answers = await dependencies.evaluate(run, (received) => { report = received; });
    assessment = judgeAssessment(run, answers, versions, report);
  } catch (error) {
    assessment = { ...judgeAssessment(run, [], versions, report), error: error instanceof Error ? error.message : String(error) };
  }
  if (dependencies.choice !== undefined) assessment = { ...assessment, requestedModel: dependencies.choice, replacesAssessmentId: dependencies.previous?.id ?? null };
  await dependencies.append(assessment);
  return assessment;
}

export interface SavedJudgeServices {
  readonly store?: Pick<HistoryStore, "runFolder" | "readAssessments" | "appendAssessment">;
  readonly judge?: typeof judgeRun;
  readonly scratch?: () => Promise<string>;
  readonly options?: JudgeOptions;
}

export async function judgeSavedRun(suite: Suite, run: RunView, choice: JudgeChoice, versions: AssessmentVersions, services: SavedJudgeServices = {}, missingOnly = false, legacyChoice?: JudgeChoice): Promise<JudgeAssessment> {
  const store = services.store ?? { runFolder, readAssessments, appendAssessment };
  const previous = latestJudgeFor(run.id, await store.readAssessments(run.batchId), choice, legacyChoice);
  if (missingOnly && previous !== undefined) return previous;
  const scratch = await (services.scratch ?? (() => mkdtemp(join(tmpdir(), "eval-rejudge-"))))();
  try {
    return await rejudgeRun(run, versions, {
      choice, ...(previous === undefined ? {} : { previous }), append: (assessment) => store.appendAssessment(assessment),
      evaluate: async (_run, onReport) => {
        await prepareJudgeFolder(join(store.runFolder(run.batchId, run.id), ResultFiles.files), join(scratch, "project"), run.build?.html);
        return (services.judge ?? judgeRun)(suite, choice, join(scratch, "project"), join(scratch, "home"), JudgeTimeoutMs,
          { ...services.options, images: judgeImagePaths(run), onReport: (report) => { onReport(report); services.options?.onReport?.(report); } });
      },
    });
  } finally { await rm(scratch, { recursive: true, force: true }); }
}

export async function completeSavedJudgments(suite: Suite, run: RunView, config: EvalConfig, versions: AssessmentVersions, services: SavedJudgeServices = {}): Promise<void> {
  if (run.startedAt === null || run.finishedAt === null || run.failureKind === FailureKind.infrastructure) return;
  const options = services.options ?? { stage: { ...(config.containers === undefined ? {} : { limits: config.containers }) } };
  for (const choice of configuredJudges(config)) await judgeSavedRun(suite, run, choice, versions, { ...services, options }, true, config.judge);
}

export async function rejudgeRuns(suite: Suite, runs: readonly RunView[], config: EvalConfig, versions: AssessmentVersions, services: SavedJudgeServices = {}, onlyRun?: string): Promise<void> {
  const selected = runs.filter((run) => run.startedAt !== null && (onlyRun === undefined || run.id === onlyRun));
  const options = services.options ?? { stage: { ...(config.containers === undefined ? {} : { limits: config.containers }) } };
  await runInParallel(selected, config.parallelRuns || 1, async (run) => {
    for (const choice of configuredJudges(config)) await judgeSavedRun(suite, run, choice, versions, { ...services, options }, false, config.judge);
  }, () => false);
}

/** A plan's judges replace the batch's saved ones, so saved attempts can be judged by the judges a plan uses now. */
export function withPlanJudges(saved: EvalConfig, plan: EvalConfig): EvalConfig {
  return { ...saved, judge: plan.judge, judges: configuredJudges(plan) };
}

async function main(): Promise<void> {
  const args = process.argv.slice(2), at = args.indexOf(Flag.plan), planPath = at < 0 ? undefined : args[at + 1];
  const [batchId, onlyRun] = args.filter((_, index) => at < 0 || (index !== at && index !== at + 1));
  if (batchId === undefined) throw new Error("Name the batch to judge again.");
  const batch = await readBatch(batchId), suite = await loadSuite(batch.suite);
  const config = planPath === undefined ? batch.config : withPlanJudges(batch.config, await loadPlan(planPath));
  await rejudgeRuns(suite, await readRuns(batch), config, await gradingVersions(suite), {}, onlyRun);
}

if (import.meta.main) await main();
