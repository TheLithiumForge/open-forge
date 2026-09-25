import assert from "node:assert/strict";
import { execFileSync } from "node:child_process";
import { existsSync, mkdirSync, mkdtempSync, readFileSync, readdirSync, rmSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { dirname, join } from "node:path";
import { test } from "node:test";
import { pack } from "../pack.ts";
import { createManifest } from "../manifest.ts";
import { sourceIdentity } from "../source.ts";
import { deliveryDirectory, DevelopmentPublish, hostRuntime, nativeDirectory, suites } from "../layout.ts";
import { inspectPackageContents } from "../package-contents.ts";
import { readPublication } from "../npm/publication.ts";

const version = "0.0.0";
const nativeBytes = "inert native fixture: packaging must never execute this";

test("skip-tests packs an unqualified build without execution, preserves integrity checks and cannot authorize native publication", (context) => {
  const root = mkdtempSync(join(tmpdir(), "open-forge-untested-pack-"));
  context.after(() => rmSync(root, { force: true, recursive: true }));
  const write = (path: string, content: string) => {
    mkdirSync(dirname(join(root, path)), { recursive: true });
    writeFileSync(join(root, path), content);
  };
  write("package.json", JSON.stringify({ version, private: true }));
  write("LICENSE", "owned fixture license\n");
  write("README.md", "# Fixture readme\n");
  const git = (args: string[]) => execFileSync("git", args, { cwd: root, stdio: "pipe" });
  git(["init", "--quiet"]);
  git(["add", "package.json", "LICENSE", "README.md"]);
  git(["-c", "user.name=Fixture", "-c", "user.email=fixture@example.invalid", "-c", "commit.gpgsign=false", "commit", "--quiet", "-m", "Fixture"]);
  const rid = hostRuntime();
  const delivery = deliveryDirectory(rid);
  const native = `${nativeDirectory(rid)}/open-forge/OpenForge.Cli${process.platform === "win32" ? ".exe" : ""}`;
  write(native, nativeBytes);
  write(`${delivery}/build/launcher/npm/open-forge.js`, 'throw new Error("An untested pack must not execute its launcher");');
  write(`${delivery}/build/launcher/package-model.js`, "export {};\n");
  write(`${DevelopmentPublish}/fixture`, "inert development closure");
  const source = sourceIdentity(root);
  const manifest = { ...createManifest(root, { ...source, rid, version }, [native], [`${delivery}/build`, DevelopmentPublish, nativeDirectory(rid)]), ...source, tested: false };
  const manifestPath = `${delivery}/manifest.json`;
  write(manifestPath, JSON.stringify(manifest));
  assert.throws(() => pack(root, rid), /test:built/);
  pack(root, rid, true);
  const output = join(root, delivery, "packages");
  const metadata = JSON.parse(readFileSync(join(output, "package.json"), "utf8"));
  assert.equal(metadata.tested, false);
  assert.equal(JSON.parse(readFileSync(join(root, manifestPath), "utf8")).tested, false);
  assert.equal(existsSync(join(output, "journey")), false, "No npm installation test ran.");
  assert.equal(metadata.files.length, 3);
  for (const tarball of readdirSync(output).filter((name) => name.endsWith(".tgz"))) {
    const contents = inspectPackageContents(output, tarball);
    assert.ok(contents.files.some((file) => file.path === "LICENSE"));
    assert.equal(contents.version, version);
  }
  // Simulate later .NET qualification: skipped npm installation still blocks native publication.
  const reports = `${delivery}/reports`;
  const passedReport = {
    reportFormat: "CTRF",
    results: { summary: { tests: 1, passed: 1, failed: 0, skipped: 0, pending: 0, other: 0 }, tests: [{ status: "passed" }], extra: { suites: [{ errors: [] }] } },
  };
  for (const suite of suites(rid)) write(`${reports}/${suite.name}/results.json`, JSON.stringify(passedReport));
  write(manifestPath, JSON.stringify({ ...manifest, tested: true, reports }));
  assert.throws(() => readPublication(root, "native"), /Untested packages/);
  write(native, "changed native bytes");
  assert.throws(() => pack(root, rid, true), /Changed artifact/);
  write(native, nativeBytes);
  write("LICENSE", "changed source license");
  assert.throws(() => pack(root, rid, true), /Source changed/);
});
