import { copyFile, mkdir, writeFile } from "node:fs/promises";
import { join } from "node:path";
import { TurnOutcome } from "./providers/agent-session.ts";
import type { RunSpace } from "./containers/executor.ts";
import { buildProject } from "../grading/build.ts";
import type { EvalConfig } from "../config/config.types.ts";
import { Isolation, Judging, Tool } from "../config/choices.ts";
import { withAgentStage } from "./containers/agent.ts";
import { executionLabel, ExecutionLabelFile, LocalExecutionLabel } from "./containers/eligibility.ts";
import type { Docker } from "./containers/docker.ts";
import { judgeRun } from "../grading/judge/judge.ts";
import { JudgeLimited } from "../grading/judge/reply.ts";
import type { JudgeReport } from "../grading/judge/report.ts";
import { completeSavedJudgments } from "../grading/rejudge.ts";
import { configuredJudges } from "../config/judges.ts";
import { prepareJudgeFolder } from "../grading/judge/judge-folder.ts";
import { judgeImagePaths } from "../grading/judge/shot-paths.ts";
import { ResultFiles } from "../paths.ts";
import type { RunRecord } from "../history/results.types.ts";
import { FailureKind, RunStatus } from "../history/statuses.ts";
import { ReferenceImplementer } from "../history/reference.ts";
import { appendAnalysis, appendAssessment, readAnalyses, readAssessments, readBatch, readSetups, runFolder, saveRun } from "../history/store.ts";
import { analyzeFinishedRun } from "../history/analysis/saved-run.ts";
import { checksAssessment, failedChecksAssessment, judgeAssessment } from "../history/assessments.ts";
import { countChecks, legacyCheckView } from "../grading/counting.ts";
import type { Suite } from "../benchmarks/suite.types.ts";
import type { SuiteChecks } from "../benchmarks/suite-checks.types.ts";
import type { HistoryStore } from "../history/store.ts";
import { ConversationRole, emptyConversation, SimulatorFailure } from "./conversation/report.ts";
import { ConversationFile } from "./conversation/answer-bank.ts";
import { layerChecks } from "../grading/document-layers.ts";
import { converse } from "./conversation/conversation.ts";
import { withCheckSlot } from "./check-slot.ts";
import { progress } from "./progress.ts";
import { stopProcessesUnder } from "./leftovers.ts";
import { createRunSpace, freezeFiles, removeFolder, scratchFolder } from "./workspace.ts";
import { OrchestratorLine } from "../materialize/delivery.ts";

export interface RunInput {
  readonly run: RunRecord;
  readonly suite: Suite;
  readonly config: EvalConfig;
  readonly snapshot: string;
  readonly position: string;
  readonly signal?: AbortSignal;
  readonly docker?: Docker;
  readonly services?: RunServices;
  /** Grades the files an earlier attempt already saved, without starting the agent again. */
  readonly savedFiles?: boolean;
}

export interface RunServices {
  readonly store?: Pick<HistoryStore, "runFolder" | "readBatch" | "saveRun" | "appendAssessment" | "readAssessments" | "readSetups" | "appendAnalysis" | "readAnalyses">;
  readonly createSpace?: typeof createRunSpace;
  readonly scratch?: typeof scratchFolder;
  readonly conversation?: typeof converse;
  readonly build?: typeof buildProject;
  readonly checks?: SuiteChecks;
  readonly judge?: typeof judgeRun;
  readonly stop?: typeof stopProcessesUnder;
}

const JudgeTimeoutMs = 15 * 60_000;

