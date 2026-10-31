// Runs the current hidden checks again on a batch's finished runs, from the games and files they saved.
// Use it after the checks change. With --rebuild, or for a run whose build failed, it first installs the
// packages from the saved lockfile and builds offline again. The agents and the judge don't run again.
import { existsSync } from "node:fs";
import { copyFile } from "node:fs/promises";
import { join } from "node:path";
import { NpmCommand, spawnInRun } from "../agents/executor.ts";
import { waitForProcess } from "../agents/process.ts";
import { buildProject } from "../checks/build.ts";
import { Isolation } from "../config/choices.ts";
import { ResultFiles } from "../paths.ts";
import type { BuildResult, RunRecord } from "../results/results.types.ts";
import { RunStatus } from "../results/statuses.ts";
import { readBatch, readRun, readRuns, runFolder, saveBatch, saveRun } from "../results/store.ts";
import { loadSuite } from "../suite/suite.ts";
import { suiteChecks } from "../../suites/registry.ts";
import { createRunSpace, removeFolder } from "./workspace.ts";

const RebuildFlag = "--rebuild";
const InstallMs = 10 * 60_000;
const BuildMs = 15 * 60_000;

const args = process.argv.slice(2);
const rebuildAll = args.includes(RebuildFlag);
const [batchId, onlyRun] = args.filter((arg) => arg !== RebuildFlag);
if (batchId === undefined) throw new Error("Name the batch to check again: npm run recheck -- <batch ID> [run ID] [--rebuild]");
const batch = await readBatch(batchId);
const suite = await loadSuite(batch.suite);
const checks = suiteChecks[suite.id];
if (checks === undefined) throw new Error(`No hidden checks for ${suite.id}.`);

/** Installs the saved project's packages, then builds it offline as a run does. */
async function rebuild(run: RunRecord, folder: string): Promise<{ build: BuildResult; project: string; cleanup: () => Promise<void> }> {
  const space = await createRunSpace(suite.project, join(folder, ResultFiles.files));
  await waitForProcess(spawnInRun({ isolation: Isolation.local, space, command: NpmCommand, args: ["ci", "--no-audit", "--no-fund"], network: true }), { timeoutMs: InstallMs });
  const build = await buildProject(Isolation.local, space, BuildMs);
  if (build.html !== null) await copyFile(join(space.work, build.html), join(folder, ResultFiles.game));
  console.log(`${run.id}: rebuilt, ${build.passed ? "passed" : "failed"}.`);
  return { build, project: space.work, cleanup: () => removeFolder(space.root) };
}

for (const run of await readRuns(batch)) {
  if (run.status !== RunStatus.done || run.build === null || (onlyRun !== undefined && run.id !== onlyRun)) continue;
  const folder = runFolder(batch.id, run.id);
  const fresh = rebuildAll || !run.build.passed ? await rebuild(run, folder) : null;
  const build = fresh?.build ?? run.build;
  const game = join(folder, ResultFiles.game);
  const result = await checks({ project: fresh?.project ?? join(folder, ResultFiles.files), build, html: existsSync(game) && build.passed ? game : null, shotsFolder: join(folder, ResultFiles.shots) });
  // Read again before saving, so judge answers written meanwhile by a rejudge are kept.
  await saveRun({ ...(await readRun(batch.id, run.id)), build, checks: result.checks, performance: result.performance, shots: result.shots });
  await fresh?.cleanup();
  const passed = result.checks.filter((check) => check.passed === true).length;
  console.log(`${run.id}: ${passed} of ${result.checks.length} checks passed.`);
}
await saveBatch({ ...(await readBatch(batch.id)), fingerprint: suite.fingerprint });
console.log(`Batch ${batch.id} now carries eval ${suite.fingerprint}.`);
