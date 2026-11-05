import { existsSync } from "node:fs";
import { cp, mkdir, mkdtemp, rm } from "node:fs/promises";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { judgeRun } from "./judge/judge.ts";
import type { JudgeOptions } from "./judge/judge.ts";
import type { JudgeReport } from "./judge/report.ts";
import { prepareJudgeFolder } from "./judge/judge-folder.ts";
import { JudgeLimited } from "./judge/reply.ts";
import { judgeImagePaths } from "./judge/shot-paths.ts";
import { capturePlaythrough, casePlaythrough, PlaythroughFiles, savedPlaythrough, type Playthrough } from "./judge/playthrough.ts";
import { ResultFiles } from "../paths.ts";
import { judgeAssessment } from "../history/assessments.ts";
import { gradingVersions } from "../history/capture.ts";
import type { AssessmentVersions, JudgeAssessment, RunView } from "../history/records.types.ts";
import type { JudgeAnswer } from "../history/results.types.ts";
import { appendAssessment, assessmentEvidenceFolder, readAssessments, readBatch, readRuns, runFolder, type HistoryStore } from "../history/store.ts";
import { loadSuite } from "../benchmarks/suite.ts";
import type { Suite } from "../benchmarks/suite.types.ts";
import type { EvalConfig, JudgeChoice } from "../config/config.types.ts";
import { configuredJudges } from "../config/judges.ts";
import { judgeOptions } from "../config/preset-options.ts";
import { latestJudgeFor } from "../history/summary.ts";
import { FailureKind } from "../history/statuses.ts";
import { runInParallel } from "../execution/run-queue.ts";
import { applyMachineOptions, splitMachineOptions } from "../config/machine-options.ts";
import { requireSignIns } from "../config/auth.ts";

const JudgeTimeoutMs = 30 * 60_000;

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
    // A usage limit leaves the judgment missing, so resume or a later rejudge can still add it.
    if (error instanceof JudgeLimited) throw error;
    assessment = { ...judgeAssessment(run, [], versions, report), error: error instanceof Error ? error.message : String(error) };
  }
  if (dependencies.choice !== undefined) assessment = { ...assessment, requestedModel: dependencies.choice, ballots: dependencies.choice.repeats, replacesAssessmentId: dependencies.previous?.id ?? null };
  await dependencies.append(assessment);
  return assessment;
}

export interface SavedJudgeServices {
  readonly store?: Pick<HistoryStore, "runFolder" | "readAssessments" | "appendAssessment">;
  readonly judge?: typeof judgeRun;
  readonly capture?: typeof capturePlaythrough;
  readonly scratch?: () => Promise<string>;
  readonly options?: JudgeOptions;
}

/** The game the checks played, which a run that was built again keeps with its check. */
function builtGame(run: RunView, folder: string): string | null {
  const games = [...(run.checksAssessmentId === null ? [] : [join(assessmentEvidenceFolder(run.batchId, run.checksAssessmentId), ResultFiles.game)]), join(folder, ResultFiles.game)];
  return games.find((game) => existsSync(game)) ?? null;
}

/** Whether a judge has what this case asks of it. A case that plays its game has nothing to play in a run that never built. */
function judgeable(suite: Suite, run: RunView): boolean {
  return !existsSync(casePlaythrough(suite.id)) || (run.build?.passed !== false && builtGame(run, runFolder(run.batchId, run.id)) !== null);
}

/** One capture per run beside the batch's runs, so every judge reads the same evidence, and later ballots can too. */
async function playthroughOf(suite: Suite, run: RunView, folder: string, services: SavedJudgeServices): Promise<Playthrough | null> {
  if (!existsSync(casePlaythrough(suite.id))) return null;
  const target = join(folder, "..", "..", PlaythroughFiles.folder, run.id);
  const saved = await savedPlaythrough(target);
  if (saved !== null) return saved;
  const game = builtGame(run, folder);
  if (game === null) throw new Error("A run with no built game has no playthrough.");
  return (services.capture ?? capturePlaythrough)(suite, game, target, services.options?.stage ?? {});
}

