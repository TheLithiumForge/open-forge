import { copyFile, mkdir } from "node:fs/promises";
import { join } from "node:path";
import { TurnOutcome } from "./providers/agent-session.ts";
import type { RunSpace } from "./containers/executor.ts";
import { buildProject } from "../grading/build.ts";
import type { EvalConfig } from "../config/config.types.ts";
import { judgeRun } from "../grading/judge/judge.ts";
import { prepareJudgeFolder } from "../grading/judge/judge-folder.ts";
import { ResultFiles } from "../paths.ts";
import type { RunRecord } from "../history/results.types.ts";
import { FailureKind, RunStatus } from "../history/statuses.ts";
import { appendAssessment, readBatch, runFolder, saveRun } from "../history/store.ts";
import { checksAssessment, failedChecksAssessment, judgeAssessment } from "../history/assessments.ts";
import { countChecks } from "../grading/counting.ts";
import type { Suite } from "../benchmarks/suite.types.ts";
import { suiteChecks } from "../benchmarks/registry.ts";
import { converse } from "./conversation/conversation.ts";
import { oneCheckAtATime } from "./run-queue.ts";
import { progress } from "./progress.ts";
import { stopProcessesUnder } from "./leftovers.ts";
import { createRunSpace, freezeFiles, removeFolder, scratchFolder } from "./workspace.ts";

export interface RunInput {
  readonly run: RunRecord;
  readonly suite: Suite;
  readonly config: EvalConfig;
  readonly snapshot: string;
  readonly position: string;
}

const JudgeTimeoutMs = 15 * 60_000;

/** One run, start to finish: the agent and its questions, the offline build, the hidden checks and the judge. */
export async function executeRun(input: RunInput): Promise<RunRecord> {
  const { suite, config } = input;
  const folder = runFolder(input.run.batchId, input.run.id);
  const batch = await readBatch(input.run.batchId);
  const versions = { checker: batch.fingerprints.checks, rubric: batch.fingerprints.rubric, judge: batch.versions["judge"] ?? null };
  await mkdir(folder, { recursive: true });
  let run: RunRecord = { ...input.run, status: RunStatus.agent, pipelineStatus: RunStatus.agent, startedAt: new Date().toISOString(), error: null };
  const update = async (changes: Partial<RunRecord>, note: string | null = null): Promise<void> => {
    run = { ...run, ...changes, pipelineStatus: changes.status ?? run.status, reportedModel: changes.telemetry?.model ?? run.reportedModel };
    await saveRun(run);
    if (note !== null) progress.report(run.batchId, `Run ${input.position}: ${note}`);
  };
  await update({}, "the agent is working.");
  let space: RunSpace | null = null;
  try {
    space = await createRunSpace(suite.project, input.snapshot);
    const context = { isolation: config.isolation, space, choice: run.agent, eventsPath: join(folder, ResultFiles.events) };
    const talk = await converse(context, suite, config);
    await stopProcessesUnder(space.root);
    if (talk.outcome === TurnOutcome.limited) {
      await freezeFiles(space, join(folder, ResultFiles.files));
      await update({ status: RunStatus.limited, telemetry: talk.telemetry, agentSeconds: talk.seconds, finalMessage: talk.finalMessage, questions: talk.questions, error: talk.error, failureKind: FailureKind.infrastructure, finishedAt: new Date().toISOString() }, "the tool hit a usage limit. The batch pauses.");
      return run;
    }
    await update({ status: RunStatus.building, finalMessage: talk.finalMessage, questions: talk.questions, telemetry: talk.telemetry, agentSeconds: talk.seconds, error: talk.error }, `the agent finished after ${Math.round(talk.seconds)}s. Building offline.`);
    const checkMs = config.limits.checkMinutes * 60_000;
    const build = await buildProject(config.isolation, space, checkMs);
    await stopProcessesUnder(space.root);
    const html = build.html === null ? null : join(space.work, build.html);
    if (html !== null) await copyFile(html, join(folder, ResultFiles.game));
    await update({ status: RunStatus.checking, build }, `the build ${build.passed ? "passed" : "failed"}. Running the hidden checks.`);
    const project = space.work;
    await freezeFiles(space, join(folder, ResultFiles.files));
    const checks = await oneCheckAtATime(async () => suiteChecks[suite.id]?.({ project, build, html, shotsFolder: join(folder, ResultFiles.shots) }));
    await update({ status: RunStatus.judging, checks: checks?.checks ?? [], performance: checks?.performance ?? null, shots: checks?.shots ?? [] }, "checks done. The judge is reading the code.");
    const assessment = checksAssessment(run, checks?.checks ?? [], versions);
    await appendAssessment(assessment);
    await update({ functionalResult: countChecks(assessment.outcomes, assessment.selection), checkOutcomes: assessment.outcomes, checkSelection: assessment.selection, checksAssessmentId: assessment.id });
    const judgeFolder = await scratchFolder();
    try {
      await prepareJudgeFolder(join(folder, ResultFiles.files), join(judgeFolder, "project"));
      const judge = await judgeRun(suite, config.judge, join(judgeFolder, "project"), join(judgeFolder, "home"), JudgeTimeoutMs);
      await appendAssessment({ ...judgeAssessment(run, judge, versions), requestedModel: config.judge });
      await update({ status: RunStatus.done, judge, finishedAt: new Date().toISOString() }, "done.");
    } finally {
      await removeFolder(judgeFolder);
    }
  } catch (error) {
    const failureKind = run.status === RunStatus.agent ? FailureKind.infrastructure : run.status === RunStatus.judging || run.status === RunStatus.checking ? FailureKind.grading : FailureKind.attempt;
    if (run.status === RunStatus.checking) await appendAssessment(failedChecksAssessment(run, error, versions));
    if (run.status === RunStatus.judging) await appendAssessment({ ...judgeAssessment(run, [], versions), requestedModel: config.judge, error: error instanceof Error ? error.message : String(error) });
    await update({ status: RunStatus.failed, failureKind, error: error instanceof Error ? error.message : String(error), finishedAt: new Date().toISOString() }, "failed.");
  } finally {
    if (space !== null) {
      await stopProcessesUnder(space.root);
      await removeFolder(space.root);
    }
  }
  return run;
}
