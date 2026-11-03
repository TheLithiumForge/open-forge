import { existsSync } from "node:fs";
import { readFile, realpath, writeFile } from "node:fs/promises";
import { join } from "node:path";
import type { EvalConfig, ModelChoice } from "../config/config.types.ts";
import { modelKey } from "../config/judges.ts";
import type { ChainSkip } from "../history/records.types.ts";
import type { RunRecord } from "../history/results.types.ts";
import { plannedRun } from "../history/run-records.ts";
import { RunStatus } from "../history/statuses.ts";
import { chainedFiles, SeedConflict, writeChainedStart, type ChainedStart } from "../materialize/chain.ts";
import { GitDirectory, inventory, type FileHash } from "../materialize/files.ts";
import type { ScopePolicy } from "../materialize/scope.types.ts";
import { ResultFiles } from "../paths.ts";

/** Why a pair of a chain has no child run. Skipped pairs stay out of every mean, since the child never started. */
export const ChainSkipReason = { noParent: "no-parent", parentUnfinished: "parent-unfinished", parentBuild: "parent-build-failed", seedConflict: "seed-conflict" } as const;
export type ChainSkipReason = (typeof ChainSkipReason)[keyof typeof ChainSkipReason];

/** The start a chained run graded against, saved in its run folder so a resume or a recheck uses the same baseline. */
export const ChainStartFile = "start.json";
export const ChainStartVersion = 1;
export interface ChainStartManifest { readonly schemaVersion: typeof ChainStartVersion; readonly parentBatchId: string; readonly parentRunId: string; readonly files: readonly FileHash[] }

/** The parent a child starts from: the same agent, setup and repeat, and the run that replaced it when there is one. */
export function parentRun(parents: readonly RunRecord[], agent: ModelChoice, setup: string, repeat: number): RunRecord | undefined {
  const replaced = new Set(parents.flatMap((run) => run.replacesRunId === null ? [] : [run.replacesRunId]));
  return parents.find((run) => run.setup === setup && run.repeat === repeat && modelKey(run.agent) === modelKey(agent) && !replaced.has(run.id));
}

/** Why a parent can't start a child, or null when it can. A child needs a parent whose agent finished and whose build passed. */
export function parentProblem(parent: RunRecord | undefined): ChainSkipReason | null {
  if (parent === undefined) return ChainSkipReason.noParent;
  if (parent.status !== RunStatus.done) return ChainSkipReason.parentUnfinished;
  return parent.build?.passed === true ? null : ChainSkipReason.parentBuild;
}

export function parentFiles(runFolder: (batchId: string, runId: string) => string, parentBatchId: string, parentRunId: string): string {
  return join(runFolder(parentBatchId, parentRunId), ResultFiles.files);
}

// Runners reach the run folders through a junction, and the inventory refuses links, so parent folders resolve to their real path first.
/** Pairs every planned child with its parent. A pair whose parent can't start a child, or whose defect won't apply, is skipped. */
export async function planChainRuns(config: EvalConfig, batchId: string, parents: readonly RunRecord[], files: (parent: RunRecord) => string, seedPatch: string,
  id: () => string): Promise<{ readonly runs: RunRecord[]; readonly skipped: ChainSkip[] }> {
  const runs: RunRecord[] = [], skipped: ChainSkip[] = [];
  for (const agent of config.agents) for (const setup of config.setups) for (let repeat = 1; repeat <= config.runsPerSetup; repeat += 1) {
    const parent = parentRun(parents, agent, setup, repeat);
    let reason = parentProblem(parent);
    if (reason === null && parent !== undefined) {
      try { chainedFiles(await inventory(await realpath(files(parent)), new Set([GitDirectory, "node_modules"])), seedPatch); }
      catch (error) { if (!(error instanceof SeedConflict)) throw error; reason = ChainSkipReason.seedConflict; }
    }
    if (reason === null && parent !== undefined) runs.push({ ...plannedRun(id(), batchId, setup, agent, repeat), parentRunId: parent.id });
    else skipped.push({ setup, repeat, agent, parentRunId: parent?.id ?? null, reason: reason ?? ChainSkipReason.noParent });
  }
  return { runs, skipped };
}

/** A chained run's start. A run graded from its saved files reads its saved manifest. Every other run builds its start again and saves its manifest. */
export async function chainedRunStart(input: { readonly folder: string; readonly parentBatchId: string; readonly parentRunId: string; readonly parentFiles: string;
  readonly scope: ScopePolicy | undefined; readonly seedPatch: string; readonly project: string; readonly saved: boolean }): Promise<{ readonly start: ChainedStart | null; readonly files: readonly FileHash[] }> {
  const manifestPath = join(input.folder, ChainStartFile);
  if (input.saved && existsSync(manifestPath)) return { start: null, files: (JSON.parse(await readFile(manifestPath, "utf8")) as ChainStartManifest).files };
  const start = await writeChainedStart(await realpath(input.parentFiles), input.scope?.ignores ?? [], input.seedPatch, input.project);
  const manifest: ChainStartManifest = { schemaVersion: ChainStartVersion, parentBatchId: input.parentBatchId, parentRunId: input.parentRunId, files: start.files };
  await writeFile(manifestPath, `${JSON.stringify(manifest, null, 2)}\n`);
  return { start, files: start.files };
}

/** A chained run's saved baseline, or undefined for a run that didn't start a chain. */
export async function savedChainBaseline(folder: string): Promise<readonly FileHash[] | undefined> {
  const path = join(folder, ChainStartFile);
  return existsSync(path) ? (JSON.parse(await readFile(path, "utf8")) as ChainStartManifest).files : undefined;
}
