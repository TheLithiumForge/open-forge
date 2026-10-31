import { copyFile, mkdir } from "node:fs/promises";
import { join } from "node:path";
import { TurnOutcome } from "../agents/agent-session.ts";
import type { RunSpace } from "../agents/executor.ts";
import { buildProject } from "../checks/build.ts";
import type { EvalConfig } from "../config/config.types.ts";
import { judgeRun } from "../judge/judge.ts";
import { prepareJudgeFolder } from "../judge/judge-folder.ts";
import { ResultFiles } from "../paths.ts";
import type { RunRecord } from "../results/results.types.ts";
import { RunStatus } from "../results/statuses.ts";
import { runFolder, saveRun } from "../results/store.ts";
import type { Suite } from "../suite/suite.types.ts";
import { suiteChecks } from "../../suites/registry.ts";
import { converse } from "./conversation.ts";
import { oneCheckAtATime } from "./run-queue.ts";
import { progress } from "./progress.ts";
import { stopProcessesUnder } from "../agents/leftovers.ts";
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
  await mkdir(folder, { recursive: true });
  let run: RunRecord = { ...input.run, status: RunStatus.agent, startedAt: new Date().toISOString(), error: null };
  const update = async (changes: Partial<RunRecord>, note: string | null = null): Promise<void> => {
    run = { ...run, ...changes };
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
      await update({ status: RunStatus.limited, error: talk.error }, "the tool hit a usage limit. The batch pauses.");
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
    const checks = await oneCheckAtATime(async () => suiteChecks[suite.id]?.({ project, build, html, shotsFolder: join(folder, ResultFiles.shots) }));
    await freezeFiles(space, join(folder, ResultFiles.files));
    await update({ status: RunStatus.judging, checks: checks?.checks ?? [], performance: checks?.performance ?? null, shots: checks?.shots ?? [] }, "checks done. The judge is reading the code.");
    const judgeFolder = await scratchFolder();
    try {
      await prepareJudgeFolder(join(folder, ResultFiles.files), join(judgeFolder, "project"));
      const judge = await judgeRun(suite, config.judge, join(judgeFolder, "project"), join(judgeFolder, "home"), JudgeTimeoutMs);
      await update({ status: RunStatus.done, judge, finishedAt: new Date().toISOString() }, "done.");
    } finally {
      await removeFolder(judgeFolder);
    }
  } catch (error) {
    await update({ status: RunStatus.failed, error: error instanceof Error ? error.message : String(error), finishedAt: new Date().toISOString() }, "failed.");
  } finally {
    if (space !== null) {
      await stopProcessesUnder(space.root);
      await removeFolder(space.root);
    }
  }
  return run;
}
