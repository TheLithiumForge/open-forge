import assert from "node:assert/strict";
import { spawnSync } from "node:child_process";
import { copyFileSync, mkdirSync, mkdtempSync, rmSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { dirname, join } from "node:path";
import { test } from "node:test";
import { fileURLToPath } from "node:url";
import { readPackage } from "../package-json.ts";

const cli = fileURLToPath(new URL("../cli.ts", import.meta.url));

test("delivery help and setup help bootstrap without node_modules", (context) => {
  const root = mkdtempSync(join(tmpdir(), "open-forge-delivery-bootstrap-"));
  context.after(() => rmSync(root, { recursive: true, force: true }));
  const scripts = join(root, "scripts/delivery");
  mkdirSync(scripts, { recursive: true });
  const manifest = readPackage(fileURLToPath(new URL("../../../package.json", import.meta.url)));
  writeFileSync(join(root, "package.json"), JSON.stringify({ name: "forge-bootstrap-fixture", version: "0.0.0", type: "module", private: true, bin: manifest["bin"] }));
  for (const file of ["cli.ts", "commands.ts", "options.ts", "process.ts", "repository.ts"])
    copyFileSync(fileURLToPath(new URL(`../${file}`, import.meta.url)), join(scripts, file));
  for (const args of [["--help"], ["setup", "--help"], ["pack", "--help"]]) {
    const result = spawnSync(process.execPath, [join(scripts, "cli.ts"), ...args], { cwd: root, encoding: "utf8" });
    assert.equal(result.status, 0, result.stderr);
    assert.match(result.stdout, /Usage: npx forge/);
  }
  const npm = process.env["npm_execpath"];
  assert.ok(npm, "Run integration tests through npm.");
  const installed = spawnSync(process.execPath, [join(dirname(npm), "npx-cli.js"), "--offline", "--ignore-scripts", "--cache", join(root, "cache"), "forge", "pack", "--help"], {
    cwd: root,
    encoding: "utf8",
  });
  assert.equal(installed.status, 0, installed.stderr);
  assert.match(installed.stdout, /Usage: npx forge pack/);
  assert.match(installed.stdout, /--skip-tests/);
});

test("delivery version forwards to standard npm with Git commit/tag disabled", (context) => {
  const root = mkdtempSync(join(tmpdir(), "open-forge-delivery-version-"));
  context.after(() => rmSync(root, { recursive: true, force: true }));
  const npm = join(root, "npm.mjs");
  writeFileSync(npm, "console.log(JSON.stringify(process.argv.slice(2)));");
  const result = spawnSync(process.execPath, [cli, "version", "prerelease", "--preid", "beta"], { encoding: "utf8", env: { ...process.env, npm_execpath: npm } });
  assert.equal(result.status, 0, result.stderr);
  assert.deepEqual(JSON.parse(result.stdout.trim().split("\n").at(-1) ?? ""), ["version", "prerelease", "--preid", "beta", "--no-git-tag-version"]);
});

test("a failed test stage stops dist before packing and identifies the failure", (context) => {
  const root = mkdtempSync(join(tmpdir(), "open-forge-delivery-failure-"));
  context.after(() => rmSync(root, { recursive: true, force: true }));
  const npm = join(root, "npm.mjs");
  writeFileSync(
    npm,
    `
    const [, command] = process.argv.slice(2);
    console.log("executed:" + command);
    if (command === "test:built") { console.error("owned test fixture failed"); process.exit(3); }
  `,
  );
  const result = spawnSync(process.execPath, [cli, "dist", "--no-restore"], { encoding: "utf8", env: { ...process.env, npm_execpath: npm } });
  assert.equal(result.status, 3);
  assert.match(result.stdout, /executed:build:native/);
  assert.match(result.stdout, /executed:test:built/);
  assert.doesNotMatch(result.stdout, /executed:pack/);
  assert.match(result.stderr, /\[FAIL\] Test managed and native suites on this host/);
  assert.match(result.stderr, /owned test fixture failed/);
});
