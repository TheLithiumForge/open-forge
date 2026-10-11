import assert from "node:assert/strict";
import { test } from "node:test";
import { readBuildRun, readBuildRuns, readBuildArtifacts, newestBuilds } from "../build-runs.ts";
import { BuildId, BuildTime, PackageArtifacts, recordedBuild } from "../../__tests__/build-fixtures.ts";

test("Build API readers preserve recorded identities and artifact expiration", () => {
  const run = recordedBuild();
  assert.deepEqual(readBuildRun(run), run);
  assert.deepEqual(readBuildRuns({ workflow_runs: [run] }), [run]);
  assert.deepEqual(readBuildArtifacts({ artifacts: PackageArtifacts }), PackageArtifacts);
  assert.deepEqual(readBuildRuns({ workflow_runs: [] }), []);
  assert.throws(() => readBuildRun({ ...run, id: "invalid" }));
  assert.throws(() => readBuildRun({ ...run, created_at: "invalid" }));
  assert.throws(() => readBuildRuns({}));
  assert.throws(() => readBuildArtifacts({ artifacts: [{ name: "incomplete" }] }));
});

test("Build ordering uses creation time and run ID without mutating responses", () => {
  const older = recordedBuild({ id: BuildId.newer });
  const newer = recordedBuild({ created_at: BuildTime.newer });
  const runs = [older, newer];
  assert.deepEqual(newestBuilds(runs), [newer, older]);
  assert.deepEqual(runs, [older, newer]);
  assert.deepEqual(
    newestBuilds([recordedBuild(), older]).map((run) => run.id),
    [BuildId.newer, BuildId.older],
  );
});
