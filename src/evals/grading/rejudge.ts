import { mkdtemp, rm } from "node:fs/promises";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { judgeRun } from "./judge/judge.ts";
import { prepareJudgeFolder } from "./judge/judge-folder.ts";
import { ResultFiles } from "../paths.ts";
import { judgeAssessment } from "../history/assessments.ts";
import { gradingVersions } from "../history/capture.ts";
import type { AssessmentVersions, JudgeAssessment, RunView } from "../history/records.types.ts";
import type { JudgeAnswer } from "../history/results.types.ts";
import { appendAssessment, readBatch, readRuns, runFolder } from "../history/store.ts";
import { loadSuite } from "../benchmarks/suite.ts";
import { runInParallel } from "../execution/run-queue.ts";

const JudgeTimeoutMs = 15 * 60_000;

export interface RejudgeDependencies {
  readonly evaluate: (run: RunView) => Promise<readonly JudgeAnswer[]>;
  readonly append: (assessment: JudgeAssessment) => Promise<void>;
}

export async function rejudgeRun(run: RunView, versions: AssessmentVersions, dependencies: RejudgeDependencies): Promise<JudgeAssessment> {
  let assessment: JudgeAssessment;
  try {
    assessment = judgeAssessment(run, await dependencies.evaluate(run), versions);
  } catch (error) {
    assessment = { ...judgeAssessment(run, [], versions), error: error instanceof Error ? error.message : String(error) };
  }
  await dependencies.append(assessment);
  return assessment;
}

async function main(): Promise<void> {
  const [batchId, onlyRun] = process.argv.slice(2);
  if (batchId === undefined) throw new Error("Name the batch to judge again.");
  const batch = await readBatch(batchId);
  const suite = await loadSuite(batch.suite);
  const versions = await gradingVersions(suite);
  const runs = (await readRuns(batch)).filter((run) => run.startedAt !== null && (onlyRun === undefined || run.id === onlyRun));
  await runInParallel(runs, batch.config.parallelRuns || 1, async (run) => {
    const scratch = await mkdtemp(join(tmpdir(), "eval-rejudge-"));
    try {
      await prepareJudgeFolder(join(runFolder(batch.id, run.id), ResultFiles.files), join(scratch, "project"));
      await rejudgeRun(run, versions, {
        append: (assessment) => appendAssessment({ ...assessment, requestedModel: batch.config.judge }),
        evaluate: () => judgeRun(suite, batch.config.judge, join(scratch, "project"), join(scratch, "home"), JudgeTimeoutMs),
      });
    } finally {
      await rm(scratch, { recursive: true, force: true });
    }
  }, () => false);
}

if (import.meta.main) await main();
