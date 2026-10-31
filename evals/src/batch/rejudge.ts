// Runs the judge again on a batch's finished runs, from the files they saved, several at once.
// Use it after the judge or its rubric changes. The agents and the hidden checks don't run again.
import { join } from "node:path";
import { judgeRun } from "../judge/judge.ts";
import { prepareJudgeFolder } from "../judge/judge-folder.ts";
import { ResultFiles } from "../paths.ts";
import { RunStatus } from "../results/statuses.ts";
import { readBatch, readRun, readRuns, runFolder, saveRun } from "../results/store.ts";
import { loadSuite } from "../suite/suite.ts";
import { runInParallel } from "./run-queue.ts";
import { removeFolder, scratchFolder } from "./workspace.ts";

const JudgeTimeoutMs = 15 * 60_000;

const [batchId, onlyRun] = process.argv.slice(2);
if (batchId === undefined) throw new Error("Name the batch to judge again: npm run rejudge -- <batch ID> [run ID]");
const batch = await readBatch(batchId);
const suite = await loadSuite(batch.suite);
const runs = (await readRuns(batch)).filter((run) => run.status === RunStatus.done && (onlyRun === undefined || run.id === onlyRun));
// Batches saved before parallel runs existed have no setting, so they judge one run at a time.
await runInParallel(runs, batch.config.parallelRuns || 1, async (run) => {
  const scratch = await scratchFolder();
  try {
    await prepareJudgeFolder(join(runFolder(batch.id, run.id), ResultFiles.files), join(scratch, "project"));
    const judge = await judgeRun(suite, batch.config.judge, join(scratch, "project"), join(scratch, "home"), JudgeTimeoutMs);
    // Read again before saving, so a recheck that ran meanwhile keeps its checks.
    await saveRun({ ...(await readRun(batch.id, run.id)), judge });
    const sure = judge.filter((answer) => answer.yes + answer.no > 0).length;
    console.log(`${run.id}: ${sure} of ${judge.length} questions answered.`);
  } finally {
    await removeFolder(scratch);
  }
}, () => false);
