import assert from "node:assert/strict";
import { mkdir, mkdtemp, rm, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { dirname, join } from "node:path";
import { test } from "node:test";
import { FixtureConfig } from "../config/test-fixtures.ts";
import { MatrixAgents } from "../config/matrix.ts";
import { plannedRun, replacementRun } from "../history/run-records.ts";
import type { RunRecord } from "../history/results.types.ts";
import { RunStatus } from "../history/statuses.ts";
import { ChainSkipReason, chainedRunStart, ChainStartFile, parentRun, planChainRuns, savedChainBaseline } from "./chain-runs.ts";

const [Agent] = MatrixAgents;
const Original = "export const moves = 0;\n";
const Seed = ["--- a/src/game.ts", "+++ b/src/game.ts", "@@ -1 +1 @@", `-${Original.trimEnd()}`, "+export const moves = 1;", ""].join("\n");
const Passed = { passed: true, typecheck: true, seconds: 1, packages: 1, html: "dist/index.html", bytes: 1, log: "" };

function parent(id: string, setup: string, repeat: number, changes: Partial<RunRecord> = {}): RunRecord {
  if (Agent === undefined) throw new Error("The matrix needs an agent.");
  return { ...plannedRun(id, "parent-batch", setup, Agent, repeat), status: RunStatus.done, build: Passed, ...changes } as RunRecord;
}

async function parentFolders(t: { after(fn: () => Promise<void>): void }, files: Readonly<Record<string, string>>) {
  const root = await mkdtemp(join(tmpdir(), "chain-runs-test-"));
  t.after(() => rm(root, { recursive: true, force: true }));
  for (const [run, text] of Object.entries(files)) {
    const file = join(root, run, "src", "game.ts");
    await mkdir(dirname(file), { recursive: true });
    await writeFile(file, text);
  }
  return root;
}

test("a child pairs with the parent of its setup and repeat, or with that parent's replacement", () => {
  if (Agent === undefined) throw new Error("The matrix needs an agent.");
  const first = parent("first", "plain-docs", 1), replaced = parent("replaced", "plain-docs", 2);
  const replacement = { ...parent("replacement", "plain-docs", 2), replacesRunId: replaced.id } as RunRecord;
  const runs = [first, replaced, replacement];
  assert.equal(parentRun(runs, Agent, "plain-docs", 1)?.id, first.id);
  assert.equal(parentRun(runs, Agent, "plain-docs", 2)?.id, replacement.id);
  assert.equal(parentRun(runs, Agent, "full-core", 1), undefined);
});

test("only parents that finished, built and still take the next defect start a child, and the rest are skipped with a reason", async (t) => {
  if (Agent === undefined) throw new Error("The matrix needs an agent.");
  const root = await parentFolders(t, { ready: Original, failed: Original, conflict: "export const moves = 9;\n" });
  const parents = [parent("ready", "plain-docs", 1), parent("failed", "plain-docs", 2, { build: { ...Passed, passed: false } }),
    parent("conflict", "plain-docs", 3), parent("stopped", "full-core", 1, { status: RunStatus.failed })];
  const config = { ...FixtureConfig, agents: [Agent], setups: ["plain-docs", "full-core"], runsPerSetup: 3 };
  let next = 0;
  const planned = await planChainRuns(config, "child-batch", parents, (run) => join(root, run.id), Seed, () => `child-${next += 1}`);
  assert.deepEqual(planned.runs.map((run) => [run.setup, run.repeat, run.parentRunId]), [["plain-docs", 1, "ready"]]);
  assert.deepEqual(planned.skipped.map((pair) => [pair.setup, pair.repeat, pair.reason]), [
    ["plain-docs", 2, ChainSkipReason.parentBuild], ["plain-docs", 3, ChainSkipReason.seedConflict],
    ["full-core", 1, ChainSkipReason.parentUnfinished], ["full-core", 2, ChainSkipReason.noParent], ["full-core", 3, ChainSkipReason.noParent],
  ]);
});

test("a chained run saves its start, a resume reads it back, and a replacement keeps its parent", async (t) => {
  const root = await parentFolders(t, { ready: Original });
  const folder = join(root, "child");
  await mkdir(folder);
  const built = await chainedRunStart({ folder, parentBatchId: "parent-batch", parentRunId: "ready", parentFiles: join(root, "ready"), scope: undefined, seedPatch: Seed, project: "neon-pairs", saved: false });
  t.after(() => built.start === null ? Promise.resolve() : rm(dirname(built.start.folder), { recursive: true, force: true }));
  assert.deepEqual(built.files.map((file) => file.path), ["src/game.ts"]);
  assert.deepEqual(await savedChainBaseline(folder), built.files);
  const resumed = await chainedRunStart({ folder, parentBatchId: "parent-batch", parentRunId: "ready", parentFiles: join(root, "missing"), scope: undefined, seedPatch: Seed, project: "neon-pairs", saved: true });
  assert.equal(resumed.start, null, "A resume grades saved files, so it builds no start.");
  assert.deepEqual(resumed.files, built.files);
  assert.equal(await savedChainBaseline(join(root, "ready")), undefined, `Only a chained run has a ${ChainStartFile}.`);
  const child = { ...parent("child", "plain-docs", 1), parentRunId: "ready" } as RunRecord;
  assert.equal(replacementRun(child, "again", "Fixture replacement.").parentRunId, "ready");
});
