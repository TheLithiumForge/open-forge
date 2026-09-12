import assert from "node:assert/strict";
import { spawnSync } from "node:child_process";
import { test } from "node:test";
import { join } from "node:path";

for (const [script, args, message] of [
  ["build.ts", ["--rid", "osx-arm64"], "build does not accept --rid"],
  ["build.ts", ["--sha"], "build does not accept --sha"],
  ["test.ts", ["--offline", "--no-restore"], "Choose --offline or --no-restore"],
  ["pack.ts", ["--sha"], "pack does not accept --sha"],
  ["test-built.ts", ["--offline"], "test:built does not accept --offline"],
] as const) {
  test(`unsupported options fail before ${script} executes tools: ${args.join(" ")}`, () => {
    const result = spawnSync(process.execPath, [join(import.meta.dirname, "..", script), ...args], { encoding: "utf8" });
    assert.equal(result.error, undefined);
    assert.equal(result.status, 1);
    assert.ok(result.stderr.includes(message), result.stderr);
    assert.equal(result.stdout, "");
  });
}
