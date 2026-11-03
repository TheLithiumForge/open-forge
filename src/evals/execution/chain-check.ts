import { readFile } from "node:fs/promises";
import { dirname, join } from "node:path";
import type { EvalConfig } from "../config/config.types.ts";
import { Isolation } from "../config/choices.ts";
import type { Suite } from "../benchmarks/suite.types.ts";
import { checksForSuite } from "../benchmarks/registry.ts";
import { buildProject } from "../grading/build.ts";
import { readBatch, readRuns, readSetups, runFolder } from "../history/store.ts";
import type { RunRecord } from "../history/results.types.ts";
import { writeChainedStart } from "../materialize/chain.ts";
import { opaqueId, createRunSpace, removeFolder } from "./workspace.ts";
import { parentFiles, planChainRuns } from "./chain-runs.ts";

/** One child start graded as it is, beside its parent's own grading. */
export interface ChainCheckRow {
  readonly setup: string;
  readonly repeat: number;
  readonly parentRunId: string;
  readonly buildPassed: boolean;
  /** The new task's own checks, which a correct start fails where its defect lands. */
  readonly target: readonly { readonly id: string; readonly status: string }[];
  /** Every other check whose result differs from the parent's grading. */
  readonly changed: readonly { readonly id: string; readonly parent: string | null; readonly child: string }[];
}

/** The checks whose result differs between the parent's grading and the child start's, leaving out the new task's own checks. */
export function changedChecks(parent: readonly { readonly id: string; readonly status: string }[], child: readonly { readonly id: string; readonly status: string; readonly area: string }[]): ChainCheckRow["changed"] {
  const before = new Map(parent.map((check) => [check.id, check.status]));
  return child.filter((check) => check.area === "regression" || check.id.startsWith("brownfield-")).flatMap((check) => {
    const previous = before.get(check.id) ?? null;
    return previous === check.status ? [] : [{ id: check.id, parent: previous, child: check.status }];
  });
}

/**
 * The no-model check before a chain milestone runs. It builds each child start from its parent run, builds it offline and
 * grades it as it is. A correct start keeps the parent's results apart from the checks the new defect touches.
 */
export async function checkChain(config: EvalConfig, suite: Suite, parentBatchId: string): Promise<ChainCheckRow[]> {
  const item = suite.brownfield?.case;
  if (item === undefined || item.seedPatch === null) throw new Error("A chain check needs a Layer 4 case with a planted defect.");
  const parent = await readBatch(parentBatchId), parents = await readRuns(parent), setups = await readSetups(parent);
  const seedPatch = await readFile(join(item.root, item.seedPatch), "utf8");
  const files = (run: RunRecord) => parentFiles(runFolder, parentBatchId, run.id);
  // One child per setup keeps the check to a few full gradings at once.
  const planned = await planChainRuns({ ...config, runsPerSetup: 1 }, "chain-check", parents, files, seedPatch, opaqueId);
  return Promise.all(planned.runs.map(async (child): Promise<ChainCheckRow> => {
    const parentRunId = child.parentRunId ?? "", source = parents.find((run) => run.id === parentRunId);
    const scope = setups.find((setup) => setup.id === child.setup)?.scope;
    const start = await writeChainedStart(parentFiles(runFolder, parentBatchId, parentRunId), scope?.ignores ?? [], seedPatch, suite.project);
    const space = await createRunSpace(suite.project, start.folder);
    try {
      const build = await buildProject(Isolation.docker, space, config.limits.checkMinutes * 60_000);
      const checks = checksForSuite(suite, start.files, scope);
      if (checks === undefined) throw new Error("The case has no hidden checks.");
      const graded = await checks({ project: space.work, build, html: build.html === null ? null : join(space.work, build.html), shotsFolder: join(space.root, "shots") });
      return { setup: child.setup, repeat: child.repeat, parentRunId, buildPassed: build.passed,
        target: graded.checks.filter((check) => check.area !== "regression" && !check.id.startsWith("brownfield-")).map(({ id, status }) => ({ id, status })),
        changed: changedChecks(source?.checkOutcomes ?? [], graded.checks) };
    } finally {
      await removeFolder(space.root);
      await removeFolder(dirname(start.folder));
    }
  }));
}
