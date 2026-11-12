import assert from "node:assert/strict";
import { spawnSync } from "node:child_process";
import { mkdtempSync, readFileSync, rmSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { join, resolve } from "node:path";
import { AliasPackageName, MainPackageName, PlatformPackages, type SupportedRuntime } from "../package-model.ts";
import { LauncherPath } from "../npm/package-manifests.ts";
import { IsolatedNpm } from "../npm/__tests__/isolated-npm.ts";
import { pythonVersion } from "../pypi/version.ts";
import { wheelName } from "../pypi/wheel-model.ts";
import { findPython } from "./python.ts";

export function smokeAliases(directory: string, runtime: SupportedRuntime, version: string, includeWheel = true): void {
  const root = mkdtempSync(join(tmpdir(), "open-forge-alias-smoke-"));
  const packages = [AliasPackageName, MainPackageName, PlatformPackages[runtime].packageName];
  try {
    const npm = new IsolatedNpm(root);
    // Store Python needs its OS profile when starting the venv interpreter. npm keeps its own isolated home.
    const environment = { ...process.env, OPENFORGE_DATA_HOME: join(root, "data"), PYTHONNOUSERSITE: "1", PIP_CONFIG_FILE: process.platform === "win32" ? "NUL" : "/dev/null" };
    const prefix = join(root, "install");
    const tarballs = packages.map((name) => resolve(directory, `${name.replace(/^@/u, "").replaceAll("/", "-")}-${version}.tgz`));
    npm.run(["install", "--prefix", prefix, ...tarballs]);
    const alias = join(prefix, "node_modules", AliasPackageName);
    const manifest: unknown = JSON.parse(readFileSync(join(alias, "package.json"), "utf8"));
    assert.ok(typeof manifest === "object" && manifest !== null && "dependencies" in manifest);
    assert.deepEqual(manifest.dependencies, { [MainPackageName]: version });
    const run = (executable: string, args: string[]) => {
      const result = spawnSync(executable, args, { cwd: root, env: environment, encoding: "utf8", windowsHide: true });
      assert.equal(result.error, undefined);
      assert.equal(result.status, 0, result.stderr);
      return result.stdout.trim();
    };
    assert.equal(run(process.execPath, [join(alias, LauncherPath), "--version"]), version);
    process.stdout.write(`Alias smoke passed: ${AliasPackageName}@${version} delegates to ${MainPackageName}.\n`);
    if (!includeWheel) return;
    const python = findPython();
    if (python === undefined) {
      process.stdout.write("Wheel smoke not run: Python is unavailable.\n");
      return;
    }
    const venv = join(root, "venv");
    run(python.executable, [...python.args, "-m", "venv", venv]);
    const bin = join(venv, process.platform === "win32" ? "Scripts" : "bin");
    run(join(bin, process.platform === "win32" ? "python.exe" : "python"), [
      "-m",
      "pip",
      "install",
      "--no-index",
      "--no-deps",
      resolve(directory, wheelName(pythonVersion(version), runtime)),
    ]);
    assert.equal(run(join(bin, PlatformPackages[runtime].nativeFileName), ["--version"]), version);
    process.stdout.write(`Wheel smoke passed: ${wheelName(pythonVersion(version), runtime)}.\n`);
    writeFileSync(join(directory, "alias-smoke.json"), `${JSON.stringify({ runtime, version, alias: "passed", wheel: "passed" }, null, 2)}\n`);
  } finally {
    rmSync(root, { recursive: true, force: true });
  }
}
