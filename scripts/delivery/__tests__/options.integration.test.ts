import assert from "node:assert/strict";
import { spawnSync } from "node:child_process";
import { test } from "node:test";
import { join } from "node:path";

for (const [script, args, message] of [
  ["build.ts", ["--rid", "osx-arm64"], "--rid applies to native artifacts only"],
  ["build.ts", ["--sha"], "--sha applies to native builds only"],
  ["test.ts", ["--offline", "--no-restore"], "Choose --offline or --no-restore"],
  ["pack.ts", ["--sha"], "--sha applies to native builds only"],
  ["test-built.ts", ["--offline"], "Artifact consumers do not restore"],
] as const) {
  test(`unsupported options fail before ${script} executes tools: ${args.join(" ")}`, () => {
    const result = spawnSync(process.execPath, [join(import.meta.dirname, "..", script), ...args], { encoding: "utf8" });
    assert.equal(result.error, undefined);
    assert.equal(result.status, 1);
    assert.ok(result.stderr.includes(message), result.stderr);
    assert.equal(result.stdout, "");
  });
}
