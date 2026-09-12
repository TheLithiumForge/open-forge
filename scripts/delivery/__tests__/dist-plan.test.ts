import assert from "node:assert/strict";
import { spawnSync } from "node:child_process";
import { fileURLToPath } from "node:url";
import { test } from "node:test";
import { distPlan } from "../dist-plan.ts";
import { readOptions } from "../options.ts";
import { runStage } from "../stage.ts";

const rid = "osx-arm64";

test("dist forwards build flags only to build and skip-tests only to packaging", () => {
  const stages = distPlan(rid, { sha: true, offline: true, "skip-tests": true });
  assert.deepEqual(
    stages.map((stage) => [stage.script, stage.args, stage.skipped]),
    [
      ["build:native", ["--rid", rid, "--sha", "--offline"], false],
      ["test:built", ["--rid", rid], true],
      ["pack", ["--rid", rid, "--skip-tests"], false],
    ],
  );
  assert.equal(distPlan(rid, {})[1]?.skipped, false);
  assert.deepEqual(distPlan(rid, { "no-restore": true })[0]?.args, ["--rid", rid, "--no-restore"]);
});

test("dist routes a validated target selection to packaging only", () => {
  const stages = distPlan(rid, { targets: "win-x64,osx-arm64" });
  assert.deepEqual(stages[2]?.args, ["--rid", rid, "--targets", "win-x64,osx-arm64"]);
  assert.deepEqual(stages[0]?.args, ["--rid", rid]);
  assert.deepEqual(stages[1]?.args, ["--rid", rid]);
  assert.throws(() => distPlan(rid, { targets: "linux-x64" }), /include this host/);
});

test("commands reject misplaced skip-tests and restore flags", () => {
  assert.equal(readOptions("pack", ["--skip-tests"])?.["skip-tests"], true);
  assert.throws(() => readOptions("build:native", ["--skip-tests"]), /does not accept/);
  assert.throws(() => readOptions("test:built", ["--no-restore"]), /does not accept/);
});

test("dist plan is inspectable without npm or dotnet execution", () => {
  const env = { ...process.env };
  delete env["npm_execpath"];
  const result = spawnSync(process.execPath, [fileURLToPath(new URL("../dist.ts", import.meta.url)), "--plan", "--skip-tests", "--no-restore"], { env, encoding: "utf8" });
  assert.equal(result.status, 0, result.stderr);
  assert.match(result.stdout, /test:built.*--rid/);
  assert.match(result.stdout, /\[SKIPPED\]/);
  assert.match(result.stdout, /pack -- --rid .* --skip-tests/);
  assert.doesNotMatch(result.stdout, /\[START\]/);
});

test("stage failure identifies its stage and preserves the underlying cause", () => {
  const cause = new Error("fixture failed");
  assert.throws(
    () =>
      runStage("Integration tests", () => {
        throw cause;
      }),
    (error: unknown) => {
      assert.ok(error instanceof Error);
      assert.match(error.message, /\[FAIL\] Integration tests: fixture failed/);
      assert.equal(error.cause, cause);
      return true;
    },
  );
});
