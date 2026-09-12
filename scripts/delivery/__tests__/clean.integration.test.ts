import assert from "node:assert/strict";
import { existsSync, mkdirSync, mkdtempSync, readFileSync, rmSync, symlinkSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { dirname, join } from "node:path";
import { test } from "node:test";
import { cleanOutputs } from "../clean.ts";
import { removeOutputs, resetOutput } from "../output.ts";

test("cleanup removes generated outputs and preserves dependencies, links, offline feeds and unrelated evidence", (context) => {
  const root = mkdtempSync(join(tmpdir(), "open-forge-clean-"));
  context.after(() => rmSync(root, { recursive: true, force: true }));
  const removed = [
    "artifacts/bin/file",
    "artifacts/obj/file",
    "artifacts/publish/file",
    "artifacts/delivery/linux-x64/file",
    "artifacts/delivery/osx-arm64-123/file",
    "artifacts/delivery/wrapper/file",
    "artifacts/delivery/managed-reports/file",
  ];
  const retained = [
    "src/file",
    "node_modules/file",
    "artifacts/delivery/offline-feed/file",
    "artifacts/npm/stage/file",
    "artifacts/task-evidence/file",
    "artifacts/delivery/unrelated/file",
  ];
  for (const path of [...removed, ...retained]) {
    mkdirSync(dirname(join(root, path)), { recursive: true });
    writeFileSync(join(root, path), path);
  }
  cleanOutputs(root);
  cleanOutputs(root);
  for (const path of removed) assert.equal(existsSync(join(root, path)), false, path);
  for (const path of retained) assert.equal(readFileSync(join(root, path), "utf8"), path);
});

test("cleanup rejects an output symlink before deleting other owned outputs", (context) => {
  const root = mkdtempSync(join(tmpdir(), "open-forge-clean-link-"));
  context.after(() => rmSync(root, { recursive: true, force: true }));
  mkdirSync(join(root, "external"));
  mkdirSync(join(root, "artifacts/bin"), { recursive: true });
  writeFileSync(join(root, "external/keep"), "external");
  writeFileSync(join(root, "artifacts/bin/keep"), "owned");
  symlinkSync(join(root, "external"), join(root, "artifacts/publish"), process.platform === "win32" ? "junction" : "dir");
  assert.throws(() => cleanOutputs(root), /symbolic link/u);
  assert.equal(readFileSync(join(root, "external/keep"), "utf8"), "external");
  assert.equal(readFileSync(join(root, "artifacts/bin/keep"), "utf8"), "owned");
  assert.throws(() => removeOutputs(root, ["artifacts/../external"]));
});

test("replacing an output discards stale packages and success markers", (context) => {
  const root = mkdtempSync(join(tmpdir(), "open-forge-reset-"));
  context.after(() => rmSync(root, { recursive: true, force: true }));
  const target = resetOutput(root, "artifacts/delivery/wrapper");
  writeFileSync(join(target, "manifest.json"), "old success");
  writeFileSync(join(target, "old.tgz"), "old package");
  assert.equal(resetOutput(root, "artifacts/delivery/wrapper"), target);
  assert.equal(existsSync(join(target, "manifest.json")), false);
  assert.equal(existsSync(join(target, "old.tgz")), false);
});