export async function judgeSavedRun(suite: Suite, run: RunView, choice: JudgeChoice, versions: AssessmentVersions, services: SavedJudgeServices = {}, missingOnly = false, legacyChoice?: JudgeChoice): Promise<JudgeAssessment> {
  const store = services.store ?? { runFolder, readAssessments, appendAssessment };
  const previous = latestJudgeFor(run.id, await store.readAssessments(run.batchId), choice, legacyChoice);
  if (missingOnly && previous !== undefined) return previous;
  const scratch = await (services.scratch ?? (() => mkdtemp(join(tmpdir(), "eval-rejudge-"))))();
  let play: Playthrough | null = null;
  try {
    return await rejudgeRun(run, versions, {
      choice, ...(previous === undefined ? {} : { previous }),
      append: async (assessment) => {
        // Each judgment keeps what its judge saw, so the record stands on its own.
        if (play !== null) {
          const evidence = join(assessmentEvidenceFolder(run.batchId, assessment.id), PlaythroughFiles.folder);
          await mkdir(evidence, { recursive: true });
          await cp(play.folder, evidence, { recursive: true });
        }
        await store.appendAssessment(assessment);
      },
      evaluate: async (_run, onReport) => {
        const project = join(scratch, "project");
        await prepareJudgeFolder(join(store.runFolder(run.batchId, run.id), ResultFiles.files), project, run.build?.html);
        play = await playthroughOf(suite, run, store.runFolder(run.batchId, run.id), services);
        if (play !== null) {
          await mkdir(join(project, PlaythroughFiles.folder));
          await cp(play.trace, join(project, PlaythroughFiles.folder, PlaythroughFiles.trace));
        }
        return (services.judge ?? judgeRun)(suite, choice, project, join(scratch, "home"), JudgeTimeoutMs,
          { ...services.options, images: play?.images ?? judgeImagePaths(run), onReport: (report) => { onReport(report); services.options?.onReport?.(report); } });
      },
    });
  } finally { await rm(scratch, { recursive: true, force: true }); }
}

export async function completeSavedJudgments(suite: Suite, run: RunView, config: EvalConfig, versions: AssessmentVersions, services: SavedJudgeServices = {}): Promise<void> {
  if (run.startedAt === null || run.finishedAt === null || run.failureKind === FailureKind.infrastructure) return;
  const options = services.options ?? { stage: { ...(config.containers === undefined ? {} : { limits: config.containers }) } };
  for (const choice of configuredJudges(config)) await judgeSavedRun(suite, run, choice, versions, { ...services, options }, true, config.judge);
}

export async function rejudgeRuns(suite: Suite, runs: readonly RunView[], config: EvalConfig, versions: AssessmentVersions, services: SavedJudgeServices = {}, onlyRun?: string, missingOnly = false, onlySetups?: readonly string[]): Promise<void> {
  // An attempt that stopped on an infrastructure failure kept no files to judge, as in completeSavedJudgments.
  const selected = runs.filter((run) => run.startedAt !== null && run.finishedAt !== null && run.failureKind !== FailureKind.infrastructure && (onlyRun === undefined || run.id === onlyRun) && (onlySetups === undefined || onlySetups.includes(run.setup)) && judgeable(suite, run));
  const options = services.options ?? { stage: { ...(config.containers === undefined ? {} : { limits: config.containers }) } };
  await runInParallel(selected, config.parallelRuns || 1, async (run) => {
    for (const choice of configuredJudges(config)) {
      // Asked for missing judgments, a judgment that ended in an error counts as missing too.
      const previous = missingOnly ? latestJudgeFor(run.id, await (services.store ?? { readAssessments }).readAssessments(run.batchId), choice, config.judge) : undefined;
      if (previous === undefined || previous.error !== null) await judgeSavedRun(suite, run, choice, versions, { ...services, options }, false, config.judge);
    }
  }, () => false);
}

async function main(): Promise<void> {
  const { args, options } = splitMachineOptions(process.argv.slice(2));
  applyMachineOptions(options);
  const { batchId, onlyRun, onlySetups, overrides, missingOnly } = judgeOptions(args);
  const batch = await readBatch(batchId), suite = await loadSuite(batch.suite);
  const config = { ...batch.config, ...overrides };
  requireSignIns(configuredJudges(config).map((choice) => choice.tool));
  await rejudgeRuns(suite, await readRuns(batch), config, await gradingVersions(suite), {}, onlyRun, missingOnly, onlySetups);
}

if (import.meta.main) await main();
