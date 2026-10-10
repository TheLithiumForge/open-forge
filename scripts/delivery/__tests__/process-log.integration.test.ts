import assert from "node:assert/strict";
import { spawnSync } from "node:child_process";
import { mkdtempSync, readFileSync, rmSync } from "node:fs";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { test } from "node:test";
import { fileURLToPath } from "node:url";
import { LogFlag } from "../log-option.ts";
import { ProcessExit } from "../process.ts";
import { ProcessFixture } from "./process-fixture.ts";

const Cli = fileURLToPath(new URL("../cli.ts", import.meta.url));
const Fixture = fileURLToPath(new URL("fixtures/process-output.ts", import.meta.url));

for (const code of [ProcessExit.success, ProcessFixture.failure]) {
  test(`dispatch logging creates parents, prints both streams and preserves exit ${code}`, (context) => {
    const root = mkdtempSync(join(tmpdir(), "forge-log-"));
    context.after(() => rmSync(root, { recursive: true, force: true }));
    const log = join(root, "nested", "command.log");
    const result = spawnSync(process.execPath, [Cli, "version", "patch", LogFlag, log], {
      encoding: "utf8",
      env: { ...process.env, npm_execpath: Fixture, [ProcessFixture.exitVariable]: String(code) },
    });
    assert.equal(result.status, code, result.stderr);
    assert.ok(result.stdout.includes(ProcessFixture.stdout));
    assert.ok(result.stderr.includes(ProcessFixture.stderr));
    const recorded = readFileSync(log, "utf8");
    assert.ok(recorded.includes(ProcessFixture.stdout));
    assert.ok(recorded.includes(ProcessFixture.stderr));
    if (code !== ProcessExit.success) assert.ok(recorded.includes(`failed (${code})`));
  });
}

test("help output passes through the same logging boundary", (context) => {
  const root = mkdtempSync(join(tmpdir(), "forge-help-log-"));
  context.after(() => rmSync(root, { recursive: true, force: true }));
  const log = join(root, "help.log");
  const result = spawnSync(process.execPath, [Cli, "--help", LogFlag, log], { encoding: "utf8" });
  assert.equal(result.status, ProcessExit.success, result.stderr);
  assert.ok(readFileSync(log, "utf8").includes("Usage: npx forge"));
});