/** One run, start to finish: the agent and its questions, the offline build, the hidden checks and the judge. */
export async function executeRun(input: RunInput): Promise<RunRecord> {
  const { suite, config } = input;
  const services = input.services ?? {};
  const store = services.store ?? { runFolder, readBatch, saveRun, appendAssessment, readAssessments, readSetups, appendAnalysis, readAnalyses };
  const primaryJudge = configuredJudges(config)[0] ?? config.judge;
  const judgeNow = config.judging !== Judging.later;
  const folder = store.runFolder(input.run.batchId, input.run.id);
  const batch = await store.readBatch(input.run.batchId);
  const reference = input.run.implementer === ReferenceImplementer;
  const saved = input.savedFiles === true;
  const versions = { checker: batch.fingerprints.checks, rubric: batch.fingerprints.rubric, judge: batch.versions["judge"] ?? null,
    ...(batch.reference === undefined ? {} : { spec: batch.reference.specHash, specVersion: batch.reference.specVersion }) };
  await mkdir(folder, { recursive: true });
  // Saved files keep the conversation and events of the attempt that made them.
  if (!saved) {
    await writeFile(join(folder, ResultFiles.events), "", { flag: "a" });
    await writeFile(join(folder, ConversationFile), `${JSON.stringify(emptyConversation(config), null, 2)}\n`);
    await writeFile(join(folder, ExecutionLabelFile), `${JSON.stringify(executionLabel(config.isolation), null, 2)}\n`);
  }
  if (config.isolation === Isolation.local) progress.report(input.run.batchId, LocalExecutionLabel.reason);
  const firstStatus = reference || saved ? RunStatus.building : RunStatus.agent;
  let run: RunRecord = { ...input.run, status: firstStatus, pipelineStatus: firstStatus, startedAt: saved ? input.run.startedAt : new Date().toISOString(), error: null };
  const update = async (changes: Partial<RunRecord>, note: string | null = null): Promise<void> => {
    run = { ...run, ...changes, pipelineStatus: changes.status ?? run.status, reportedModel: changes.telemetry?.model ?? run.reportedModel };
    await store.saveRun(run);
    if (note !== null) progress.report(run.batchId, `Run ${input.position}: ${note}`);
  };
  await update({}, reference ? "grading the reference. Building offline." : saved ? "grading the saved files. Building offline." : "the agent is working.");
  let space: RunSpace | null = null;
  let judgeReport: JudgeReport | undefined;
  try {
    if (config.isolation !== Isolation.docker && run.agent.tool !== Tool.mock) throw new Error("Real agents require container isolation.");
    space = await (services.createSpace ?? createRunSpace)(suite.project, saved ? join(folder, ResultFiles.files) : input.snapshot);
    // The mock agent only installs our own reference, and the agent container runs nothing but a real provider's CLI.
    const agentIsolation = run.agent.tool === Tool.mock ? Isolation.local : Isolation.docker;
    const context = { isolation: agentIsolation, space, choice: run.agent, eventsPath: join(folder, ResultFiles.events) };
    // A team's orchestrator gets the registered line that tells it to delegate each task, after the request.
    const teamSuite = run.agent.team === undefined ? suite : { ...suite, prompt: `${suite.prompt}\n\n${OrchestratorLine}` };
    const stageOptions = { timeoutMs: config.limits.runMinutes * 60_000,
      ...(config.containers === undefined ? {} : { limits: config.containers }),
      ...(input.signal === undefined ? {} : { signal: input.signal }), ...(input.docker === undefined ? {} : { docker: input.docker }) };
    if (reference) await freezeFiles(space, join(folder, ResultFiles.files));
    else if (!saved) {
    const talk = run.agent.tool !== Tool.mock
      ? await withAgentStage(space, run.agent.tool, stageOptions, () => (services.conversation ?? converse)(context, teamSuite, config, { options: stageOptions }))
      : await (services.conversation ?? converse)(context, teamSuite, config, { options: stageOptions });
    await (services.stop ?? stopProcessesUnder)(space.root);
    if (talk.outcome === TurnOutcome.limited) {
      await freezeFiles(space, join(folder, ResultFiles.files));
      await update({ status: RunStatus.limited, telemetry: talk.telemetry, agentSeconds: talk.seconds, finalMessage: talk.finalMessage, questions: talk.questions, error: talk.error, failureKind: FailureKind.infrastructure, finishedAt: new Date().toISOString() }, "the provider hit a usage limit or rejected the sign-in. The batch pauses.");
      return run;
    }
    await update({ status: RunStatus.building, finalMessage: talk.finalMessage, questions: talk.questions, telemetry: talk.telemetry, agentSeconds: talk.seconds, error: talk.error }, `the agent finished after ${Math.round(talk.seconds)}s. Building offline.`);
    }
    const checkMs = config.limits.checkMinutes * 60_000;
    const build = await (services.build ?? buildProject)(Isolation.docker, space, checkMs, stageOptions);
    await (services.stop ?? stopProcessesUnder)(space.root);
    const html = build.html === null ? null : join(space.work, build.html);
    if (html !== null) await copyFile(html, join(folder, ResultFiles.game));
    // The files freeze before the status says checking, so a chained child planned meanwhile reads a parent's final files.
    if (!reference && !saved) await freezeFiles(space, join(folder, ResultFiles.files));
    await update({ status: RunStatus.checking, build }, `the build ${build.passed ? "passed" : "failed"}. Running the hidden checks.`);
    const project = space.work;
    // The machine-wide check slots alone limit how many checks run at once, in this process or any other.
    const checks = await withCheckSlot(async () => (services.checks ?? layerChecks(suite.id))?.({ project, build, html, shotsFolder: join(folder, ResultFiles.shots), stage: { ...stageOptions, timeoutMs: checkMs } }));
    await update({ status: judgeNow ? RunStatus.judging : RunStatus.checking, checks: checks?.checks.map(legacyCheckView) ?? [], performance: checks?.performance ?? null, shots: checks?.shots ?? [] },
      judgeNow ? "checks done. The judge is reading the code." : "checks done.");
    const assessment = checksAssessment(run, checks?.checks ?? [], versions, checks?.selection);
    await store.appendAssessment(assessment);
    await update({ functionalResult: countChecks(assessment.outcomes, assessment.selection), checkOutcomes: assessment.outcomes, checkSelection: assessment.selection, checksAssessmentId: assessment.id });
    // The objective measures come from the saved logs. A failure here never fails the run, and analyze --missing retries it.
    if (!reference) await analyzeFinishedRun(store, run).catch((error: unknown) => progress.report(run.batchId, `Run ${input.position}: the analysis failed, so analyze --missing adds it later. ${error instanceof Error ? error.message : String(error)}`));
    if (!judgeNow) {
      await update({ status: RunStatus.done, finishedAt: new Date().toISOString() }, "done. The judges read it later with judge --missing.");
      return run;
    }
    const judgeFolder = await (services.scratch ?? scratchFolder)();
    try {
      await prepareJudgeFolder(join(folder, ResultFiles.files), join(judgeFolder, "project"), run.build?.html);
      const judge = await (services.judge ?? judgeRun)(suite, primaryJudge, join(judgeFolder, "project"), join(judgeFolder, "home"), JudgeTimeoutMs,
        { stage: stageOptions, images: judgeImagePaths(run), onReport: (report) => { judgeReport = report; } });
      const judged = { ...judgeAssessment(run, judge, versions, judgeReport), requestedModel: primaryJudge, ballots: primaryJudge.repeats };
      await store.appendAssessment(judged);
      await update({ status: RunStatus.done, judge, judgeAssessmentId: judged.id, finishedAt: new Date().toISOString() }, "done.");
    } finally {
      await removeFolder(judgeFolder);
    }
  } catch (error) {
    if (error instanceof JudgeLimited && run.status === RunStatus.judging) {
      // The attempt and its checks are complete. judge --missing adds the judgment later without running the agent again.
      await update({ status: RunStatus.done, finishedAt: new Date().toISOString() }, "the judge's provider hit a usage limit or rejected the sign-in. The judgment stays missing for judge --missing.");
      throw error;
    }
    if (error instanceof SimulatorFailure) {
      await writeFile(join(folder, ConversationFile), `${JSON.stringify(error.report, null, 2)}\n`);
      await update({ questions: error.report.questions, finalMessage: error.report.messages?.findLast((message) => message.role === ConversationRole.agent)?.text ?? run.finalMessage });
    }
    // Both judges can still read the source when a verifier build fails.
    if (!reference && !saved && run.status === RunStatus.building && space !== null) await freezeFiles(space, join(folder, ResultFiles.files)).catch(() => undefined);
    const failureKind = run.status === RunStatus.agent ? FailureKind.infrastructure
      : saved || run.status === RunStatus.judging || run.status === RunStatus.checking ? FailureKind.grading : FailureKind.attempt;
    if (run.status === RunStatus.checking) {
      const failed = failedChecksAssessment(run, error, versions);
      await store.appendAssessment(failed);
      run = { ...run, checksAssessmentId: failed.id, checkOutcomes: failed.outcomes, functionalResult: countChecks(failed.outcomes, failed.selection) };
    }
    if (run.status === RunStatus.judging) {
      const judged = { ...judgeAssessment(run, [], versions, judgeReport), requestedModel: primaryJudge, ballots: primaryJudge.repeats, error: error instanceof Error ? error.message : String(error) };
      await store.appendAssessment(judged);
      run = { ...run, judgeAssessmentId: judged.id };
    }
    await update({ status: RunStatus.failed, failureKind, error: error instanceof Error ? error.message : String(error), finishedAt: new Date().toISOString() }, "failed.");
  } finally {
    if (space !== null) {
      await (services.stop ?? stopProcessesUnder)(space.root);
      await removeFolder(space.root);
    }
  }
  if (!judgeNow) return run;
  await completeSavedJudgments(suite, run, config, versions, { store, ...(services.judge === undefined ? {} : { judge: services.judge }),
    ...(services.scratch === undefined ? {} : { scratch: services.scratch }), options: { stage: {
      ...(config.containers === undefined ? {} : { limits: config.containers }), ...(input.signal === undefined ? {} : { signal: input.signal }), ...(input.docker === undefined ? {} : { docker: input.docker }),
    } } });
  return run;
}
